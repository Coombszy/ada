using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Text;
using Google.Cloud.TextToSpeech.V1;
using Newtonsoft.Json.Linq;

namespace ada.synthesizer
{
    class Worker
    {
        public Worker(HttpListenerContext context)
        {
            Debugger.Write($"Worker Thread Started ID:{Thread.CurrentThread.ManagedThreadId}", 5);

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
            // Debug outputs
            Debugger.Write($"Received request for {request.Url}", 5);
            Debugger.Write($"Request headers: {request.Headers}", 5);
            Debugger.Write($"Request body: \n{requestContents}", 5);

            // Obtain a response object
            HttpListenerResponse response = context.Response;

            // Get the Json object from the request
            JObject requestJson = JObject.Parse(requestContents);
            JToken textToConvertJToken;

            // Check if the correct body is present
            if(!requestJson.TryGetValue("textToConvert", out textToConvertJToken))
            {
                Debugger.Write("Request did not contain key: textToConvert", 2);
                sendFail("Request did not contain key: textToConvert", response);
            }
            else
            {
                Debugger.Write($"Request contains textToConvert key. Value : '{textToConvertJToken.ToString()}'", 5);
                sendResponse(createResponse(textToConvertJToken.ToString().ToLower()), response);
            }
            
            Debugger.Write($"Worker Thread Closing ID:{Thread.CurrentThread.ManagedThreadId}", 5);
        }

        /// <summary>
        /// Creates a response Json containing a pass message and voice stream
        /// </summary>
        /// <param name="textToConvert"></param>
        /// <returns></returns>
        private JObject createResponse(string textToConvert)
        {
            // If the file does not exist, sythesize it
            if (!File.Exists($"stored_responses/{textToConvert.Replace(' ', '_')}.mp3"))
            {
                Debugger.Write($"File '{textToConvert.Replace(' ', '_')}.mp3' could not be found, Generating new file...", 5);
                synthesizeVoice(textToConvert);
            }
            else
            {
                Debugger.Write($"File '{textToConvert.Replace(' ', '_')}.mp3' was found!", 5);
            }

            byte[] voiceStream = getFileBytes(textToConvert);

            JObject response = JObject.FromObject(new
            {
                voiceResponseStatus = "PASS",
                voiceResponseBytes = Convert.ToBase64String(voiceStream)
            });

            return response;
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
        /// Sends a Json object containing a fail response and source
        /// </summary>
        /// <param name="failReason"></param>
        /// <param name="responseObject"></param>
        private void sendFail(string failSource, string failReason, HttpListenerResponse responseObject)
        {
            JObject failResponse = JObject.FromObject(new
            {
                adaSynthesizerSource = $"{failSource}",
                adaSynthesizerResponse = $"{failReason}"
            });
            sendResponse(failResponse, responseObject, 400);
        }

        /// <summary>
        /// Sends a Json object containing a fail response with generic source
        /// </summary>
        /// <param name="failReason"></param>
        /// <param name="responseObject"></param>
        private void sendFail(string failReason, HttpListenerResponse responseObject)
        {
            sendFail("GENERIC", failReason, responseObject);
        }

        /// <summary>
        /// Using environment vars, it connects to google api to generate text to speech
        /// </summary>
        /// <param name="text"></param>
        private static void synthesizeVoice(string text)
        {
            TextToSpeechClient client = TextToSpeechClient.Create();
            var response = client.SynthesizeSpeech(new SynthesizeSpeechRequest
            {
                Input = new SynthesisInput
                {
                    Text = text
                },
                // Note: voices can also be specified by name
                Voice = new VoiceSelectionParams
                {
                    LanguageCode = "en-US",
                    Name = "en-US-Wavenet-F"
                },
                AudioConfig = new AudioConfig
                {
                    AudioEncoding = AudioEncoding.Mp3,
                    SampleRateHertz = 32000,
                    SpeakingRate = 1.0
                }
            });
            Debugger.Write("Successfully downloaded voice audio from google api");

            using (Stream output = File.Create($"stored_responses/{text.Replace(' ','_')}.mp3"))
            {
                response.AudioContent.WriteTo(output);
            }
        }

        /// <summary>
        /// Aborts from the worker
        /// </summary>
        private static void closeWorker()
        {
            Thread currentThread = Thread.CurrentThread;
            Debugger.Write($"Worker Thread Aborting ID:{Thread.CurrentThread.ManagedThreadId}", 5);
            currentThread.Abort();
        }

        /// <summary>
        /// Returns a byte array of the target voice file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static byte[] getFileBytes(string fileName)
        {
            try
            {
                byte[] stream = File.ReadAllBytes($"stored_responses/{fileName.Replace(' ', '_')}.mp3");
                return stream;
            }
            catch (FileNotFoundException ioEx)
            {
                Debugger.Write(ioEx.Message, 1);
                return null;
            }
        }
    }
}
