using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Web;
using System.Json;
using BroadcastLoggerLib.Misc;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;

namespace BroadcastLoggerLib.Handlers {
    public class HttpHandler
    {
       /* public HttpHandler() {
            ServicePointManager.DefaultConnectionLimit = 100;
        }*/
        public Task<string> executeAsync(string url, string postData)
        {
            return Task.Run(() => execute(url, postData));
        }

        public String execute(String url, String postData)
        {
#if true
            // Create a request for the URL.
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.KeepAlive = false;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            StreamWriter requestWriter = new StreamWriter(request.GetRequestStream());
            requestWriter.Write(postData);
            requestWriter.Close();

            // Get the response.
            WebResponse response = request.GetResponse();
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            // Clean up the streams and the response.
            reader.Close();
            response.Close();

            return responseFromServer;
#endif



#if false
            String result;
            using (var client = new HttpClient()) {
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                HttpContent content = new StringContent(postData);
                // and add the header to this object instance
                // optional: add a formatter option to it as well
                content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                // synchronous request without the need for .ContinueWith() or await
                var response = client.PostAsync(url, content).Result;
                result = response.Content.ReadAsStringAsync().Result;
            }
            return result;
#endif




#if false
            WebClient client = new WebClient();
            client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            // Apply ASCII Encoding to obtain the string as a byte array. 
            byte[] byteArray = Encoding.ASCII.GetBytes(postData);
            // Upload the input string using the HTTP 1.0 POST method. 
            byte[] result = client.UploadData(url, "POST", byteArray);
            return Encoding.ASCII.GetString(result);
#endif
        }
    }
}