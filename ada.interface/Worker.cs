using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

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
            HttpListenerResponse responseObject = context.Response;

            // Get the Json object from the request
            JObject requestJson = JObject.Parse(requestContents);

            if (request.Url.ToString().Contains("QUEUE"))
            {
                handleQueue(requestJson, responseObject);
            }
            else
            {
                sendFail($"Invalid URL: {request.Url.ToString()}", responseObject);
            }

            Debugger.Write($"Worker Thread Closing ID:{Thread.CurrentThread.ManagedThreadId}");
        }

        /// <summary>
        /// Handles all logic for QUEUE URL
        /// </summary>
        /// <param name="response"></param>
        /// <param name="requestContents"></param>
        /// <returns></returns>
        private void handleQueue(JObject requestJson, HttpListenerResponse responseObject)
        {
            // Required keys for queue functionality
            List<string> requiredKeys = new List<string>() { "adaQueueInstruction" };

            // If requied keys are not present, throw error response
            if(!containsCorrectKeys(requiredKeys, requestJson))
            {
                sendFail("QUEUE", $"Json did not contain required keys for QUEUE: {string.Join(',', requiredKeys.ToArray())}", responseObject);
                return;
            }

            // If queue request is pushing new messages
            if(requestJson.GetValue("adaQueueInstruction").ToString() == "PUSH")
            {
                // Add addition required keys and check they are present
                requiredKeys.AddRange(new List<string>() { "adaQueueMessageTarget", "adaQueueMessageContent", "adaQueueMessageTimeToConsume" });
                if (!containsCorrectKeys(requiredKeys, requestJson))
                {
                    sendFail("QUEUE", $"Json did not contain required keys for PUSH intruction to QUEUE: {string.Join(',', requiredKeys.ToArray())}", responseObject);
                    return;
                }

                // Creates message object from Json
                Message newMessage = new Message(
                    requestJson.GetValue("adaQueueMessageTarget").ToString(),
                    requestJson.GetValue("adaQueueMessageContent").ToString(),
                    Convert.ToDateTime(requestJson.GetValue("adaQueueMessageTimeToConsume").ToString())
                );

                // Add message to queue
                if (Queue.pushMessage(newMessage))
                {
                    sendResponse("QUEUE", "Successfully pushed to queue", responseObject);
                }
                else
                {
                    sendFail("QUEUE", "Failed to push to queue", responseObject);
                }
            }
            // If queue request is requesting messages
            else if (requestJson.GetValue("adaQueueInstruction").ToString() == "REQUEST")
            {
                // Add addition required keys and check they are present
                requiredKeys.AddRange(new List<string>() { "adaQueueMessageTarget" });
                if (!containsCorrectKeys(requiredKeys, requestJson))
                {
                    sendFail("QUEUE", $"Json did not contain required keys for REQUEST intruction to QUEUE: {string.Join(',', requiredKeys.ToArray())}", responseObject);
                    return;
                }

                // Get message from queue for target
                Message returnMessage = Queue.getNextMessage(requestJson.GetValue("adaQueueMessageTarget").ToString());

                // Return message back
                if (returnMessage != null)
                {
                    JObject response = JObject.FromObject(new
                    {
                        adaQueueMessageContent = returnMessage.getContent()

                    });
                    sendResponse(response, responseObject, 200);
                }
                // Return message with error due to no messages
                else
                {
                    JObject response = JObject.FromObject(new
                    {
                        adaQueueError = "NOMESSAGE"

                    });
                    sendResponse(response, responseObject, 500);
                }

            }
            // if queue request is requesting all messages for a target
            else if (requestJson.GetValue("adaQueueInstruction").ToString() == "LIST")
            {
                // Add addition required keys and check they are present
                requiredKeys.AddRange(new List<string>() { "adaQueueMessageTarget" });
                if (!containsCorrectKeys(requiredKeys, requestJson))
                {
                    sendFail("QUEUE", $"Json did not contain required keys for REQUEST intruction to QUEUE: {string.Join(',', requiredKeys.ToArray())}", responseObject);
                    return;
                }

                Message[] returnMessages = Queue.getMessages(requestJson.GetValue("adaQueueMessageTarget").ToString());

                // Return messages back
                if (returnMessages != null)
                {
                    // Convert message contents to list of strings
                    List<string> messagesContents = new List<string>();
                    foreach (Message msg in returnMessages)
                    {
                        messagesContents.Add(msg.getContent());
                    }

                    JObject response = JObject.FromObject(new
                    {
                        adaQueueMessages = JsonConvert.SerializeObject(messagesContents)

                    });
                    sendResponse(response, responseObject, 200);
                }
                // Return message with error due to no messages
                else
                {
                    JObject response = JObject.FromObject(new
                    {
                        adaQueueError = "NOMESSAGES"

                    });
                    sendResponse(response, responseObject, 500);
                }
            }
            else
            {
                // if an invalid message queue option
                sendFail("QUEUE", "Invalid queue option", responseObject);
            }
        }

        /// <summary>
        /// Checks if all keys are preset in request Json, returns true if all are present
        /// </summary>
        /// <param name="keyList"></param>
        /// <param name="requestJson"></param>
        /// <returns></returns>
        private bool containsCorrectKeys(List<string> keyList, JObject requestJson)
        {
            foreach(string key in keyList)
            {
                if (!requestJson.ContainsKey(key))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Sends a Json object as a response to the http request
        /// </summary>
        /// <param name="toSend"></param>
        /// <param name="responseObject"></param>
        private void sendResponse(JObject toSend, HttpListenerResponse responseObject, int status)
        {
            // Construct a response
            byte[] buffer = Encoding.UTF8.GetBytes(toSend.ToString());

            // Get a response stream and write the response to it
            responseObject.Headers.Add("Access-Control-Allow-Origin", "*");
            responseObject.ContentLength64 = buffer.Length;
            responseObject.StatusCode = status;
            responseObject.ContentType = "application/json";
            Stream output = responseObject.OutputStream;
            output.Write(buffer, 0, buffer.Length);

            // close the output stream
            output.Close();
        }

        /// <summary>
        /// Calls sendResponse method with status code 200
        /// </summary>
        /// <param name="toSend"></param>
        /// <param name="responseObject"></param>
        private void sendResponse(JObject toSend, HttpListenerResponse responseObject)
        {
            sendResponse(toSend, responseObject, 200);
        }

        /// <summary>
        /// Calls sendResponse method with status code 200
        /// </summary>
        /// <param name="sourceString"></param>
        /// <param name="responseString"></param>
        private void sendResponse(string sourceString, string responseString, HttpListenerResponse responseObject)
        {
            JObject response = JObject.FromObject(new
            {
                adaInterfaceSource = $"{sourceString}",
                adaInterfaceResponse = $"{responseString}",
            });
            sendResponse(response, responseObject, 200);
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
                adaInterfaceSource = $"{failSource}",
                adaInterfaceResponse = $"{failReason}"
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
    }
}
