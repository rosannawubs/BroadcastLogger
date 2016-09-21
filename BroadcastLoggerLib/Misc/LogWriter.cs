using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCLLib.Misc
{
    public static class LogWriter
    {
        private static string workingDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static StreamWriter writer;
        private static string fileName;
        //used to sync the threads
        private static object sync = new object();
        static LogWriter() 
        {
            workingDir = Path.Combine(workingDir, "BroadcastLogger");
            workingDir = Path.Combine(workingDir, "Logs");
            
        }
        public static void Write(string message)
        {
            if (!Directory.Exists(workingDir))
                Directory.CreateDirectory(workingDir);
            UpdateFileName();
            string target = Path.Combine(workingDir, fileName);
            lock (sync)
            {
                using (writer = File.AppendText(target))
                {
                    writer.WriteLine("****************************");
                    writer.WriteLine(DateTime.Now);
                    writer.WriteLine(message+"\n\n\n");
                    writer.WriteLine("****************************");
                    writer.Flush();
                }
            }
        }
        private static void UpdateFileName() 
        {
            fileName = DateTime.Now.ToString("d MMM yyyy") + ".txt"; 
        }
    }
}
