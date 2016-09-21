using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BroadcastLogger;
using System.Net;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using BroadcastLoggerLib;
using NAudio.Wave;
using BroadcastLoggerLib.Handlers;
using BroadcastLoggerLib.Misc;
using System.Threading;
using System.Diagnostics;
using MS.WindowsAPICodePack.Internal;
using System.Text.RegularExpressions;
using BCLLib.Misc;
namespace Test3
{
    class Program
    {
        static void Main(string[] args)
        {
            
            Console.WriteLine("TestComplete");
            Console.ReadKey();
        }   
     }
    class Worker
    {
        private string name;
        public void DoWork()
        {
            for (int i = 0; i < 100; ++i)
            {
                LogWriter.Write(name + "  " + i.ToString());
            }
        }
        public Worker(string name)
        {
            this.name = name;
        }
    }
}
