public class Config
{
    public int NumberOfGPUS { get; set; }
    public double Delay { get; set; }
    public int RestartMiner { get; set; }
    public int ComputerStrikes { get; set; }
    public int MinerStrikes { get; set; }
    public string RestartBat { get; set; }
    public string StartBat { get; set; }
    public string ToEmailAddress { get; set; }
    public string FromEmailAddress { get; set; }
    public string FromEmailPassword { get; set; }
    public string ProdOrTest { get; set; }
    public string SendEmail { get; set; }
    public string Restart { get; set; }
    public string Rig { get; set; }
    public string TestUrl{ get; set;}
    public int BadCardThreshold{get; set; }
    public string ScreenShotApp { get; set; }
    public string ScreenShotPath { get; set; }
    public Config() { }
    public Config(int numberofgpus, double delay, int computerstrikes, int minerstrikes, string restartbat, string startbat, string toemailaddress,
        string fromemailaddress, string fromemailpassword, string prodortest, string sendemail, string restart, string rig, string testurl, int badcardthreshold,
        string screenshotapp, string screenshotpath)
    {
        NumberOfGPUS = numberofgpus;
        Delay = delay;
        ComputerStrikes = computerstrikes;
        MinerStrikes = minerstrikes;
        RestartBat = restartbat;
        StartBat = startbat;
        ToEmailAddress = toemailaddress;
        FromEmailAddress = fromemailaddress;
        FromEmailPassword = fromemailpassword;
        ProdOrTest = prodortest;
        SendEmail = sendemail;
        Restart = restart;
        Rig = rig;
        TestUrl = testurl;
        BadCardThreshold = badcardthreshold;
        ScreenShotApp = screenshotapp;
        ScreenShotPath = screenshotpath;
    }
    //Other properties, methods, events...
}