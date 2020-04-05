using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Newtonsoft.Json.Linq;
using System.IO;

namespace ada.rest
{
    class VoiceSynth
    {
        // Sythesizer address
        private static string target = "http://localhost:40402";

        /// <summary>
        /// This will request from the sythesizer a voice byte stream within a Json
        /// </summary>
        /// <param name="toSay"></param>
        /// <returns></returns>
        public static JObject getVoiceAudio(string toSay)
        {
            // Create request
            WebRequest request = WebRequest.Create(target);

            // Set the request type to JSON
            request.ContentType = "application/json";
            request.Method = "POST";

            // Create a data stream to contain the query
            Stream dataStream = request.GetRequestStream();

            // Write request to request streams
            byte[] byteArray = Encoding.UTF8.GetBytes(createQuery(toSay).ToString());
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            // Get the request content
            WebResponse response = request.GetResponse();
            string responseContent;
            using (Stream receiveStream = response.GetResponseStream())
            {
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    responseContent = readStream.ReadToEnd();
                }
            }

            return JObject.Parse(responseContent);
        }

        /// <summary>
        /// Creates a json object containing the audio to be generated
        /// </summary>
        /// <param name="toSay"></param>
        /// <returns></returns>
        private static JObject createQuery(string toSay)
        {
            JObject response = JObject.FromObject(new
            {
                textToConvert = toSay
            });

            return response;
        }
    }
}
