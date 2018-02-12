using System;
using System.IO;

namespace LogService
{
    public class LogClass
    {
        public void Log(string logMessage)
        {
            StreamWriter w = File.AppendText("log.txt");
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
    }
}