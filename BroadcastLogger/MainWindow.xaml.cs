using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BroadcastLoggerLib.Handlers;
using BroadcastLoggerLib.Misc;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using MS.WindowsAPICodePack.Internal;
using Microsoft.WindowsAPICodePack.ApplicationServices;

namespace BroadcastLogger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon notifyIcon;
        public static bool loggedIn = false;
        public static string stationID = string.Empty;
        public static string authCode = string.Empty;
        public static bool recording = false;
        public static string device = string.Empty; 
        /// <summary>
        /// Main window that is shared between all of the different views. 
        /// The parameters are used for the recovery of the application in
        /// the event of a failure.
        /// For default instantiation
        /// set loggedIn, and recording as false.
        /// Set everything else as null. 
        /// </summary>
        /// <param name="loggedIn">Used for recovery, boolean value for if the previous entry was logged in</param>
        /// <param name="stationID">Used for recovery, that stationID of the previous session</param>
        /// <param name="authCode">Used for recovery, the authcode of the previous session</param>
        /// <param name="recording">Used for recovery, a boolean value to see if the previous seesion was recording</param>
        /// <param name="device">Used for recovery, the device that we were using to record audio. </param>
        public MainWindow(bool loggedIn, string stationID, string authCode, bool recording, string device)
        {
            InitializeComponent();
#if !DEBUG
            //Restart the application if it crashes.
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#endif
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = BroadcastLogger.Properties.Resources.icon;
            notifyIcon.MouseDoubleClick += notifyIcon_MouseDoubleClick;
            notifyIcon.ContextMenu = CreateMenu();
            notifyIcon.Visible = true;
            switcher.mainWindow = this;
            //Start the login screen with the given args.(Args are used for recovery 
            //so we can get back to where we were automatically
            switcher.Switch(new Login(loggedIn, stationID, authCode, recording, device));
            
            
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("Exception was not handled");
            string arguments = string.Empty;
            if (loggedIn == true)
                arguments = "loggedIn ";
            arguments += "\"authCode-" + authCode + "\" ";
            arguments += "\"stationID-"+stationID+"\" ";
            if (recording == true)
                arguments += "recording ";
            arguments += "\"device-" + device + "\"";
            Console.WriteLine(arguments);
            Process.Start(
                System.Reflection.Assembly.GetEntryAssembly().Location, arguments);
            Environment.Exit(1);
        }
        void notifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!this.IsVisible)
            {
                this.Show();
            }

            if (this.WindowState == WindowState.Minimized)
            {
                this.WindowState = WindowState.Normal;
            }

            this.Activate();
            this.Topmost = true;  // important
            this.Topmost = false; // important
            this.Focus();         // important
        }
        private System.Windows.Forms.ContextMenu CreateMenu()
        {
            System.Windows.Forms.ContextMenu menu = new System.Windows.Forms.ContextMenu();
            System.Windows.Forms.MenuItem item = new System.Windows.Forms.MenuItem();
            item.Index = 0;
            item.Text = "Exit";
            item.Click += (obj, args) => {
                System.Windows.Application.Current.Shutdown();
            };
            menu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[]{
              item
            });
            return menu;
        }
        public void Navigate(System.Windows.Controls.UserControl nextPage)
        {
            this.Content = nextPage;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.WindowState = WindowState.Minimized;

        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                notifyIcon.Visible = true;
                notifyIcon.BalloonTipTitle = "I'm Still Here";
                notifyIcon.BalloonTipText = "Right click me to close the application";
                notifyIcon.ShowBalloonTip(500);
                this.Hide();
            }
        }

    }
}
