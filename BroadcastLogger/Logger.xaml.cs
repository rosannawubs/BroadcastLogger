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
using BroadcastLoggerLib;
using BroadcastLoggerLib.Handlers;
using BroadcastLoggerLib.Misc;
using NAudio.CoreAudioApi;

namespace BroadcastLogger
{
    /// <summary>
    /// Interaction logic for Logger.xaml
    /// </summary>
    public partial class Logger : UserControl
    {
        // Testing purposes-
        // Enable/Disable Recording with Manager
        FFmpegHandler ffh;
        Metering meterController;
        public string selectedDevice_;
        /// <summary>
        /// Constructor to create the logging user interface.
        /// </summary>
        /// <param name="devices">A list of devices to be shown in the dropdown box</param>
        /// <param name="selectedDevice">The selected device from the previous page.</param>
        /// <param name="recording">A parameter used for recovery, to start recording automatically pass in true</param>
        /// <param name="authCode">The authcode to be used for recording. </param>
        public Logger(string[] devices, string selectedDevice, bool recording, String authCode)
        {
            InitializeComponent();
            authCodeLabel.Content = "Enter auth code for " + Manager.Instance.StationId + ":";
            authCodeTextBox.Text = authCode;
            updateDevices(devices, selectedDevice);
            this.selectedDevice_ = selectedDevice;
            ffh = new FFmpegHandler("ffmpeg", "ffmpeg.exe");
            initalizeMeter();
            Manager.Instance.setDevice(selectedDevice_);

            // Get status update messages from Manager
            Manager.Instance.StatusUpdate += ((e) =>
            {
                this.Dispatcher.Invoke((Action)(() => { 
                    statusLabel.Content = e;
                }));
            });
            // Notify when Manager finishes handling queue.
            Manager.Instance.RecordingStopped += (() => {
                this.Dispatcher.Invoke((Action)(() => {
                    progressBar.IsIndeterminate = false;
                    startStopButton.IsEnabled = true;
                }));
            });
            //Begin recording if the previous session was still recording. 
            if (recording == true)
            {
                beginRecording(null, null);
                startStopButton.SetStop();
            }
        }
        private void updateDevices(string[] devices, string selectedDevice)
        {
            foreach (string s in devices)
            {
                inputDevices.Items.Add(s);
            }
            if(selectedDevice != null)
            inputDevices.SelectedIndex = inputDevices.Items.IndexOf(selectedDevice);
        }

        private void inputDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.selectedDevice_ = ((ComboBox)sender).SelectedItem.ToString();
            Manager.Instance.setDevice(selectedDevice_);
            initalizeMeter();
        }

        private void initalizeMeter()
        {
            if (selectedDevice_ != null)
            {
                if (meterController != null)
                {
                    meterController.Dispose();
                }
                try
                {
                    meterController = new Metering(selectedDevice_);
                    meterController.DeviceUpdated += meterController_DeviceUpdated;
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show("Could not capture this device", "Error");
                    //setDefaultDirectDevice();
                    setDefaultDevice();
                }
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
        private void setDefaultDevice()
        {

            NAudioHandler nAudioHandler = new NAudioHandler();
            MMDevice defaultDevice = nAudioHandler.getDefaultDevice();
            if (defaultDevice != null)
            {
                inputDevices.SelectedIndex = inputDevices.Items.IndexOf(defaultDevice.ToString());
            }
        }
        void meterController_DeviceUpdated(object sender, Metering.DeviceVolume e)
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


        private async void beginRecording(object sender, RoutedEventArgs e)
        {
            MainWindow.recording = true;
            progressBar.IsIndeterminate = true;
            Manager.Instance.recordStop = false;
            await Manager.Instance.recordAsync();
            Manager.Instance.startTimer();
        }

        private void Stopbutton_Click(object sender, RoutedEventArgs e) {
            MainWindow.recording = false;
            Manager.Instance.recordStop = true;
            startStopButton.IsEnabled = false;
            statusLabel.Content = "Stopping...";
        }
        private void StartStopButton_ButtonClicked(object sender, StartStopArgs e)
        {
            if (e.Status == status.Start)
            {
                this.Stopbutton_Click(null, null);
            }
            else
            {
                this.beginRecording(null, null);
            }
        }

        private void changeAuthCode(object sender, TextChangedEventArgs e) {
            Manager.Instance.AuthCode = authCodeTextBox.Text.ToString();
        }
    }
}
