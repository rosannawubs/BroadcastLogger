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

namespace UnitTests {

    [TestClass]
    public class APIHandlerTests {
        #region BroadcastLoggerHandler Tests
        // ----------------------------------------------------------------------
        // BroadcastLoggerHandler Tests
        // ----------------------------------------------------------------------

        /// <summary>
        /// To use a different stationId and authCode, you may need to create a 
        /// new station at https://secure.broadcastlogger.com.
        /// 
        /// </summary>
        private String stationId = "";
        private String authCode = "";
        private BroadcastLoggerHandler handler = new BroadcastLoggerHandler();

        // Used with BroadcastLogger API calls
        private const String API_KEY = "";

        // All BroadcastLogger API calls start with this URL
        private const String API_URL = "http://api.broadcastlogger.com/api/";

        /// <summary>
        /// Tests the app_auth_station API call
        /// </summary>
        [TestMethod, TestCategory("API Handlers"), TestCategory("BroadcastLoggerHandler")]
        public void authStationTest() {
            //**
            bool result = handler.authStation(stationId, authCode);
            Assert.AreEqual(result, true);
        }

        /// <summary>
        /// Tests the app_auth_station API call asynchronously
        /// </summary>
        /// <returns></returns>
        [TestMethod, TestCategory("API Handlers"), TestCategory("BroadcastLoggerHandler")]
        public async Task authStationAsyncTest() {
            //**
            bool result = await handler.authStationAsync(stationId, authCode);
            Assert.AreEqual(result, true);
        }

        [TestMethod, TestCategory("API Handlers"), TestCategory("BroadcastLoggerHandler")]
        public async Task BitRateTest() {
            //**
            await handler.authStationAsync(stationId, authCode);
            Assert.AreEqual(handler.BitRate, 24);
        }

        [TestMethod, TestCategory("API Handlers"), TestCategory("BroadcastLoggerHandler")]
        public async Task BitRateTestFail() {
            //**
            await handler.authStationAsync(stationId, "wrong authcode");
            Assert.AreEqual(handler.BitRate, -1);
        }

        /// <summary>
        /// Tests the app_recording_create API call. Since it returns a random id every time,
        /// We can only test if the successful result is NOT equal to its failstate
        /// </summary>
        [TestMethod, TestCategory("API Handlers"), TestCategory("BroadcastLoggerHandler")]
        public void recordingCreateTest() {
            //**
            handler.recordingCreate(stationId, authCode, DateTime.UtcNow, false);
            Console.WriteLine(DateTime.UtcNow);
            Assert.AreNotEqual(handler.RecordingId, -1);
        }

        /// <summary>
        /// Tests the app_recording_create API call asynchronously
        /// </summary>
        /// <returns></returns>
        [TestMethod, TestCategory("API Handlers"), TestCategory("BroadcastLoggerHandler")]
        public async Task recordingCreateAsyncTest() {
            //**
            await handler.recordingCreateAsync(stationId, authCode, DateTime.Now, false);
            Assert.AreNotEqual(handler.RecordingId, -1);
        }

        /// <summary>
        /// Tests a failed app_recording_create API call
        /// </summary>
        /// <returns></returns>
        [TestMethod, TestCategory("API Handlers"), TestCategory("BroadcastLoggerHandler")]
        public async Task recordingCreateAsyncTestFail() {
            //**
            await handler.recordingCreateAsync(stationId, "wrongAuthCode", DateTime.Now, false);
            Console.WriteLine(handler.ErrorMessage);
            Assert.AreEqual(handler.RecordingId, -1);
        }

        /// <summary>
        /// Tests the app_recording_done API call.
        /// </summary>
        [TestMethod, TestCategory("API Handlers"), TestCategory("BroadcastLoggerHandler")]
        public void recordingDoneTest() {
            //**
            handler.recordingCreate(stationId, authCode, DateTime.Now, false);
            bool result = handler.recordingDone(stationId, authCode, handler.RecordingId);
            Assert.AreEqual(result, true);
        }

        /// <summary>
        /// Tests the app_recording_done API call with the added "next recording id" parameter
        /// </summary>
        [TestMethod, TestCategory("API Handlers"), TestCategory("BroadcastLoggerHandler")]
        public void recordingDoneTest2() {
            //**
            //handler.recordingCreate(stationId, authCode, DateTime.Now, true);
            bool result = handler.recordingDone(stationId, authCode, handler.RecordingId, 2);
            Assert.AreEqual(result, true);
        }

        [TestMethod, TestCategory("API Handlers"), TestCategory("BroadcastLoggerHandler")]
        public void recordingDoneTestFail() {
            //**
            handler.recordingDone(stationId, "wrongAuthCode", 1);
            Assert.AreEqual(handler.ErrorMessage, "Incorrect Station ID or Auth Code");
        }

        /// <summary>
        /// Tests the app_recording_done API call asynchronously
        /// </summary>
        /// <returns></returns>
        [TestMethod, TestCategory("API Handlers"), TestCategory("BroadcastLoggerHandler")]
        public async Task recordingDoneAsyncTest() {
            //**
            await handler.recordingCreateAsync(stationId, authCode, DateTime.Now, false);
            Task<bool> task = handler.recordingDoneAsync(stationId, authCode, handler.RecordingId);
            await task;
            Assert.AreEqual(task.Result, true);
        }

        [TestMethod, TestCategory("API Handlers"), TestCategory("BroadcastLoggerHandler")]
        public void setAsUploadedTest() {
            //**
            handler.recordingCreate(stationId, authCode, DateTime.Now, false);
            handler.recordingDone(stationId, authCode, handler.RecordingId);
            handler.setAsUploaded(stationId, authCode, handler.RecordingId);
            Assert.AreEqual(handler.Status, true);
        }

        [TestMethod, TestCategory("API Handlers"), TestCategory("BroadcastLoggerHandler")]
        public void setAsUploadedTestFail() {
            //**
            handler.setAsUploaded(stationId, authCode, 1);
            Assert.AreEqual(handler.Status, false);
        }

        [TestMethod, TestCategory("API Handlers"), TestCategory("BroadcastLoggerHandler")]
        public async Task setAsUploadedAsyncTest() {
            //**
            await handler.recordingCreateAsync(stationId, authCode, DateTime.Now, false);
            await handler.recordingDoneAsync(stationId, authCode, handler.RecordingId);
            await handler.setAsUploadedAsync(stationId, authCode, handler.RecordingId);
            Assert.AreEqual(handler.Status, true);
        }

        [TestMethod, TestCategory("API Handlers"), TestCategory("BroadcastLoggerHandler")]
        public void ErrorMessageTest() {
            //**
            handler.authStation(stationId, "wrong authcode");
            String reference = "Auth post incorrect. Check to ensure the hash is being formed correctly.";
            Assert.AreEqual(handler.ErrorMessage, reference);
        }

        [TestMethod, TestCategory("API Handlers"), TestCategory("BroadcastLoggerHandler")]
        public void RecordingIdTest() {
            //**
            handler.recordingCreate(stationId, authCode, DateTime.Now, false);
            Assert.AreNotEqual(handler.RecordingId, -1);
        }

        [TestMethod, TestCategory("API Handlers"), TestCategory("BroadcastLoggerHandler")]
        public void ObjectNameTest() {
            //**
            handler.recordingDone(stationId, "asdf", 1);
            String reference = "ERROR: Incorrect Station ID or Auth Code";
            Assert.AreEqual(handler.ObjectName, reference);
        }

        [TestMethod, TestCategory("API Handlers"), TestCategory("BroadcastLoggerHandler")]
        public void ObjectNameTest2() {
            DateTime timeStamp = DateTime.Now;
            DateTime utc = DateTime.Now.ToUniversalTime();
            Console.WriteLine("DateTime.Now: " + timeStamp);
            Console.WriteLine("DateTime.Now.ToUniversalTime(): " + DateTime.Now.ToUniversalTime());
            String nameReference = "test/" + timeStamp.Year.ToString().PadLeft(2, '0');
            nameReference += "-" + timeStamp.Month.ToString().PadLeft(2, '0');
            nameReference += "-" + timeStamp.Day.ToString().PadLeft(2, '0');
            nameReference += "/test_" + timeStamp.Year.ToString().PadLeft(2, '0');
            nameReference += "-" + timeStamp.Month.ToString().PadLeft(2, '0');
            nameReference += "-" + timeStamp.Day.ToString().PadLeft(2, '0');
            nameReference += "_" + timeStamp.Hour.ToString().PadLeft(2, '0');
            nameReference += ":" + timeStamp.Minute.ToString().PadLeft(2, '0');
            nameReference += ":" + timeStamp.Second.ToString().PadLeft(2, '0') + ".m4a";
            //**
            handler.recordingCreate(stationId, authCode, utc, false);
            handler.recordingDone(stationId, authCode, handler.RecordingId);
            Assert.AreEqual(handler.ObjectName, nameReference);
        }

        [TestMethod, TestCategory("API Handlers"), TestCategory("BroadcastLoggerHandler")]
        public void StationNameTest() {
            //**
            handler.authStation(stationId, authCode);
            Assert.AreEqual(handler.StationName, "Test Station");
        }

        [TestMethod, TestCategory("API Handlers"), TestCategory("BroadcastLoggerHandler")]
        public void StationNameTest2() {
            //**
            handler.authStation(stationId, "wrong authcode");
            String reference = "ERROR: Auth post incorrect. Check to ensure the hash is being formed correctly.";
            Console.WriteLine(handler.StationName);
            Assert.AreEqual(handler.StationName, reference);
        }
        #endregion

        #region AmazonHandler Tests
        // ----------------------------------------------------------------------
        // AmazonHandler Tests
        // ----------------------------------------------------------------------

        [TestMethod, TestCategory("API Handlers"), TestCategory("AmazonHandler")]
        public void putObjectTest() {
            String filePath = Directory.GetCurrentDirectory().ToString();
            filePath = Directory.GetParent(filePath).ToString();
            filePath = Directory.GetParent(filePath).ToString();
            filePath += @"\UnitTest1.cs";
            String objectName = "UnitTests/UnitTest1.cs";
            String bucketName = "broadcastlogger-audio";
            AmazonHandler amazonHandler = new AmazonHandler();
            bool result = amazonHandler.putObject(filePath, objectName, bucketName);
            Assert.AreEqual(result, true);
        }

        [TestMethod, TestCategory("API Handlers"), TestCategory("AmazonHandler")]
        public async Task putObjectAsyncTest() {
            String filePath = Directory.GetCurrentDirectory().ToString();
            filePath = Directory.GetParent(filePath).ToString();
            filePath = Directory.GetParent(filePath).ToString();
            filePath += @"\UnitTest1.cs";
            String objectName = "UnitTests/UnitTest1.cs";
            String bucketName = "broadcastlogger-audio";
            AmazonHandler amazonHandler = new AmazonHandler();
            Task<bool> task = amazonHandler.putObjectAsync(filePath, objectName, bucketName);
            bool result = await task;
            Assert.AreEqual(result, true);
        }

        [TestMethod, TestCategory("API Handlers"), TestCategory("AmazonHandler")]
        public void AmazonConstructorTest() {
            AmazonHandler handler = new AmazonHandler("",
                "",
                "");
            Assert.IsInstanceOfType(handler, typeof(AmazonHandler));
        }

        [TestMethod, TestCategory("API Handlers"), TestCategory("AmazonHandler")]
        public void AmazonConstructorTest2() {
            AmazonHandler handler = new AmazonHandler();
            Assert.IsInstanceOfType(handler, typeof(AmazonHandler));
        }

        [TestMethod, TestCategory("API Handlers"), TestCategory("AmazonHandler")]
        public void StatusTestTrue() {
            String filePath = Directory.GetCurrentDirectory().ToString();
            filePath = Directory.GetParent(filePath).ToString();
            filePath = Directory.GetParent(filePath).ToString();
            filePath += @"\UnitTest1.cs";
            String objectName = "UnitTests/UnitTest1.cs";
            String bucketName = "broadcastlogger-audio";
            AmazonHandler amazonHandler = new AmazonHandler();
            amazonHandler.putObject(filePath, objectName, bucketName);
            Assert.AreEqual(amazonHandler.Status, true);
        }

        [TestMethod, TestCategory("API Handlers"), TestCategory("AmazonHandler")]
        public void StatusTestFalse() {
            String filePath = Directory.GetCurrentDirectory().ToString();
            filePath = Directory.GetParent(filePath).ToString();
            filePath = Directory.GetParent(filePath).ToString();
            filePath += @"\UnitTest1.cs";
            String objectName = "UnitTests/UnitTest1.cs";
            String bucketName = "broadcastlogger-audio";
            AmazonHandler amazonHandler = new AmazonHandler();
            amazonHandler.putObject(filePath, objectName, "asdf");
            Assert.AreEqual(amazonHandler.Status, false);

        }
        #endregion

        #region HttpHandler Tests
        // ----------------------------------------------------------------------
        // HttpHandler Tests
        // ----------------------------------------------------------------------

        [TestMethod, TestCategory("API Handlers"), TestCategory("HttpHandler")]
        public void executeTest() {
            // assemble the string to be hashed
            String hashFormat = API_KEY + "-app_auth_station-" + stationId + "-" + authCode;
            // assemble the post data with the hashed string
            String postData = "auth=" + Hasher.calculateMD5Hash(hashFormat);
            // assemble the api url
            String url = API_URL + "app_auth_station/" + stationId + "/" + authCode;
            HttpHandler handler = new HttpHandler();
            String reference = "{\"status\":1,\"stationinfo\":{\"name\":\"Test Station\",\"bitrate\":\"24k\"}}";
            String result = handler.execute(url, postData);
            Console.WriteLine(result);
            Assert.AreEqual(result, reference);
        }

        [TestMethod, TestCategory("API Handlers"), TestCategory("HttpHandler")]
        public async Task executeAsyncTest() {
            // assemble the string to be hashed
            String hashFormat = API_KEY + "-app_auth_station-" + stationId + "-" + authCode;
            // assemble the post data with the hashed string
            String postData = "auth=" + Hasher.calculateMD5Hash(hashFormat);
            // assemble the api url
            String url = API_URL + "app_auth_station/" + stationId + "/" + authCode;
            HttpHandler handler = new HttpHandler();
            String reference = "{\"status\":1,\"stationinfo\":{\"name\":\"Test Station\",\"bitrate\":\"24k\"}}";
            Task<String> task = handler.executeAsync(url, postData);
            await task;
            Assert.AreEqual(task.Result, reference);
        }
        #endregion
    }
}
