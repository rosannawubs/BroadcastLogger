using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BroadcastLoggerLib;
using System.Data.SQLite;
using System.Collections.Generic;

namespace UnitTests
{
    [TestClass]
    public class StorageTests
    {
        [TestCategory("Storage"), TestMethod]
        public void StorageCreation()
        {
            Storage storage = new Storage();
            ResetStorage(storage);
        }
        [TestCategory("Storage"), TestMethod]
        public void AddStation()
        {
            Storage storage = new Storage();
            ResetStorage(storage);
            storage.AddStation("stationName", "authCode");
        }
        [TestCategory("Storage"), TestMethod]
        public void SetStationName()
        {
            Storage storage = new Storage();
            ResetStorage(storage);
            storage.AddStation("stationID", "authCode");
            storage.SetStationName("stationID", "stationName");
        }
        [TestCategory("Storage"), TestMethod]
        public void AddToQueueWithoutStation()
        {
            Storage storage = new Storage();
            ResetStorage(storage);
            try
            {
                storage.AddToQueue("stationID", "fileName", "recordID");
                throw new ArgumentException();
            }
            catch (ArgumentException) { Assert.Fail(); }
            catch (Exception ex) { Console.WriteLine(ex); }
        }
        [TestCategory("Storage"), TestMethod]
        public void UpdateLocalFileName()
        {
            Storage storage = new Storage();
            ResetStorage(storage);
            storage.AddStation("stationID", "authCode");
            int uploadID = storage.AddToQueue("stationID", "fileName", "recordID");
            storage.UpdateLocalFileName(uploadID, "alocalFileName");
            List<StorageResult> result = storage.GetSeveral(2);
            if (result[0].LocalFileName != "alocalFileName")
                Assert.Fail("Was not able to set file name");
        }
        [TestCategory("Storage"), TestMethod]
        public void UpdateAWSFileName()
        {
            Storage storage = new Storage();
            ResetStorage(storage);
            storage.AddStation("stationID", "authCode");
            int uploadID = storage.AddToQueue("stationID", "fileName", "recordID");
            storage.UpdateAWSFileName(uploadID, "FirstAWSName");
            List<StorageResult> result = storage.GetSeveral(2);
            if (result[0].AWSFileName != "FirstAWSName")
                Assert.Fail("Was not able to set AWS file name that was previously empty");
            storage.UpdateAWSFileName(uploadID, "SecondAWSName");
            result = storage.GetSeveral(2);
            if (result[0].AWSFileName != "SecondAWSName")
                Assert.Fail("Was not able to change AWS file name");
        }
        [TestCategory("Storage"), TestMethod]
        public void UpdateRecordID()
        {
            Storage storage = new Storage();
            ResetStorage(storage);
            storage.AddStation("stationID", "authCode");
            int uploadID = storage.AddToQueue("stationID", "filename", "recordID");
            storage.UpdateRecordID(uploadID, "FirstRecordID");
            List<StorageResult> result = storage.GetSeveral(2);
            if (result[0].RecordID != "FirstRecordID")
                Assert.Fail("Was not able to set record ID that was previously empty");
            storage.UpdateRecordID(uploadID, "SecondRecordID");
            result = storage.GetSeveral(2);
            if (result[0].RecordID != "SecondRecordID")
                Assert.Fail("Was not able to modify a prexisting record ID");

        }
        [TestCategory("Storage"), TestMethod]
        public void IndexerTest()
        {
            Storage storage = new Storage();
            ResetStorage(storage);
            storage.AddStation("stationID", "authCode");
            int uploadID = storage.AddToQueue("stationID", "LocalFileName", "recordID");
            storage.UpdateAWSFileName(uploadID, "AWSFileName");
            storage.UpdateStatus(uploadID, Status.AUTHORIZED);
            storage.SetStationName("stationID", "stationName");
            Console.WriteLine("Upload ID" + uploadID);
            StorageResult result = storage[uploadID];
            if (result.AuthCode != "authCode")
                Assert.Fail("Failed to get correct auth code");
            if (result.AWSFileName != "AWSFileName")
                Assert.Fail("Failed to get correct aws file name");
            if (result.LocalFileName != "LocalFileName")
                Assert.Fail("Failed to get correct local file name");
            if (result.RecordID != "recordID")
                Assert.Fail("Failed to get correct recordID");
            if (result.StationID != "stationID")
                Assert.Fail("Failed to get correct stationID");
            if (result.StationName != "stationName")
                Assert.Fail("Failed to get correct station name");
            if (result.Status != Status.AUTHORIZED)
                Assert.Fail("Failed to get correct status number");
            if (int.Parse(result.UploadID) != uploadID)
                Assert.Fail("Failed to get the correct uploadID");
        }
        [TestCategory("Storage"),TestMethod]
        public void PopTest()
        {
            Storage storage = new Storage();
            ResetStorage(storage);
            storage.AddStation("stationID", "authCode");
            int uploadID = storage.AddToQueue("stationID", "LocalFileName", "recordID");
            storage.UpdateAWSFileName(uploadID, "AWSFileName");
            storage.UpdateStatus(uploadID, Status.AUTHORIZED);
            storage.SetStationName("stationID", "stationName");
            storage.AddToQueue("stationID", "LocalFileName2", "recordId2");
            List<StorageResult> result = storage.GetSeveral(2);
            if (result.Count != 2)
                Assert.Fail("Failed to load in all of the queue");
            storage.Pop();
            if (storage.Length != 1)
                Assert.Fail("Nothing got deleted");
            result = storage.GetSeveral(2);
            if (result.Count != 1)
                Assert.Fail("Failed to delete");
            if (result[0].RecordID != "recordId2")
                Assert.Fail("Failed to delete");    
        }
        [TestCategory("Storage"), TestMethod]
        public void GetInvalidEntry()
        {
            Storage storage = new Storage();
            ResetStorage(storage);
            try
            {
                StorageResult sr = storage[0];
                Assert.Fail("Exception should of been thrown");
            }
            catch (InvalidOperationException)
            {
                //Expected exception
            }

        }
        private void ResetStorage(Storage s)
        {
            s.DropUploadQueue();
            s.DropStations();
            s.InitalizeTables();
        }
    }
}
