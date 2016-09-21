using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Json;
using BroadcastLoggerLib;
using BroadcastLoggerLib.Handlers;
using BroadcastLoggerLib.Misc;

using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace BroadcastLoggerLib.Handlers {
    public class AmazonHandler {
        private const String URL_HOST = ".s3.amazonaws.com";
        private bool status = false;
        private IAmazonS3 amazonS3Client;

        #region REMOVE THESE ON PROJECT COMPLETION
        public static String ACCESS_KEY_ID = "";
        public static String SECRET_ACCESS_KEY = "";
        public static String REGION_ENDPOINT = "";
        private const String consolePassword = "";
        private const String consoleURL = "";
        #endregion

        public AmazonHandler(String ACCESS_KEY_ID,
            String SECRET_ACCESS_KEY,
            String regionEndPoint) {

            amazonS3Client = new AmazonS3Client(ACCESS_KEY_ID, SECRET_ACCESS_KEY, new AmazonS3Config {
                ServiceURL = regionEndPoint
            });
        }

        public Task<bool> putObjectAsync(String filePath, String keyName, String bucketName) {
            return Task.Run(() => putObject(filePath, keyName, bucketName));
        }

        public bool putObject(String filePath, String keyName, String bucketName) {
            PutObjectRequest request = new PutObjectRequest {
                BucketName = bucketName,
                Key = keyName,
                ContentType = "audio/mp4",
                FilePath = filePath,
            };
            try {
                PutObjectResponse response = amazonS3Client.PutObject(request);
            } catch (Amazon.S3.AmazonS3Exception ex) {
                Console.WriteLine(ex.StackTrace);
                status = false;
                return false;
            }
            status = true;
            return true;
        }

        public bool Status {
            get {
                return status;
            }
        }

    }
}
