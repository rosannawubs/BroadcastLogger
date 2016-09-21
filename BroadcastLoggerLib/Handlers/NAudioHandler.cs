using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BroadcastLoggerLib.Handlers
{
    public class NAudioHandler
    {
        public MMDevice[] getDevices()
        {
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToArray();
            return devices;
        }
        public string[] getDirectDevices()
        {
            int waveInDevices = WaveIn.DeviceCount;
            string[] output = new string[waveInDevices];
            for (int waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++)
            {
                WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(waveInDevice);
                output[waveInDevice] = deviceInfo.ProductName;
            }
            return output;
        }
        public string getDefaultDirectDevice()
        {
            var enumerator = new MMDeviceEnumerator();
            MMDevice defaultDevice = null;
            try
            {
                defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return defaultDevice.ToString().Substring(0, 31);
        }
        public MMDevice getDefaultDevice()
        {
            var enumerator = new MMDeviceEnumerator();
            MMDevice defaultDevice = null;
            try
            {
                defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return defaultDevice;
        }
        /// <summary>
        /// Used to to match a partial device name to a MMDevice. 
        /// FFmpeg is limited to only return 31characters of a device
        /// name in Windows 7. 
        /// </summary>
        /// <param name="device">Partial string name.</param>
        /// <returns>The MMDevice</returns>
        public static MMDevice MatchDevice(string device)
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
    }
}

