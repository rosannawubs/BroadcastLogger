using System;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
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
using System.Threading;
using BroadcastLoggerLib.Handlers;
using BroadcastLoggerLib.Misc;
using BroadcastLoggerLib;

namespace BroadcastLogger
{
    /// <summary>
    /// Interaction logic for login.xaml
    /// </summary>
    public partial class Login : UserControl
    {
        private string[] devices;
        private static Metering meterController;
        private readonly SynchronizationContext synchronizationContext;
        NAudioHandler nAudioHandler;
        private bool loggedIn = false;
        private string selectedDevice;
        private bool recording;
        /// <summary>
        /// Instantiate a new login screen
        /// If you want a default instantiation set 
        /// loggedIn, and recording as false, and pass 
        /// null in for everything else. 
        /// </summary>
        /// <param name="loggedIn">Boolean value for recovery </param>
        /// <param name="stationID">StatationID to recover back to where we were previously</param>
        /// <param name="authCode">Authorization code for recovery</param>
        /// <param name="recording">Boolean value to determin if we were recording before</param>
        /// <param name="device">Device we were recordingo on.</param>
        public Login(bool loggedIn, string stationID, string authCode, bool recording, string device)
        {
            InitializeComponent();
            synchronizationContext =     SynchronizationContext.Current;
            nAudioHandler = new NAudioHandler();
            this.loggedIn = loggedIn;
            //If the application is in recovery, fill in the details
            //as the previous seesion
            if (loggedIn == true)
            {
                if (stationID != null)
                    this.stationID.Text = stationID;
                if (authCode != null)
                    this.Auth_Code.Text = authCode;
                if (!string.IsNullOrEmpty(device))
                    this.selectedDevice = device;
                this.recording = recording;
            }
            FFmpegGetDevices();
        }

        #region initalization
        /// <summary>
        /// Takes in an array of devices and updatees the dropdown box.
        /// </summary>
        /// <param name="devices">An array of devices using the NAudio MMdevice object</param>
        private void updateDevices(MMDevice[] devices)
        {
            string[] stringDevices = new string[devices.Length];
            this.devices = new string[devices.Length];
            int count = 0;
            foreach (MMDevice device in devices)
            {
                inputDevices.Items.Add(device.ToString());
                this.devices[count++] = device.ToString();
            }

        }
        private void updateDevices(string[] devices)
        {
            this.devices = devices;
            foreach (string d in devices)
            {
                inputDevices.Items.Add(d);
            }
            //If selectedDevice is not set then we know we
            //are not in recovery, therefore we wait for used input.
            if (string.IsNullOrEmpty(this.selectedDevice))
            {
                setDefaultDevice();
            }
            else
            {

                this.inputDevices.SelectedIndex = this.inputDevices.Items.IndexOf(this.selectedDevice);
                this.loginClick(null, null);
            }
                
        }
        private void setDefaultDirectDevice()
        {
            NAudioHandler nAudioHandler = new NAudioHandler();
            String defaultDevice = nAudioHandler.getDefaultDirectDevice();
            if (defaultDevice != null)
            {
                inputDevices.SelectedIndex = inputDevices.Items.IndexOf(defaultDevice);
            }
        }
        private void getDevices()
        {
            MMDevice[] devices = nAudioHandler.getDevices();
            //this.devices = nAudioHandler.getDirectDevices();
            updateDevices(devices);

        }
        private void FFmpegGetDevices()
        {
            FFmpegHandler handler = new FFmpegHandler("ffmpeg", "ffmpeg.exe");
            handler.getInputDevices(updateDevices);
        }
        private void setDefaultDevice()
        {
            MMDevice defaultDevice = nAudioHandler.getDefaultDevice();
            if (defaultDevice != null)
            {
                string defaultDeviceName = defaultDevice.ToString();
                foreach (string s in inputDevices.Items)
                {
                    for (int i = 0; i < s.Length; i++)
                    {
                        if (i == s.Length - 1 && s[i] == defaultDeviceName[i])
                            inputDevices.SelectedIndex = inputDevices.Items.IndexOf(defaultDeviceName.Substring(0, s.Length));
                        if (s[i] != defaultDeviceName[i])
                            continue;
                    }
                }
            }
            else
            {
                inputDevices.SelectedItem = null;
            }
        }
        #endregion
        #region FormChanged
        private async void loginClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(stationID.Text);
            Console.WriteLine(Auth_Code.Text);
            Manager.Instance.StationId = stationID.Text;
            Manager.Instance.AuthCode = Auth_Code.Text;
            //Manager.Instance.timeLeft = uint.Parse(timeLeftInput.Text);

            if (inputDevices.SelectedItem == null)
            {
                statusLabel.Content = "Please select an audio device";
                return;
            }

            Task<bool> status = Manager.Instance.requestAuthorization();
            statusLabel.Content = "Authorizing...";

            await status;

            if (status.Result)
            {
                // Updating label doesn't work for some reason.
                statusLabel.Content = "Success! Loading Logger...";
                Manager.Instance.Initialization();
                //Used for recovery to keep track of our current state. 
                MainWindow.loggedIn = true;
                MainWindow.stationID = stationID.Text;
                MainWindow.authCode = Auth_Code.Text;
                MainWindow.device = this.selectedDevice;
                Console.WriteLine("Selected device: " + this.selectedDevice);
                //Task task = Task.Factory.StartNew(() => Manager.Instance.handleQueue());
                switcher.Switch(new Logger(devices, inputDevices.SelectedItem.ToString(), recording, Auth_Code.Text));
            }
            else
            {
                statusLabel.Content = "Login failed.";
            }
        }

        /// <summary>
        /// When a user changes the device we want to change the device the meter is associating with.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void inputDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = ((ComboBox)sender).SelectedItem;
            if (selectedItem == null)
                return;
            this.selectedDevice = selectedItem.ToString();
            Console.WriteLine(selectedDevice);
            if (meterController != null)
            {
                meterController.Dispose();
            }
            try
            {
                meterController = new Metering(selectedDevice);
                meterController.DeviceUpdated += meter_DeviceUpdated;
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show("Could not capture this device", "Error");
                Console.WriteLine(ex.StackTrace);
                setDefaultDevice();
            }
        }
        #endregion
        #region UpdatingUserInterface
        /// <summary>
        /// Updating the meter based on the device.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void meter_DeviceUpdated(object sender, Metering.DeviceVolume e)
        {
            try
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    meter.updateBar(e.Volume);
                }));
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Metering was interupted");
                meterController.Dispose();
            }
        }
        #endregion


    }
}
