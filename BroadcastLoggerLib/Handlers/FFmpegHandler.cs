using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using BroadcastLoggerLib.Handlers;
using BroadcastLoggerLib.Misc;

namespace BroadcastLoggerLib.Handlers
{
    public class FFmpegHandler
    {
            #region privates
            /// <summary>
            /// FFmpeg executable name
            /// </summary>
            private string _ffExe;
            /// <summary>
            /// Path to this class
            /// </summary>
            private string _WorkingPath;
            /// <summary>
            /// Full target ex: C:/users/name/broadcastlogger/ffmpeg/ffmpeg.exe
            /// </summary>
            private string _ffTarget;
            /// <summary>
            /// Subdirectory to our current working directory.
            /// </summary>
            private string _subDir;
            /// <summary>
            /// The process we will be running ffmpeg on. 
            /// </summary>
            private Process process;
            /// <summary>
            /// The devices that ffmpeg detects.
            /// </summary>
            private string[] devices;
            /// <summary>
            /// Target to store saved audio logs
            /// </summary>
            private string storageDir;
            private static List<string> AudioDevices;
            #endregion
        
        #region properties
        /// <summary>
        /// Name of the file that was created
        /// </summary>
        public string FileName
        {
            get;
            set;
        }
        public Process RunningProcess
        {
            get { return process; }
        }
        #endregion
        #region events
        public class FinishedArgs : EventArgs
        {
            public string FileName
            {
                get;
                set;
            }
            public FinishedArgs(string FileName)
            {
                this.FileName = FileName;
            }
        }
        public event EventHandler<FinishedArgs> FinishedRecording;
        #endregion
        public FFmpegHandler(string subDir, string exeName) {
            _ffExe = exeName;
            _subDir = subDir;
            initialize();
        }
        ~FFmpegHandler()
        {
            if (process != null)
            {
                try
                {
                process.Kill();
            }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
        #region methods
        /// <summary>
        /// Verify exe exists and setup properties
        /// </summary>
        private void initialize()
        {
            if (string.IsNullOrEmpty(_ffExe))
            {
                throw new ArgumentException("The name does not exist or was not put in");
            }
            string workingPath = Directory.GetCurrentDirectory();
            _ffTarget = workingPath +"\\" + _subDir + "\\" + _ffExe;//building the target
            if (!File.Exists(_ffTarget))
            {
                throw new ArgumentException("Could not find the ffmpeg executable");
            }
            NAudioHandler handler = new NAudioHandler();
            MMDevice[] devices = handler.getDevices();
            this.devices = new string[devices.Length];
            int count = 0;
            foreach (MMDevice d in devices)
            {
                this.devices[count++] = d.ToString();
            }
            //this.devices = handler.getDirectDevices();
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Combine the base folder with your specific folder....
            storageDir = Path.Combine(folder, "BroadcastLogger");

            // Check if folder exists and if not, create it
            if (!Directory.Exists(storageDir))
                Directory.CreateDirectory(storageDir);
        }
        /// <summary>
        /// Take raw parameters and a runworkercompletedeventhandler to deal with the result. 
        /// 
        /// </summary>
        /// <param name="parameters">The parameters to run ffmpeg with</param>
        /// <param name="e">This holds the output to ffmpeg when it is done.</param>
        private void runRaw(string parameters, RunWorkerCompletedEventHandler e = null) {
            //Console.WriteLine(parameters + "\n\n\n");
            ProcessStartInfo oInfo = new ProcessStartInfo(this._ffTarget, parameters);
            //Don't show the shell
            oInfo.UseShellExecute = false;
            oInfo.CreateNoWindow = true;
            //Change the output
            oInfo.RedirectStandardError = true;
            oInfo.RedirectStandardInput = true;
            if (process != null)
            {
                return;
            }
            string output = null;
            StreamReader srOutput = null;
            try
            {
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += new DoWorkEventHandler(
                    delegate(object o, DoWorkEventArgs args)
                    {
                        //Start ffmpeg
                        process = System.Diagnostics.Process.Start(oInfo);
                        process.EnableRaisingEvents = true;
                        //When ffmpeg is finished we want to fire the event to let the user know we are done.
                        process.Exited += (obj, a) =>
                        {
                            EventHandler<FinishedArgs> handler = FinishedRecording;
                            if (handler != null)
                            {
                                FinishedArgs finishedArgs = null;
                                if (FileName != null)
                                {
                                    finishedArgs = new FinishedArgs(FileName);
                                }
                                handler(this, finishedArgs);
                            }
                        };
                        //Getting the output
                        srOutput = process.StandardError;
                        output = srOutput.ReadToEnd();
                        args.Result = output;
                    });
                bw.RunWorkerCompleted += e;
                bw.RunWorkerCompleted += (o, args) =>
                {
                    this.stopRecording();
                };
                bw.RunWorkerAsync();
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                output = string.Empty;
            }
            finally
            {
                if(srOutput != null) {
                    srOutput.Close();
                    srOutput.Dispose();
                }
            }
        }
        /// <summary>
        /// Takes in an action that will recieve an array of strings that hold the connected input devices
        /// </summary>
        /// <param name="s">The action to be preformed, it will take in an array of devices as string</param>
        public void getInputDevices(Action<string[]> s)
        {
            RunWorkerCompletedEventHandler e = new RunWorkerCompletedEventHandler(
                delegate(object o, RunWorkerCompletedEventArgs args)
                {
                    string pat = "\\  \"(.*?)\\\"";//Pattern to find the correct devices
                    string input = args.Result.ToString();
                    MatchCollection matches = Regex.Matches(input, pat);
                    //Convert to string array for easy handling
                    string[] output = new string[matches.Count];
                    for (int i = 0; i < matches.Count; ++i)
                    {
                        string temp = matches[i].Value;
                        temp = temp.Substring(3, temp.Length - 4);
                        output[i] = temp;
                    }
                    s(output);
                });
            string parameters = "-list_devices true -f dshow -i outputDevice";
            runRaw(parameters, e);
        }
        /// <summary>
        /// Concatenate audio files given an array of strings
        /// that hold all the paths to the files we want to 
        /// concatenate
        /// </summary>
        /// <param name="files">The files we need to concatenate. (Order of array is order concatenated)</param>
        /// <returns>The path to the concatenated file. </returns>
        public string ConcatenateFiles(string[] files)
        {
            string hash = Hasher.calculateMD5Hash(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.f"));
            string outputFile = storageDir + "\\" + hash + ".m4a";
            string target = storageDir + "\\" + hash + ".txt";
            //Console.WriteLine(target);
            StreamWriter textFile = new StreamWriter(target);
            foreach (string s in files)
            {
                textFile.WriteLine("file '" + s + "'");
            }
            textFile.Close();
            string command = "-f concat -i \"" + target + "\" -c copy \"" + outputFile + "\"";
            //When we are done concatenating the files delete the file we created to feed into ffmpeg
            RunWorkerCompletedEventHandler handler = (obj, args) => {
                if (File.Exists(target))
                {
                    File.Delete(target);
                }
            };
            //Console.WriteLine(command);
            runRaw(command, handler);
            return outputFile;
        }
        /// <summary>
        /// Starts the recording as stores the file name as an md5 hash of the current time.
        /// </summary>
        /// <param name="device">Device name</param>
        /// <param name="seconds">Seconds to run the recording for</param>
        /// <returns></returns>
        public string beginRecording(string device, uint seconds)
        {
            /*if (!verifyDevice(device))
            {
                throw new ArgumentException("Device is not available");
            }*/
            string hash = Hasher.calculateMD5Hash(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                FileName = storageDir + "\\" + hash + ".m4a";
                string parameters = "-f dshow -i audio=\"" + device + "\" -c:a libfdk_aac -profile:a aac_he -b:a 64k -t " + seconds + " \"" + FileName + "\"";
            runRaw(parameters, null);
            return FileName;
        }
        /// <summary>
        /// Start recording at a given bitrate, if the bitrate is not either
        /// 24, 48, 64, 96, or 128 it will default to the 24
        /// </summary>
        /// <param name="device">The device to record from in a string format</param>
        /// <param name="seconds">The number of seconds to record for</param>
        /// <param name="bitrate">The bit rate to record at. 24, 48, 64, 96, or 128</param>
        public string beginRecording(string device, uint seconds, int bitrate)
        {
            /*if (!verifyDevice(device))
            {
                throw new ArgumentException("Device is not available");
            }*/
            string bitrateFlag;
            switch (bitrate) {
                case 24 :
                    bitrateFlag = "-c:a libfdk_aac -profile:a aac_he_v2 -b:a 24k";
                    break;
                case 48 :
                    bitrateFlag = "-c:a libfdk_aac -profile:a aac_he -b:a 48k";
                    break;
                case 64 :
                    bitrateFlag = "-c:a libfdk_aac -profile:a aac_he -b:a 64k";
                    break;
                case 96 :
                    bitrateFlag = "-c:a libfdk_aac -b:a 96k";
                    break;
                case 128:
                    bitrateFlag = "-c:a libfdk_aac -b:a 128k";
                    break;
                default :
                    bitrateFlag = "-c:a libfdk_aac -profile:a aac_he_v2 -b:a 24k";
                    break;
            }
            string hash = Hasher.calculateMD5Hash(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            FileName = storageDir + "\\" + hash + ".m4a";
            string parameters = "-f dshow -i audio=\"" + device + "\" " + bitrateFlag +" -t " + seconds + " \"" + FileName + "\"";
            runRaw(parameters, null);
            return FileName;
        }
        public bool verifyDevice(string device) {
            foreach(string d in devices) {
                if (d.Equals(device))
                {
                    //Console.WriteLine("Success");
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Starts the recording and runs indefinietly.
        /// </summary>
        /// <param name="device">The device to run on </param>
        /// <returns></returns>
        public bool beginRecording(string device)
        {
            /*if (!verifyDevice(device))
                {
                Console.WriteLine("Could not find device\n\n\n\n\n");
                throw new ArgumentException("Device is not available");
                }*/
            string hash = Hasher.calculateMD5Hash(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            FileName = hash + ".m4a";
            string parameters = "-f dshow -i audio=\"" + device + "\" -c:a libfdk_aac -profile:a aac_he -b:a 64k \"" + FileName + "\"";
            runRaw(parameters, null);
            return false;
        }
        /// <summary>
        /// Stops the recording if there was one and returns the file name of the recording. 
        /// </summary>
        /// <returns></returns>
        public string stopRecording()
        {
            if (process != null)
            {
                process.StandardInput.Write("q");
                process = null;
                return FileName;
            }
            return string.Empty;
        }
        #endregion 
    }
}
