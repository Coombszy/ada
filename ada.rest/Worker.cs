using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Buffers.Text;

namespace ada.rest
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


            //==============================================================================================================
            //PROTOTYPE - Testing generation of text API calls to synthesizer ==============================================

            // Get the Json object from the request
            JObject requestJson = JObject.Parse(requestContents);
            JToken adaQuery;

            // Check if the correct key in json is present
            if (!requestJson.TryGetValue("adaQuery", out adaQuery))
            {
                Debugger.Write("Request did not contain key: adaQuery", 2);
                sendFail("Request did not contain key: adaQuery", response);
            }
            else
            {
                Debugger.Write($"Request contains adaQuery key. Value : '{adaQuery.ToString()}'", 5);
                sendResponse(convertSynthJson(VoiceSynth.getVoiceAudio(adaQuery.ToString())), response);
            }

            // THIS CODE IS TO BE REMOVED WHEN MEMORY UNIT IS COMPLETE =====================================================
            //==============================================================================================================

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
                adaQueryStatus = $"FAIL: {failReason}"
            });
            sendResponse(failResponse, response, 400);
        }

        /// <summary>
        /// Converts the json from the synthesizer to a rest layer json
        /// </summary>
        /// <param name="jsonToConvert"></param>
        /// <returns></returns>
        private JObject convertSynthJson(JObject jsonToConvert)
        {
            JObject response = JObject.FromObject(new
            {
                adaQueryStatus = "PASS",
                adaQueryVoiceBytes = jsonToConvert.GetValue("voiceResponseBytes")
            });

            return response;
        }
    }
}
