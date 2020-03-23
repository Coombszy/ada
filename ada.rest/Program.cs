using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ada.rest
{
    class Program
    {
        static void Main(string[] args)
        {
            #region DEBUG
            Debugger.enableDebug();
            Debugger.setLevel(5);
            #endregion

            Debugger.Write("ada.api MAIN() ENTRY", 5);

            // Main while loop conditional
            bool running = true;

            // URI prefixes are required
            var prefixes = new List<string>() { "http://*:40401/" };

            // Create a listener
            HttpListener listener = new HttpListener();

            // Add the prefixes
            foreach (string s in prefixes)
            {
                listener.Prefixes.Add(s);
            }
            listener.Start();

            Debugger.Write("HTTPListener is now Listening", 5);

            while (running)
            {
                // Get context
                HttpListenerContext context = listener.GetContext();

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

                // Construct a response
                string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
                byte[] buffer = Encoding.UTF8.GetBytes(responseString);

                // Get a response stream and write the response to it
                response.ContentLength64 = buffer.Length;
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);

                // close the output stream
                output.Close();
            }
        }
    }
}
