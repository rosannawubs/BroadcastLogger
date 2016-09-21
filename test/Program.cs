using BroadcastLogger;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test
{
    class Program
    {
        public static MMDevice getDevice(string device)
        {
            NAudioHandler handler = new NAudioHandler();
            MMDevice[] devices = handler.getDevices();
            foreach (MMDevice x in devices)
            {
                if (x.ToString().Equals(device))
                {
                    return x;
                }
            }
            return null;
        }
        static public MMDevice SelectedDevice;
        static void Main(string[] args)
        {
            //var enumerator = new MMDeviceEnumerator();
            //MMDevice[] CaptureDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToArray();
            //MMDevice defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
            //SelectedDevice = CaptureDevices.FirstOrDefault(c => c.ID == defaultDevice.ID);
            SelectedDevice = getDevice("Microphone (Realtek High Definition Audio)");
            WasapiCapture capture = new WasapiCapture(SelectedDevice);
            capture.StartRecording();
            capture.DataAvailable += CaptureOnDataAvailable;
        }

        private static void CaptureOnDataAvailable(object sender, WaveInEventArgs e)
        {
            //var value = SelectedDevice.AudioMeterInformation.MasterPeakValue;
            Console.WriteLine(SelectedDevice.AudioMeterInformation);
        }
        
    }
}
