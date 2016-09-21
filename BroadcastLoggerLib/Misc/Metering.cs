using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BroadcastLoggerLib.Handlers;

namespace BroadcastLoggerLib.Misc
{
    public class Metering
    {
        public MMDevice SelectedDevice;
        public WasapiCapture capture;
        public event EventHandler<DeviceVolume> DeviceUpdated;
        public class DeviceVolume : EventArgs
        {
            public float Volume
            {
                get;
                set;
            }
            public DeviceVolume(float volume)
            {
                this.Volume = volume;
            }

        }

        public Metering(string deviceName)
        {
            //SelectedDevice = getDevice(deviceName);
            SelectedDevice = NAudioHandler.MatchDevice(deviceName);
            if (SelectedDevice == null)
            {
                throw new ArgumentException("No audio device found for " + deviceName);
            }
            capture = new WasapiCapture(SelectedDevice);
            try
            {
                capture.StartRecording();
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Can not capture device");
            }

            capture.DataAvailable += CaptureOnDataAvailable;
            Console.WriteLine(SelectedDevice.AudioMeterInformation.MasterPeakValue);
        }
        private void CaptureOnDataAvailable(object sender, WaveInEventArgs e)
        {
            float value = SelectedDevice.AudioMeterInformation.MasterPeakValue;
            if (DeviceUpdated != null)
            {
                DeviceUpdated(this, new DeviceVolume(value));
            }

        }
        public static MMDevice getDevice(string device)
        {
            NAudioHandler handler = new NAudioHandler();
            MMDevice[] devices = handler.getDevices();

            foreach (MMDevice d in devices)
            {
                string stringDevice = d.ToString();
                for (int i = 0; i < device.Length; i++)
                {
                    Console.Write(stringDevice[i]);
                    if (i == device.Length - 1 && stringDevice[i] == device[i])
                    {
                        return d;
                    }
                    if (stringDevice[i] != device[i])
                        break;
                }
            }
            return null;
        }

        public void Dispose()
        {
            capture.StopRecording();
        }
    }
}

