using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using EmailService;
using LogService;
using System.Net;
using System.Configuration;

class Program
{
    public static Config objConfig = new Config(); //Public config obj easier then passing her around
    public static int intTestCount = 0; //Just a global variable for counting test loops please ignore my poor coding standards
    static void Main()
    {
        //Declaring and initilizing some stuff probaly more stuff then i need
        EmailClass emailClass = new EmailClass();
        LogClass logClass = new LogClass();
        double dblCountDown;
        string[] strUtilization;
        int intStrikes = 0;
        int intMissing = 0;
        bool bReboot = false;
        bool bAddStrike = false;
        string strMsg = "";
        //Get config values from the config file
        getConfig();
        logClass.Log("Starting process assuming reboot");
        strMsg = DateTime.Now.ToString("MM/dd/yyyy h:mm:ss tt") + " - Starting process assuming reboot";
        //Give user time to read
        Thread.Sleep(2000);
        //Send Email starting up
        if (objConfig.SendEmail != "no")
        {
            logClass.Log("\r\nSending Email");
            strMsg = strMsg + "\r\n" + DateTime.Now.ToString("MM/dd/yyyy h:mm:ss tt") + " - Sending Email";
            emailClass.SendEmail(objConfig.ToEmailAddress, "GPU Utilization Monitor - " + objConfig.Rig + "Monitoring is starting", "Monitoring is starting", objConfig.FromEmailAddress, objConfig.FromEmailPassword);
        }
        //Delay checks to give the computer time to get going. Added fancy count down.
        Console.Clear();
        dblCountDown = objConfig.Delay;
        while (dblCountDown > 0)
        {
            Console.Write(strMsg + "\r\nStarting GPU utilization monitor in " + objConfig.Delay / 60000 + " minutes! Exit now if there are other issues! \r\n");
            Console.Write(dblCountDown / 1000 + " second remaning. \r\n");
            Thread.Sleep(1000);
            dblCountDown = dblCountDown - 1000;
            Console.Clear();
        }
        //If startup bat present run it
        if (objConfig.StartBat != "")
        {
            logClass.Log("\r\nStarting miner command - " + objConfig.StartBat);
            ExecuteCommand(objConfig.StartBat);
        }
        if (objConfig.ProdOrTest == "test")
        {
            logClass.Log("\r\nUsing Test Data");
        }
        //Loop till something breaks
        while (!bReboot)
        {
            //Clear message for every loop
            strMsg = "";
            //Decide if we are using real values or test values
            if (objConfig.ProdOrTest != "test")
            {
                strUtilization = getUtilization();
            }
            else
            {
                strUtilization = testgetUtilization();
            }
            //Convert the string utilizations into ints so we can do some math.
            int[] intUtilization = Array.ConvertAll(strUtilization, delegate (string s) { return int.Parse(s); });
            //Make sure all cards are utilized above threshold
            if (checkUtilization(intUtilization, ref strMsg)) { bAddStrike = true; }
            //Make sure all cards are present don't bother if utilization already counts as a strike
            else if (intUtilization.Length < objConfig.NumberOfGPUS)
            {
                bAddStrike = true;
                intMissing = objConfig.NumberOfGPUS - intUtilization.Length;
                strMsg = strMsg + "\r\nThe Ugly - " + intMissing + " GPUs are missing \r\n";
            }
            //Clean run new batter clear the board
            else { intStrikes = 0; }
            //If Error Log int
            if (strMsg.Contains("Bad") || strMsg.Contains("Ugly")) { logClass.Log(strMsg); }
            else { Console.Write("\r\n" + DateTime.Now.ToString("MM/dd/yyyy h:mm:ss tt") + " - " + strMsg); }
            //If error and internet is up count it as strike
            if (bAddStrike)
            {
                //If testing don't ping a web page a bunch
                if (objConfig.ProdOrTest != "test")
                {
                    //Check for internet connection before looking for strikes. If not internet/pool then no need to try anything.
                    if (CheckForInternetConnection()) { intStrikes++; }
                }
                else
                {
                    intStrikes++;
                }
            }
            //Check if we need attempt to restart computer
            if (intStrikes >= objConfig.ComputerStrikes) { bReboot = true; }
            //Check if we need to attempt to restart miner 
            else if (intStrikes >= objConfig.MinerStrikes)
            {
                //If restart bat present execute it
                if (objConfig.RestartBat != "")
                {
                    logClass.Log("\r\nRestarting miner command - " + objConfig.StartBat);
                    ExecuteCommand(objConfig.RestartBat);
                }
            }

            //sleep for the set delay
            dblCountDown = objConfig.Delay;
            while (dblCountDown > 0)
            {
                Thread.Sleep(1000);
                dblCountDown = dblCountDown - 1000;
            }
        }
        //Send Email something went wrong
        if (objConfig.SendEmail != "no")
        {
            logClass.Log("\r\nSending Email");
            emailClass.SendEmail(objConfig.ToEmailAddress, "GPU Utilization Monitor - " + objConfig.Rig + "struck out", strMsg, objConfig.FromEmailAddress, objConfig.FromEmailPassword);
        }
        //Well crap we made it out time to reboot
        if (objConfig.Restart != "no")
        {
            //Send Email something went wrong
            if (objConfig.SendEmail != "no")
            {
                logClass.Log("\r\nSending Email");
                emailClass.SendEmail(objConfig.ToEmailAddress, "GPU Utilization Monitor - " + objConfig.Rig + "struck out rebooting", "Rebooting\r\n" + strMsg, objConfig.FromEmailAddress, objConfig.FromEmailPassword);
            }
            logClass.Log("\r\nAttemping reboot with force");
            strMsg = "-r -f -t 60 -c \"" + strMsg + "\"";
            System.Diagnostics.Process.Start("shutdown.exe", strMsg);
        }
        else
        {
            logClass.Log("\r\nComputer struck out but reboot is disabled.");
            if (objConfig.SendEmail != "no")
            {
                emailClass.SendEmail(objConfig.ToEmailAddress, "GPU Utilization Monitor - " + objConfig.Rig + "struck out", "Reboot is diabled taking no farther action", objConfig.FromEmailAddress, objConfig.FromEmailPassword);
            }
        }
    }
    //Check if utilixation is abouve the threashold
    public static bool checkUtilization(int[] lintUtilization, ref string strMessage)
    {
        //Declaring some stuff
        int intCounter = 0;
        string strBad = "";
        string strGood = "";
        //Loop through utilizations to check for issues and to build the message.
        foreach (int i in lintUtilization)
        {
            if (i < 90)
            {
                strBad = strBad + " " + i + "%";
            }
            else
            {
                strGood = strGood + " " + i + "%";
            }
            intCounter++;
        }
        //No GPUs made the good list
        if (strGood == "") { strGood = "No good GPUS."; }
        //If any bad gpus build message and return the strike
        if (strBad != "") { strMessage = strMessage + "\r\nThe Bad - " + strBad + "\r\nThe good - " + strGood; return true; }
        //No bad gpus build message accordingly
        else { strMessage = strMessage + "\r\nThe Good - " + strGood; return false; }
    }
    //Return some fake utilization for a bit then fail.
    public static string[] testgetUtilization()
    {
        //Test block no farther comments needed not like we would ever test anything anyways.
        string[] array = new string[objConfig.NumberOfGPUS];
        if (intTestCount < 3)
        {
            for (int i = 0; i < objConfig.NumberOfGPUS; i++)
            {
                array[i] = "99";
            }
        }
        else
        {
            for (int i = 0; i < objConfig.NumberOfGPUS; i++)
            {
                array[i] = "50";
            }
        }

        intTestCount++;
        return array;
    }
    //Copy pasted from somewhere lost source but why it looks way nicer then my code
    public static string[] getUtilization()
    {   //
        // Setup the process with the ProcessStartInfo class.
        //
        string[] lines;
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = @"""C:\Program Files\NVIDIA Corporation\NVSMI\nvidia-smi.exe"""; // Specify exe name.
        start.Arguments = " --query-gpu=utilization.gpu --format=csv";
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;
        //
        // Start the process.
        //
        using (Process process = Process.Start(start))
        {
            //
            // Read in all the text from the process with the StreamReader.
            //
            using (StreamReader reader = process.StandardOutput)
            {
                string result = reader.ReadToEnd();
                result = result.Replace("utilization.gpu [%]", String.Empty);
                result = result.Replace(" ", String.Empty);
                result = result.Replace("%", String.Empty);
                result = result.Trim();
                result = result.TrimEnd();
                lines = result.Split(
                new[] { Environment.NewLine },
                StringSplitOptions.None);
            }
        }
        return lines;
    }
    //Reads values from the file and puts them in the global object
    public static void getConfig()
    {
        string strNumberOfGPUS = ConfigurationManager.AppSettings.Get("NumberOfGPUS");
        string strDelay = ConfigurationManager.AppSettings.Get("Delay");
        string strToEmailAddress = ConfigurationManager.AppSettings.Get("ToEmailAddress");
        string strFromEmailAddress = ConfigurationManager.AppSettings.Get("FromEmailAddress");
        string strFromEmailPassword = ConfigurationManager.AppSettings.Get("FromEmailPassword");
        string strProdOrTest = ConfigurationManager.AppSettings.Get("ProdOrTest");
        string strSendEmail = ConfigurationManager.AppSettings.Get("SendEmail");
        string strComputerStrikes = ConfigurationManager.AppSettings.Get("ComputerStrikes");
        string strMinerStrikes = ConfigurationManager.AppSettings.Get("MinerStrikes");
        string strRestartBat = ConfigurationManager.AppSettings.Get("RestartBat");
        string strStartBat = ConfigurationManager.AppSettings.Get("StartBat");
        string strRestart = ConfigurationManager.AppSettings.Get("Restart");
        string strRig = ConfigurationManager.AppSettings.Get("Rig");
        string strTestUrl = ConfigurationManager.AppSettings.Get("TestUrl");


        objConfig.NumberOfGPUS = Int32.Parse(strNumberOfGPUS);
        objConfig.Delay = double.Parse(strDelay) * 60000;
        objConfig.ComputerStrikes = Int32.Parse(strComputerStrikes);
        //If 0 then disabled set so high it doesn't matter
        if (objConfig.ComputerStrikes == 0) { objConfig.ComputerStrikes = 100000; }
        objConfig.MinerStrikes = Int32.Parse(strMinerStrikes);
        //If 0 then disabled set so high it doesn't matter
        if (objConfig.MinerStrikes == 0) { objConfig.MinerStrikes = 100000; }
        objConfig.ToEmailAddress = strToEmailAddress;
        objConfig.FromEmailAddress = strFromEmailAddress;
        objConfig.FromEmailPassword = strFromEmailPassword;
        objConfig.SendEmail = strSendEmail;
        objConfig.RestartBat = strRestartBat;
        objConfig.StartBat = strStartBat;
        objConfig.Restart = strRestart;
        objConfig.ProdOrTest = strProdOrTest;
        objConfig.Rig = strRig;
        objConfig.TestUrl = strTestUrl;
    }
    static void ExecuteCommand(string command)
    {
        //Execute in new CMD window incase of error or hang
        try
        {
            double dblCountDown = 0;
            //Setup command
            ProcessStartInfo processInfo;
            Process process;
            processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = true;
            //Execute command
            process = Process.Start(processInfo);
            //Don't want to wait for exit incase bat hangs give it two minutes then carry on
            dblCountDown = objConfig.Delay;
            Console.Write(" \r\nWaiting for bat to finish " + objConfig.Delay / 60000 + " minutes! Exit now if there are other issues! \r\n");
            Console.Write(dblCountDown / 1000 + " second remaning. \r\n");
            while (dblCountDown > 0)
            {
                Thread.Sleep(1000);
                dblCountDown = dblCountDown - 1000;
            }
            process.Close();
        }
        catch (Exception e)
        {
            LogClass logClass = new LogClass();
            logClass.Log("Error executing - " + command + "\r\nError - " + e);
        }
    }
    public static bool CheckForInternetConnection()
    {
        try
        {
            //If no URL just return true
            if (objConfig.TestUrl != "")
            {
                using (var client = new WebClient())
                {
                    using (client.OpenRead(objConfig.TestUrl))
                    {
                        return true;
                    }
                }
            }
            else
            {
                return true;
            }
        }
        //If error return false
        catch
        {
            return false;
        }
    }
}
