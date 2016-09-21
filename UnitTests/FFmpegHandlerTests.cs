using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BroadcastLoggerLib.Handlers;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace UnitTests
{
    [TestClass]
    public class FFmpegHandlerTests
    {
        [TestMethod, TestCategory("FFmpegHandler")]
        public void ProperInitialization()
        {
            FFmpegHandler handler = new FFmpegHandler("ffmpeg", "ffmpeg.exe");
        }
        [TestMethod, TestCategory("FFmpegHandler")]
        public void FinishedRecording()
        {
            FFmpegHandler handler = new FFmpegHandler("ffmpeg", "ffmpeg.exe");
            handler.FinishedRecording += (o, args) =>
            {
                if (args.FileName == null)
                    Assert.Fail();
            };
            handler.beginRecording(GetDefaultDevice(), 1);
            Thread.Sleep(5000);
        }
        [TestMethod, TestCategory("FFmpegHandler")]
        public void NoExecutable()
        {
            try
            {
                string deviceName = string.Empty;
                FFmpegHandler handler = new FFmpegHandler("ffmpeg", deviceName);
                Assert.Fail("Exception should of been thrown");
            }
            catch (ArgumentException ex)
            {

            }
        }
        [TestMethod, TestCategory("FFmpegHandler")]
        public void Cleanup()
        {
            FFmpegHandler handler = new FFmpegHandler("ffmpeg", "ffmpeg.exe");
            string ffmpegDevice = GetDefaultDevice();
            if (ffmpegDevice == "" || ffmpegDevice == null)
            {
                Assert.Fail("Failed to get a default device check NAudioHandler");
            }
            handler.beginRecording(ffmpegDevice, 1);
            Thread.Sleep(5000);
            if (handler.RunningProcess != null)
            {
                handler.RunningProcess.Kill();
                Assert.Fail("Procces was not stopped");
            }
        }
        [TestMethod, TestCategory("FFmpegHandler")]
        public void InvalidInitializationFile()
        {
            try
            {
                FFmpegHandler handler = new FFmpegHandler("ffmpeg", "InvalidFileName");
                Assert.Fail("Expected an exception");
            }catch(System.ArgumentException){
                //Expected exception
            }
            catch (Exception)
            {
                Assert.Fail("Did not catch the right exception");
            }
        }
        [TestMethod, TestCategory("FFmpegHandler")]
        public void InvalidInitializationSubDirectory()
        {
            try 
            {
                FFmpegHandler handler = new FFmpegHandler("InvalidSubDirectory", "ffmpeg.exe");
                Assert.Fail("Expected an excpetion");
            }
            catch (System.ArgumentException)
            {
                //Expected expcetion
            }
            catch (Exception)
            {
                Assert.Fail("Did not catch the right exception");
            }
        }
        [TestMethod, TestCategory("FFmpegHandler")]
        public void StopRecordingTest()
        {
            FFmpegHandler handler = new FFmpegHandler("ffmpeg", "ffmpeg.exe");
            string ffmpegDevice = GetDefaultDevice();
            if (ffmpegDevice == "" || ffmpegDevice == null)
            {
                Assert.Fail("Failed to get a default device check NAudioHandler");
            }
            
            handler.beginRecording(ffmpegDevice);
            Thread.Sleep(2000);
            if (handler.RunningProcess.HasExited)
                Assert.Fail("Process did not successfuly start");
            Thread.Sleep(1000);
            handler.stopRecording();
            Thread.Sleep(2000);
            if (handler.RunningProcess != null)
            {
                handler.RunningProcess.Kill();
                Assert.Fail("Proccess did not stop");
            }
        }
        [TestMethod, TestCategory("FFmpegHandler")]
        public void TimedRecording()
        {
            FFmpegHandler handler = new FFmpegHandler("ffmpeg", "ffmpeg.exe");
            string defaultDevice = GetDefaultDevice();
            string fileName = handler.beginRecording(defaultDevice, 2);
            Console.WriteLine(fileName);
            Thread.Sleep(5000);
            if (!File.Exists(fileName))
                Assert.Fail("Failed to create the file");
        }
        [TestMethod, TestCategory("FFmpegHandler")]
        public void FileName()
        {
            FFmpegHandler handler = new FFmpegHandler("ffmpeg", "ffmpeg.exe");
            string defaultDevice = GetDefaultDevice();
            string file = handler.beginRecording(defaultDevice, 1);
            Thread.Sleep(5000);
            if (!File.Exists(file))
                Assert.Fail("File was not created");
            File.Delete(file);
        }
        private string GetDefaultDevice()
        {
            FFmpegHandler handler = new FFmpegHandler("ffmpeg", "ffmpeg.exe");
            NAudioHandler nHandler = new NAudioHandler();
            string[] devices = new string[0];
            string defaultDevice = nHandler.getDefaultDevice().ToString();
            string ffmpegDevice = string.Empty;
            handler.getInputDevices((string[] s) =>
            {
                devices = s;
            });
            Thread.Sleep(3000);
            foreach (string s in devices)
            {
                for (int i = 0; i < s.Length; i++)
                {
                    if (i == s.Length - 1 && s[i] == defaultDevice[i])
                    {
                        ffmpegDevice = s;
                        break;
                    }
                    if (s[1] != defaultDevice[1])
                        continue;
                }
            }
            return ffmpegDevice;
        }
    }
}
