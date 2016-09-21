using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Json;
using System.Net;
using BroadcastLoggerLib.Handlers;
using BroadcastLoggerLib.Misc;
using System.Text.RegularExpressions;
using System.Threading;

namespace BroadcastLoggerLib.Handlers {
    public class BroadcastLoggerHandler {
        private static bool status;
        private int bitRate;
        private int recordingId;
        private String stationName;
        private String errorMessage;
        private String objectName;
        private String localFileName;

        private String API_KEY = "8JFdiD6SkfdmZDKFIE6DSf6567saFUD";
        private String API_URL = "http://api.broadcastlogger.com/api/";

        public BroadcastLoggerHandler(String apiKey = "8JFdiD6SkfdmZDKFIE6DSf6567saFUD",
            String apiURL = "http://api.broadcastlogger.com/api/") {
                API_KEY = apiKey;
                API_URL = apiURL;
        }

        public Task<bool> authStationAsync(string stationId, string authCode) {
            return Task.Run(() => authStation(stationId, authCode));
        }

        // sends a request to the BL API to verify the station settings input in the window
        public bool authStation(String stationId, String authCode) {
            // assemble the string to be hashed
            String hashFormat = API_KEY + "-app_auth_station-" + stationId + "-" + authCode;
            // assemble the post data with the hashed string
            String data = "auth=" + Hasher.calculateMD5Hash(hashFormat);
            // assemble the api url
            String url = API_URL + "app_auth_station/" + stationId + "/" + authCode;
            // get json object from server
            String result = null;
            try
            {
                result = new HttpHandler().execute(url, data);
            }
            catch (WebException) { return false; }
            JsonObject json = (JsonObject)JsonObject.Parse(result);
            status = Convert.ToBoolean((int)Convert.ToInt32((String)json["status"]));
            if (!status) {
                errorMessage = (String)json["message"];
                return false;
            }

            // Get stationInfo inner json object
            JsonObject stationInfo = (JsonObject)json["stationinfo"];
            stationName = (String)stationInfo["name"];
            // get bitrate string from stationInfo json object
            String bitRateString = (String)stationInfo["bitrate"];

            // parse the number from the bitrate
            var match = new Regex(@"\d+").Match(bitRateString);
            string matchString = match.Value;
            bitRate = (int)Convert.ToInt32(matchString);
            return status;
        }

        public Task<bool> recordingCreateAsync(String stationId, String authCode, DateTime timestamp, bool realtime) {
            return Task.Run(() => recordingCreate(stationId, authCode, timestamp, realtime));
        }

        public bool recordingCreate(String stationId, String authCode, DateTime timestamp, bool realtime) {
            // assemble the string to be hashed
            String real = realtime ? "-realtime" : "";
            int unixTimestamp = (int)(timestamp.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            String hashFormat = API_KEY + "-app_recording_create-" + stationId + "-" + authCode + "-" + unixTimestamp + real;
            // assemble the post data with the hashed string
            String data = "auth=" + Hasher.calculateMD5Hash(hashFormat);
            // assemble the api url
            real = realtime ? "/realtime" : "";
            String url = API_URL + "app_recording_create/" + stationId + "/" + authCode + "/" + unixTimestamp + real;
            String result = null;
            try
            {
                result = new HttpHandler().execute(url, data);
            }
            catch (WebException) { return false; }

            // Get json from server
            JsonObject json = (JsonObject)JsonObject.Parse(result);
            status = Convert.ToBoolean((int)Convert.ToInt32((String)json["status"]));
            if (!status) {
                errorMessage = (String)json["message"];
                return false;
            }

            recordingId = (int)json["recording_id"];
            return status;
        }

        public Task<bool> recordingDoneAsync(String stationId, String authCode, int recordingID, int nextRecordingId = -1) {
            return Task.Run(() => recordingDone(stationId, authCode, recordingId, nextRecordingId));
        }

        public bool recordingDone(String stationId, String authCode, int recordingID, int nextRecordingId = -1) {
            // assemble the string to be hashed
            String hashFormat = API_KEY + "-app_recording_set_done-" + stationId + "-" + authCode + "-" + recordingID;
            // assemble the api url
            String url = API_URL + "app_recording_set_done/" + stationId + "/" + authCode + "/" + recordingID;
            if (nextRecordingId != -1) {
                hashFormat += "-" + nextRecordingId;
                url += "/" + nextRecordingId;
            }

            // assemble the post data with the hashed string
            String postData = "auth=" + Hasher.calculateMD5Hash(hashFormat);
            // get json object from server
            String result = null;
            try
            {
                result = new HttpHandler().execute(url, postData);
            }
            catch (WebException) { return false; }
            JsonObject json = (JsonObject)JsonObject.Parse(result);
            status = Convert.ToBoolean((int)Convert.ToInt32((String)json["status"]));
            if (!status) {
                errorMessage = (String)json["message"];
                return false;
            }

            objectName = (String)json["awsobjectname"];
            return status;
        }

        public Task<bool> setAsUploadedAsync(String stationId, String authCode, int recordingID) {
            return Task.Run(() => setAsUploaded(stationId, authCode, recordingId));
        }

        public bool setAsUploaded(String stationId, String authCode, int recordingID) {
            // assemble the string to be hashed
            String hashFormat = API_KEY + "-app_recording_set_uploaded-" + stationId + "-" + authCode + "-" + recordingID;
            // assemble the post data with the hashed string
            String data = "auth=" + Hasher.calculateMD5Hash(hashFormat);
            // assemble the api url
            String url = API_URL + "app_recording_set_uploaded/" + stationId + "/" + authCode + "/" + recordingID;
            // get json object from server
            String result = null;
            try
            {
                result = new HttpHandler().execute(url, data);
            }
            catch (WebException) { return false; }
            JsonObject json = (JsonObject)JsonObject.Parse(result);
            status = Convert.ToBoolean((int)Convert.ToInt32((String)json["status"]));
            if (!status) {
                errorMessage = (String)json["message"];
                return false;
            }
            return status;
        }

        /// <summary>
        /// The appropriate bit rate for recording, obtained after calling authStation
        /// </summary>
        public int BitRate {
            get {
                if (status) {
                    return bitRate;
                } else {
                    return -1;
                }
            }
        }

        /// <summary>
        /// The full name of the station obtained after calling authStation
        /// </summary>
        public String StationName {
            get {
                if (status) {
                    return stationName;
                } else {
                    return "ERROR: " + ErrorMessage;
                }
            }
        }

        /// <summary>
        /// The recording id obtained after calling recordingCreate
        /// </summary>
        public int RecordingId {
            get {
                if (status) {
                    return recordingId;
                } else {
                    return -1;
                }
            }
        }

        /// <summary>
        /// The Key Name to be uploaded to Amazon S3.
        /// Can be accessed after calling recordingDone
        /// </summary>
        public String ObjectName {
            get {
                if (status) {
                    return objectName;
                } else {
                    return "ERROR: " + ErrorMessage;
                }
            }
        }

        /// <summary>
        /// The local path to the audio log on.
        /// </summary>
        public String LocalFileName {
            get { return localFileName; }
            set { localFileName = value; }
        }

        /// <summary>
        /// The status after the most recently-called method.
        /// </summary>
        public bool Status {
            get {
                return status;
            }
        }

        /// <summary>
        /// The error message after the most recently-called method that failed.
        /// </summary>
        public String ErrorMessage {
            get {
                return errorMessage;
            }
        }

    }
}
