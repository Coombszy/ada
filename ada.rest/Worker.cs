using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Text;

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

            // Construct a response
            string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);

            // Get a response stream and write the response to it
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);

            // close the output stream
            output.Close();
            Debugger.Write($"Worker Thread Closing ID:{Thread.CurrentThread.ManagedThreadId}");
        }
    }
}
