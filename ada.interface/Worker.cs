using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Buffers.Text;

namespace ada.interface_
{
    class Worker
    {
        public Worker(HttpListenerContext context)
        {
            Debugger.Write($"Worker Thread Started ID:{Thread.CurrentThread.ManagedThreadId}");

            // Get context
            HttpListenerRequest request = context.Request;

            string requestContents;
            using (Stream receiveStream = request.InputStream)
            {
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    requestContents = readStream.ReadToEnd();
                }
            }
            // Debug output
            Debugger.Write($"Received request for {request.Url}", 5);
            Debugger.Write($"Request headers: {request.Headers}", 5);
            Debugger.Write($"Request contents: {requestContents}", 5);

            // Obtain a response object
            HttpListenerResponse response = context.Response;


            // Get the Json object from the request
            JObject requestJson = JObject.Parse(requestContents);
            JToken adaInstruction;

            // Check if the correct key in json is present
            if (!requestJson.TryGetValue("adaInstruction", out adaInstruction))
            {
                Debugger.Write("Request did not contain key: adaInstruction", 2);
                sendFail("Request did not contain key: adaInstruction", response);
            }
            else
            {
                Debugger.Write($"Request contains adaInstruction key. Value : '{adaInstruction.ToString()}'", 5);

                JObject testResponse = JObject.FromObject(new
                {
                    adaInterfaceStatus = $"TESTRESPONSE"
                });

                sendResponse(testResponse, response);
            }

            Debugger.Write($"Worker Thread Closing ID:{Thread.CurrentThread.ManagedThreadId}");
        }


        /// <summary>
        /// Sends a Json object as a response to the http request
        /// </summary>
        /// <param name="toSend"></param>
        /// <param name="response"></param>
        private void sendResponse(JObject toSend, HttpListenerResponse response, int status)
        {
            // Construct a response
            byte[] buffer = Encoding.UTF8.GetBytes(toSend.ToString());

            // Get a response stream and write the response to it
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.ContentLength64 = buffer.Length;
            response.StatusCode = status;
            response.ContentType = "application/json";
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);

            // close the output stream
            output.Close();
        }

        /// <summary>
        /// Calls sendResponse method with status code 200
        /// </summary>
        /// <param name="toSend"></param>
        /// <param name="response"></param>
        private void sendResponse(JObject toSend, HttpListenerResponse response)
        {
            sendResponse(toSend, response, 200);
        }

        /// <summary>
        /// Sends a Json object containing a fail response
        /// </summary>
        /// <param name="failReason"></param>
        /// <param name="response"></param>
        private void sendFail(string failReason, HttpListenerResponse response)
        {
            JObject failResponse = JObject.FromObject(new
            {
                adaInterfaceStatus = $"FAIL: {failReason}"
            });
            sendResponse(failResponse, response, 400);
        }
    }
}
