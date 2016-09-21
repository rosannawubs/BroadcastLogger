using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using BroadcastLoggerLib.Handlers;
using BroadcastLoggerLib.Misc;
using System.Threading.Tasks;
using BroadcastLoggerLib;
using BCLLib.Misc;

namespace BroadcastLoggerLib
{
    public class Manager
    {
        //--------------------------------------------------------------------------------------------
        // MEMBERS 
        //--------------------------------------------------------------------------------------------
        #region members
        //public const int        CALL_INTERVAL = 1000;   // Interval at which TimeCallback is called, in ms.   
        /// <summary>
        /// Instance of Manager.
        /// </summary>
        private static      Manager         instance;

        private             string          stationId;
        private             string          authCode;
        private             string          bucketName = "broadcastlogger-audio";
        private             int             bitRate = 0;
        /// <summary>
        /// Recording flags and timers.
        /// </summary>
        public              bool            recordStop = true;
        private             int             currentHour = 0;
        public              uint            timeLeft = 3600; // ### CHANGE BACK TO PRIVATE ON DELIVERY
        private             bool            isHandlingQueue = false;
        /// <summary>
        /// Timer that runs on a separate thread.
        /// Using System.Threading.Timer, not System.Timers.
        /// </summary>
        private             Timer           recordTimer;
        private             Timer           queueTimer;
        private             FFmpegHandler   ffmpegFirst;
        //private             FFmpegHandler   ffmpegSecond;
        private             TimerCallback   recordCallbackDelegate;
        private             TimerCallback   queueCallbackDelegate;
        //private             bool            ffmpegFirstDone;
        //private             bool            ffmpegSecondDone;
        // Audio device as string
        private             string          audioDevice;
        public              bool            hasUploadCompleted;
        // Delegates and Event Handlers
        public delegate void    StatusUpdateHandler(String message);
        public                  StatusUpdateHandler         StatusUpdate;
        public delegate void    RecordingStoppedHandler();
        public                  RecordingStoppedHandler     RecordingStopped;
        // Pointers to the SQLite database
        private             Storage             storage;
        private             Storage             storage2;
        #endregion

        //--------------------------------------------------------------------------------------------
        // PROPERTIES
        //--------------------------------------------------------------------------------------------
        #region properties
        /// <summary>
        /// Set the station id.
        /// </summary>
        public string StationId { get { return stationId; } set { stationId = value; } }
        //--------------------------------------------------------------------------------------------
        /// <summary>
        /// Set the authorization code.
        /// </summary>
        public string AuthCode { get { return authCode; } set { authCode = value; } }
        //--------------------------------------------------------------------------------------------
        /// <summary>
        /// Set the authorization code.
        /// </summary>
        public int BitRate { set { bitRate = value; } }
        /// <summary>
        /// Assigning a String to StatusMessage will invoke the StatusUpdate event handler
        /// and notify all subscribers.
        /// </summary>
        public String StatusMessage {
            set {
                if (StatusUpdate != null) {
                    Console.WriteLine(value);
                    StatusUpdate(value);
                }
            }
        }
        #endregion
        //--------------------------------------------------------------------------------------------
        // METHODS
        //--------------------------------------------------------------------------------------------
        #region methods

        /// <summary>
        /// Constructor.
        /// </summary>
        private Manager() {}

        //--------------------------------------------------------------------------------------------
        /// <summary>
        /// Setup database/queue.
        /// </summary>
        public void Initialization()
        {
            //ffmpegFirst = new FFmpegHandler("ffmpeg", "ffmpeg.exe");
            //ffmpegSecond = new FFmpegHandler("ffmpeg", "ffmpeg.exe");
            //ffmpegFirst.FinishedRecording += ffmpegFirst_FinishedRecording;
            storage = new Storage();
            //storage2 = new Storage();
            var handler = new BroadcastLoggerHandler();
            handler.authStation(stationId, authCode);
            BitRate = handler.BitRate;
            storage.AddStation(stationId, authCode);
            storage.SetStationName(stationId, "Example DB");
            //ffmpegFirstDone = true;
            //ffmpegSecondDone = true;
        }

        //--------------------------------------------------------------------------------------------
        /// <summary>
        /// Start queue periodic call.
        /// </summary>
        public void startTimer() {
            queueCallbackDelegate = new TimerCallback(handleQueue);
            //recordCallbackDelegate = new TimerCallback(record);
            queueTimer = new Timer(queueCallbackDelegate, null, 0, 1000);
            //recordTimer = new Timer(recordCallbackDelegate, null, 0, RECORDING_TIME * 1000);
        }

        //--------------------------------------------------------------------------------------------
        /// <summary>
        /// Singleton call.
        /// </summary>
        public static Manager Instance
        {
            get
            {
                if (instance == null)
                    instance = new Manager();
                return instance;
            }
        }
        //--------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets audio device.
        /// Must call before record.
        /// </summary>
        /// <param name="device">Audio device to listen to.</param>
        public void setDevice(String device) { audioDevice = device; }
        //--------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets amount of time to record in seconds.
        /// </summary>
        /// <returns>Amount of time to record in seconds.</returns>
        public Double setRecordingTime()
        {
            DateTime currentDate = DateTime.Now;
            Double seconds = 3600;
            currentHour = currentDate.Hour;
            if (currentDate.Minute != 0 || currentDate.Second != 0)
            {
                seconds = getRemainderOfHour(currentDate);
                //seconds = getRemainderOfMinute(currentDate);
            }
            return seconds;
        }
        //--------------------------------------------------------------------------------------------
        /// <summary>
        /// Handles items in queue and uploads them.
        /// </summary>
        /// <param name="obj">Temporary object to allow compatibility with delegate.</param>
        public void handleQueue(Object obj = null)
        {
            Console.Write("----------------handleQueue----------------");
            if (isHandlingQueue) {
                return;
            } else {
                isHandlingQueue = true;
            }
            // Load items from SQLite database into a List of StorageResult objects
            List<StorageResult> results = storage.GetSeveral(storage.Length);
            // Process each item in the list
            for (int i = 0; i < results.Count; i++) {
                // Instantiate new handlers each time to ensure a fresh start.
                BroadcastLoggerHandler handler = new BroadcastLoggerHandler();
                AmazonHandler amazonHandler = new AmazonHandler();
                Console.WriteLine("storage.Length: " + storage.Length);
                LogWriter.Write("Storage.Length: " + storage.Length);
                StorageResult cur = null, next = null;
                if (results[i] != null) {
                    Console.WriteLine("cur = results[i]");
                    cur = results[i];
                } else {
                    continue;
                }
                if (i + 1 < results.Count && results[i + 1] != null)
                {
                    Console.WriteLine("next = results[i + 1]");
                    next = results[i + 1];
                }
                // Authorize current item.
                // Create a local variable to lock in the auth code for the loop, 
                // just in case it changes.
                if (cur.Status == Status.NEW)
                {
                    Console.WriteLine("---------- STATUS --------- NEW");
                    LogWriter.Write("Status : New");
                    authHelper(handler, cur);
                }
                // Get recording ID
                if (cur.RecordID == "" && cur.Status == Status.AUTHORIZED)
                {
                    Console.WriteLine("---------- STATUS --------- AUTHORIZED");
                    LogWriter.Write("Status: Authorized");
                    recordCreateHelper(handler, cur);
                }

                // Set recording as 'Done' only if cur AND next both have a recording ID;
                if (cur.Status == Status.ID_CREATED && next != null && next.Status == Status.ID_CREATED)
                {
                    Console.WriteLine("---------- STATUS --------- ID_CREATED");
                    LogWriter.Write("Status: ID_Created");
                    recordDoneHelper(handler, cur, next);
                }
                else if (recordStop && cur.Status == Status.ID_CREATED)
                {
                    Console.WriteLine("---------- STATUS --------- ID_CREATED");
                    LogWriter.Write("Status: ID_Created");
                    singleRecordDoneHelper(handler, cur);
                }

                // Upload audio log.
                if (cur.Status == Status.REC_SET_DONE)
                {
                    Console.WriteLine("---------- STATUS --------- REC_SET_DONE");
                    LogWriter.Write("Status: REC_SET_DONE");
                    amazonHelper(amazonHandler, cur);
                }
                // Set audio log as uploaded.
                if (cur.Status == Status.UPLOADED)
                {
                    Console.WriteLine("---------- STATUS --------- UPLOADED");
                    LogWriter.Write("Status: UPLOADED");
                    recordUploadedHelper(handler, cur);
                }
                // Delete the local file.
                if (cur.Status == Status.SET_UPLOADED)
                {
                    Console.WriteLine("---------- STATUS --------- SET_UPLOADED");
                    LogWriter.Write("Status: SET_UPLOADED");
                    try {
                        deleteFile(cur.LocalFileName);
                        // May lead to build up of temp files.
                        storage.Pop();
                    } catch (IOException ex) {
                        StatusMessage = "Unable to delete " + cur.LocalFileName;
                        LogWriter.Write("Unable to delete: " + cur.LocalFileName);
                        Console.Write(ex.StackTrace);
                    }
                }
            }
            isHandlingQueue = false;

            if (recordStop) {
                StatusMessage = "Recording Stopped.";
                if (RecordingStopped != null) {
                    RecordingStopped();
                }
            }
        }

        /// <summary>
        /// Calls setAsUploaded handler and updates status if success.
        /// </summary>
        /// <param name="handler">BroadcastLoggerHandler instance.</param>
        /// <param name="cur">Current item in queue.</param>
        private void recordUploadedHelper(BroadcastLoggerHandler handler, StorageResult cur)
        {
            bool succeeded = handler.setAsUploaded(cur.StationID, authCode, Int32.Parse(cur.RecordID));
            if (succeeded)
            {
                cur.Status = Status.SET_UPLOADED;
                storage.UpdateStatus(Int32.Parse(cur.UploadID), Status.SET_UPLOADED);
            }
        }

        /// <summary>
        /// Calls amazon upload handler and updates status if success.
        /// </summary>
        /// <param name="handler">AmazonHandler instance.</param>
        /// <param name="cur">Current item in queue.</param>
        private void amazonHelper(AmazonHandler amazonHandler, StorageResult cur)
        {
            bool succeeded = amazonHandler.putObject(cur.LocalFileName, cur.AWSFileName, this.bucketName);
            if (succeeded)
            {
                cur.Status = Status.UPLOADED;
                storage.UpdateStatus(Int32.Parse(cur.UploadID), Status.UPLOADED);
            }
        }

        /// <summary>
        /// Calls record done handler for a single record and updates status if success.
        /// </summary>
        /// <param name="handler">BroadcastLoggerHandler instance.</param>
        /// <param name="cur">Current item in queue.</param>
        private void singleRecordDoneHelper(BroadcastLoggerHandler handler, StorageResult cur)
        {
            // First check to see if cur has finished recording; otherwise, it will still be in use
            // and any attempt to upload it will throw an IOException.
            if (isFileAvailable(cur.LocalFileName))
            {
                // Get the AWS object name (URL to upload to Amazon S3).
                bool succeeded = handler.recordingDone(cur.StationID, authCode,
                    Int32.Parse(cur.RecordID));
                if (succeeded)
                {
                    cur.AWSFileName = handler.ObjectName;
                    storage.UpdateAWSFileName(Int32.Parse(cur.UploadID), cur.AWSFileName);
                    cur.Status = Status.REC_SET_DONE;
                    storage.UpdateStatus(Int32.Parse(cur.UploadID), Status.REC_SET_DONE);
                }
            }
        }

        /// <summary>
        /// Calls record done handler and updates status if success.
        /// </summary>
        /// <param name="handler">BroadcastLoggerHandler instance.</param>
        /// <param name="cur">Current item in queue.</param>
        /// <param name="next">Next item in queue.</param>
        private void recordDoneHelper(BroadcastLoggerHandler handler, StorageResult cur, StorageResult next)
        {
            // First check to see if cur has finished recording; otherwise, it will still be in use
            // and any attempt to upload it will throw an IOException.
            if (isFileAvailable(cur.LocalFileName))
            {
                // Get the AWS object name (URL to upload to Amazon S3).
                bool succeeded = handler.recordingDone(cur.StationID, authCode,
                    Int32.Parse(cur.RecordID), Int32.Parse(next.RecordID));
                if (succeeded)
                {
                    cur.AWSFileName = handler.ObjectName;
                    storage.UpdateAWSFileName(Int32.Parse(cur.UploadID), cur.AWSFileName);
                    cur.Status = Status.REC_SET_DONE;
                    storage.UpdateStatus(Int32.Parse(cur.UploadID), Status.REC_SET_DONE);
                }
            }
        }

        /// <summary>
        /// Calls record create handler and updates status if success.
        /// </summary>
        /// <param name="handler">BroadcastLoggerHandler instance.</param>
        /// <param name="cur">Current item in queue.</param>
        private void recordCreateHelper(BroadcastLoggerHandler handler, StorageResult cur)
        {
            bool realtime = false;
            Console.WriteLine("\n\n\n\n");
            Console.WriteLine("DateTime.UtcNow: " + DateTime.UtcNow);
            Console.WriteLine("cur.CreationDate.toUniversalTime(): " + cur.CreationDate.ToUniversalTime());
            Console.WriteLine("\n\n\n\n");
            // Set realtime if this item's Creation Date is less than two seconds from the current time.
            if ((int)(cur.CreationDate.Subtract(DateTime.UtcNow)).TotalMilliseconds < 2000)
            {
                realtime = true;
            }

            bool succeeded = false;
            succeeded = handler.recordingCreate(cur.StationID, authCode, cur.CreationDate.ToUniversalTime(), realtime);

            if (succeeded)
            {
                cur.RecordID = handler.RecordingId.ToString();
                storage.UpdateRecordID(Int32.Parse(cur.UploadID), cur.RecordID);
                cur.Status = Status.ID_CREATED;
                storage.UpdateStatus(Int32.Parse(cur.UploadID), Status.ID_CREATED);

            }
            else
            {
                Console.WriteLine(handler.ErrorMessage);
            }
        }

        /// <summary>
        /// Calls authStation handler and updates status if success.
        /// </summary>
        /// <param name="handler">BroadcastLoggerHandler instance.</param>
        /// <param name="cur">Current item in queue.</param>
        private void authHelper(BroadcastLoggerHandler handler, StorageResult cur)
        {
            // boolean var is used to help with code clarity.
            // handler has a Status property, but we used this var to avoid
            // the possibility of overwriting the property.
            bool succeeded = handler.authStation(cur.StationID, authCode);
            if (succeeded)
            {
                cur.Status = Status.AUTHORIZED;
                storage.UpdateStatus(Int32.Parse(cur.UploadID), Status.AUTHORIZED);
            }
        }
        /// <summary>
        /// Checks to see if file is still in use (still recording), or is available to upload.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private bool isFileAvailable(String fileName) {
            FileStream stream = null;
            try {  
                FileInfo file = new FileInfo(fileName);
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            } catch (IOException){
                return false;
            } finally {
                if (stream != null)
                    stream.Close();
            }
            return true;
        }
        /// <summary>
        /// Asynchronous version of record()
        /// </summary>
        public Task recordAsync(Object obj = null) {
            return Task.Run(() => record(obj));
        }
        //--------------------------------------------------------------------------------------------
        /// <summary>
        /// Recording loop, called in main loop. Called by timer to start every hour on the hour.
        /// </summary>
        public async void record(Object obj = null) {
            int count = 0;
            while (!recordStop) {
                Console.WriteLine("\n\n\n");
                Console.WriteLine("----------------  " + ++count + "  -------------------");
                StatusMessage = "Creating new recording at " + DateTime.Now + "...";
                LogWriter.Write("Creating new recording at " + DateTime.Now + "...");
                Console.WriteLine("\n\n\n");
                String localFileName = "";
                timeLeft = (uint)setRecordingTime();
                //timeLeft = 5; // ### ADD BACK IN ON RELEASE

                // Get the bitrate through authStation, in case it has been updated.
                var handler = new BroadcastLoggerHandler();
                bool succeeded = handler.authStation(stationId, authCode);
                if (succeeded) {
                    bitRate = handler.BitRate;
                } else {
                    StatusMessage = handler.ErrorMessage + "\nBacklogging record.";
                    LogWriter.Write("Backlogging record");
                }

                localFileName = new FFmpegHandler("ffmpeg", "ffmpeg.exe")
                    .beginRecording(audioDevice, timeLeft, bitRate);
                storage.AddToQueue(stationId, localFileName);
                await Task.Delay((int)timeLeft * 1000);
            }
        }
        //--------------------------------------------------------------------------------------------
        /// <summary>
        /// Requestion authorization with station id and authorization code.
        /// </summary>
        public Task<bool> requestAuthorization() {
            // Authenticate login id and auth code
            return new BroadcastLoggerHandler().authStationAsync(stationId, authCode);
        }
        //--------------------------------------------------------------------------------------------
        /// <summary>
        /// Deletes the file in the stored path of local audio files.
        /// </summary>
        /// <param name="filename">Name of file to delete.  Includes filetype.</param>
        /// <returns>Whether deletion was successful or not.</returns>
        private bool deleteFile(string filename)
        {
            string path = filename;
            bool flag = false;
            if (File.Exists(path))
            {
                File.Delete(path);
                flag = true;
            }
            return flag;
        }
        //--------------------------------------------------------------------------------------------
        /// <summary>
        /// Get difference between two dates.
        /// </summary>
        /// <param name="date">Specified date.</param>
        /// <param name="origin">Origin date that comes before date.</param>
        /// <returns>Difference between the two dates in seconds.</returns>
        public double getDifferenceInSeconds(DateTime date, DateTime origin)
        {
            TimeSpan diff = date - origin;
            return Math.Floor(diff.TotalSeconds);
        }
        //--------------------------------------------------------------------------------------------
        /// <summary>
        /// Get the number of seconds remaining in the hour of the DateTime specified.
        /// </summary>
        /// <param name="date">DateTime specified.</param>
        /// <returns>Gets the remaining seconds left in the hours.</returns>
        public double getRemainderOfHour(DateTime date)
        {
            // Get the hour in DateTime format.
            DateTime hour = new DateTime(
                                            date.Year,
                                            date.Month,
                                            date.Day,
                                            date.Hour,
                                            0,
                                            0
                                        );
            return 3600 - getDifferenceInSeconds(date, hour);
        }
        
        /// <summary>
        /// Get the number of seconds remaining in the minute of the DateTime specified.
        /// </summary>
        /// <param name="date">DateTime specified.</param>
        /// <returns>Gets the remaining seconds left in the hours.</returns>
        public double getRemainderOfMinute(DateTime date)
        {
            // Get the hour in DateTime format.
            DateTime minute = new DateTime(
                                            date.Year,
                                            date.Month,
                                            date.Day,
                                            date.Hour,
                                            date.Minute,
                                            0
                                        );
            return 60 - getDifferenceInSeconds(date, minute);
        }
    }
    #endregion
}