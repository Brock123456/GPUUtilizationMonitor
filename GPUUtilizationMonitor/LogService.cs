using System;
using System.IO;
using System.Linq;

namespace LogService
{
    public class LogClass
    {
        public void Log(string logMessage)
        {
            StreamWriter w = File.AppendText(DateTime.Now.ToString("yyyyMMdd") + ".log");
            w.Write("Log Entry : ");
            w.WriteLine("{0}", DateTime.Now.ToString("MM/dd/yyyy h:mm:ss tt"));
            w.WriteLine(logMessage);
            w.WriteLine("-------------------------------");
            w.Close();
            w.Dispose();
            Console.Write("\r\n" + DateTime.Now.ToString("MM/dd/yyyy h:mm:ss tt") + " - " + logMessage);
        }

        public void DumpLog()
        {
            string line;
            StreamReader r = File.OpenText("log.txt");
            while ((line = r.ReadLine()) != null)
            {
                Console.WriteLine(line);
            }
            r.Close();
            r.Dispose();
        }
        public void DeleteOldLogs(int daysToKeep)
        {
            DateTime dt;
            //Delete old screen shots
            DirectoryInfo dir = new DirectoryInfo(@Directory.GetCurrentDirectory());
            FileInfo[] files = dir.GetFiles("*.log")
                                 .Where(p => p.Extension == ".log").ToArray();
            foreach (FileInfo file in files)
                try
                {
                    dt = File.GetLastWriteTime(file.FullName);
                    if (dt < DateTime.Now.AddDays(daysToKeep * -1))
                    {
                        file.Attributes = FileAttributes.Normal;
                        File.Delete(file.FullName);
                    }
                }
                catch (Exception e)
                {
                    Log("Exception deleting old logs - carring on - " + e);
                }
        }
        public string returnEvents(int numberOfEvents)
        {
            try
            {
                int count = 0;
                string strWork = "";
                string strLogFile = Directory.GetCurrentDirectory() + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".log";
                using (var reader = new StreamReader(strLogFile))
                {
                    if (reader.BaseStream.Length > 1024)
                    {
                        reader.BaseStream.Seek(-1024, SeekOrigin.End);
                    }
                    string line;
                    while ((line = reader.ReadLine()) != null && count <= numberOfEvents)
                    {
                        Console.WriteLine(line);
                        strWork = strWork + line + "\r\n";
                        if (line.Contains("---------------------"))
                        {
                            count++;
                        }
                    }
                }
                return strWork;
            }catch
            {
                return "";
            }
        }
    }
}