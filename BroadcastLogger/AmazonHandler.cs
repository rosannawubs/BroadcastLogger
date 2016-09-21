using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Threading;

using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.EC2;

namespace BroadcastLogger {
    public class AmazonHandler {
        private const String URL_HOST = ".s3.amazonaws.com";
        private const String ACCESS_KEY_ID = "AKIAJLWO6RA7AG3U3CPQ";
        private const String SECRET_ACCESS_KEY = "a3E0WUqmiIVgT0m4o1D26pVhJ15TwffqWfk6Rj7y";
        String example = "https://BUCKETNAME.s3.amazonaws.com/PATH/TO/FILE";

        public String uploadFile(String bucket, String filename) {
            var client = new WebClient();
            String hash = Hasher.calculateMD5Hash(bucket);
            client.Headers.Add("Content-MD5", hash);
            client.Headers.Add("Expect", "100-continue");
            byte[] sumfin = client.UploadFile(bucket + URL_HOST, "PUT", filename);

            String s = System.Text.Encoding.UTF8.GetString(sumfin);
            Console.WriteLine(s);
            return s;
            //client.UploadFileAsync(new Uri(bucket + URL_HOST), "PUT", filename);
        }

        public String uploadFile2(String bucket, String filename) {
            WebRequest request = WebRequest.Create("http://sumfin.s3.amazonaws.com");
            request.Method = "PUT";

            StreamWriter requestWriter = new StreamWriter(request.GetRequestStream());
            requestWriter.Write(filename);
            WebResponse response = request.GetResponse();
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            Console.WriteLine("RESPONSE: ");
            Console.WriteLine(responseFromServer);
            // Clean up the streams and the response.
            reader.Close();
            response.Close();
            return responseFromServer;

        }







        //static IAmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client();
        static IAmazonS3 client = new AmazonS3Client();
        
        public static void ListingBuckets() {
            try {
                ListBucketsResponse response = client.ListBuckets();
                foreach (S3Bucket bucket in response.Buckets) {
                    Console.WriteLine("You own Bucket with name: {0}", bucket.BucketName);
                }
            } catch (AmazonS3Exception amazonS3Exception) {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity"))) {
                    Console.WriteLine("Please check the provided AWS Credentials.");
                    Console.WriteLine("If you haven't signed up for Amazon S3, please visit http://aws.amazon.com/s3");
                } else {
                    Console.WriteLine("An Error, number {0}, occurred when listing buckets with the message '{1}", amazonS3Exception.ErrorCode, amazonS3Exception.Message);
                }
            }
        }

        static String bucketName = "broadcastlogger-audio";
        static String keyName = "testkey";
        public static void WriteObject() {
            try {
                // simple object put
                /*
                PutObjectRequest request = new PutObjectRequest() {
                    ContentBody = "this is a test",
                    BucketName = bucketName,
                    Key = keyName                    
                };
                Console.WriteLine("simple object put");
                PutObjectResponse response = client.PutObject(request);
                */

                // put a more complex object with some metadata and http headers.
                Console.WriteLine("complex object put");
                PutObjectRequest titledRequest = new PutObjectRequest() {
                    BucketName = bucketName,
                    Key = keyName,
                    FilePath = @"C:\Users\Jens\Desktop\openui5-sdk-1.26.8.zip"
                };
                titledRequest.Metadata.Add("title", "the title");

                client.PutObject(titledRequest);
            } catch (AmazonS3Exception amazonS3Exception) {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity"))) {
                    Console.WriteLine("Please check the provided AWS Credentials.");
                    Console.WriteLine("If you haven't signed up for Amazon S3, please visit http://aws.amazon.com/s3");
                } else {
                    Console.WriteLine("An error occurred with the message '{0}' when writing an object", amazonS3Exception.Message);
                }
            }
        }

        public static void ReadObject() {
            try {
                GetObjectRequest request = new GetObjectRequest() {
                    BucketName = bucketName,
                    Key = keyName
                };

                using (GetObjectResponse response = client.GetObject(request)) {
                    string title = response.Metadata["x-amz-meta-title"];
                    
                    Console.WriteLine("The object's title is {0}", title);
                    Console.WriteLine("The object's bucket name is {0}", response.BucketName);
                    Console.WriteLine("The object's key is {0}", response.Key);

                    string dest = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), keyName);
                    if (!File.Exists(dest)) {
                        response.WriteResponseStreamToFile(dest);
                    }
                }
            } catch (AmazonS3Exception amazonS3Exception) {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity"))) {
                    Console.WriteLine("Please check the provided AWS Credentials.");
                    Console.WriteLine("If you haven't signed up for Amazon S3, please visit http://aws.amazon.com/s3");
                } else {
                    Console.WriteLine("An error occurred with the message '{0}' when reading an object", amazonS3Exception.Message);
                }
            }
        }




        class ClientState {
            AmazonS3Client client;
            DateTime startTime;

            public AmazonS3Client Client {
                get { return client; }
                set { client = value; }
            }

            public DateTime Start {
                get { return startTime; }
                set { startTime = value; }
            }
        }

        public static void TestPutObjectAsync() {
            
            // Create a client
            AmazonS3Client client = new AmazonS3Client(ACCESS_KEY_ID, SECRET_ACCESS_KEY);
            
            PutObjectResponse response;
            IAsyncResult asyncResult;

            
            PutObjectRequest request = new PutObjectRequest {
                BucketName = "house",
                Key = "Item",
                //ContentType = "audio/mp4",
                //FilePath = "c:/users/Jens/dekstop/temp.java",
                ContentBody = "This is sample content..."
            };
            
            response = client.PutObject(request);
            Console.WriteLine("Finished PutObject operation for {0}.", request.Key);
            Console.WriteLine("Service Response:");
            Console.WriteLine("-----------------");
            Console.WriteLine("{0}", response);
            Console.Write("\n\n");

            request.Key = "Item1";
            /*
            asyncResult = client.BeginPutObject(request, null, null);
            while (!asyncResult.IsCompleted) {
                //
                // Do some work here
                //
            }
            try {
                response = client.EndPutObject(asyncResult);
            } catch (AmazonS3Exception s3Exception) {
            }

            Console.WriteLine("Finished Async PutObject operation for {0}.", request.Key);
            Console.WriteLine("Service Response:");
            Console.WriteLine("-----------------");
            Console.WriteLine(response);
            Console.Write("\n\n");

            request.Key = "Item2";
            asyncResult = client.BeginPutObject(request, SimpleCallback, null);

            request.Key = "Item3";
            asyncResult = client.BeginPutObject(request, CallbackWithClient, client);

            request.Key = "Item4";
            asyncResult = client.BeginPutObject(request, CallbackWithState,
               new ClientState { Client = client, Start = DateTime.Now });

            Thread.Sleep(TimeSpan.FromSeconds(5));
             */
        }

        public static void SimpleCallback(IAsyncResult asyncResult) {
            Console.WriteLine("Finished PutObject operation with simple callback");
        }

        /*
        public static void CallbackWithClient(IAsyncResult asyncResult) {
            try {
                AmazonS3Client s3Client = (AmazonS3Client)asyncResult.AsyncState;
                PutObjectResponse response = s3Client.EndPutObject(asyncResult);
                Console.WriteLine("Finished PutObject operation with client callback");
            } catch (AmazonS3Exception s3Exception) {
            }
        }

        public static void CallbackWithState(IAsyncResult asyncResult) {
            try {
                ClientState state = asyncResult.AsyncState as ClientState;
                AmazonS3Client s3Client = (AmazonS3Client)state.Client;
                PutObjectResponse response = state.Client.EndPutObject(asyncResult);
                Console.WriteLine("Finished PutObject. Elapsed time: {0}",
                  (DateTime.Now - state.Start).ToString());
            } catch (AmazonS3Exception s3Exception) {
            }
        }
         */

    }
}
