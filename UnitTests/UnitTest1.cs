using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BroadcastLogger;
using NAudio.CoreAudioApi;
using System.Threading;
using System.Json;
using System.Collections.Generic;
using BroadcastLoggerLib;
using System.Threading.Tasks;
using BroadcastLoggerLib.Handlers;
using BroadcastLoggerLib.Misc;
using System.IO;

namespace UnitTests
{
    /// <summary>
    /// All tests performed successfully 2015-May-23
    /// </summary>
    [TestClass]
    public class UnitTest1
    {


        [TestMethod]
        public void DefaultDevice()
        {
            var enumerator = new MMDeviceEnumerator();
            MMDevice defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
            NAudioHandler handler = new NAudioHandler();     
            MMDevice device = handler.getDefaultDevice();
            Assert.AreEqual(device.ToString(), defaultDevice.ToString());
        }

        #region Hasher
        [TestMethod, TestCategory("Hasher")]
        public void MD5Test1() {
            String original = "Hello, World!";
            String hashRef = "65a8e27d8879283831b664bd8b7f0ad4";

            String result = Hasher.calculateMD5Hash(original);
            Console.WriteLine(result);
            Assert.AreEqual(hashRef, result, true);
        }

        [TestMethod, TestCategory("Hasher")]
        public void MD5Test2() {
            String original = "8JFdiD6SkfdmZDKFIE6DSf6567saFUD" + "-app_auth_station-house-JFZFCFh9zS";
            String hashRef = "d067f5264ca5aaca698cb9b34a4b08f3";

            String result = Hasher.calculateMD5Hash(original);
            Console.WriteLine(result);
            Assert.AreEqual(hashRef, result);
        }
        #endregion

        #region Timer
        [TestMethod, TestCategory("Timer")]
        public void setRecordingTimeTest() {
            bool flag = false;
            double time = Manager.Instance.setRecordingTime();
            if(time <= 3600)
                flag = true;
            Assert.IsTrue(flag);
        }

        [TestMethod, TestCategory("Timer")]
        public void getRemainderOfHourTest()
        {
            DateTime date = new DateTime(12, 12, 12, 1, 0, 0);
            double time = Manager.Instance.getRemainderOfHour(date);
            Assert.AreEqual(time, 3600);
        }

        [TestMethod, TestCategory("Timer")]
        public void getDifferenceInSecondsTest()
        {
            DateTime date = DateTime.Now - new TimeSpan(1, 0, 0);
            double time = Manager.Instance.getDifferenceInSeconds(DateTime.Now, date);
            Assert.AreEqual(time, 3600);
        }
        #endregion
    }
}
