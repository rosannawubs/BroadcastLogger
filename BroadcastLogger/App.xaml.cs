using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace BroadcastLogger
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private bool loggedIn = false;
        private bool recording = false;
        private string authCode;
        private string stationID;
        private string device;
        /// <summary>
        /// Manage the startup arguments. 
        /// Available args
        /// loggedIn
        /// authCode-SampleAuthCode
        /// stationID-StationID
        /// recording
        /// device-Microphone(realtek audio)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">the args</param>
        void App_Startup(object sender, StartupEventArgs e)
        {
            for (int i = 0; i < e.Args.Length; i++)
            {
                string[] args = e.Args[i].Split('-');
                switch (args[0])//The key 
                {
                    case "loggedIn" :
                        loggedIn = true;
                        break;
                    case "authCode" :
                        authCode = args[1];
                        break;
                    case "stationID" :
                        stationID = args[1];
                        break;
                    case "recording" :
                        recording = true;
                        break;
                    case "device" :
                        device = args[1];
                        break;
                }
            }
            MainWindow mainWindow = new MainWindow(loggedIn, stationID, authCode, recording, device);
            mainWindow.Show();
        }
    }
}
