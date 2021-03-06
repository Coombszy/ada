﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace ada.synthesizer
{
    class Program
    {
        static void Main(string[] args)
        {
            #region DEBUG
            Debugger.enableDebug();
            Debugger.setLevel(5);
            #endregion

            Debugger.Write("ada.synthesizer MAIN() ENTRY", 5);

            // Define system environment var for google auth
            if (!File.Exists("keys/coombszyapi.json"))
            {
                Debugger.Write("Failed to load api key, NAME=coombszyapi.json", 1);
                Debugger.AppCrash();
            }
            else
            {
                System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "keys/coombszyapi.json");
            }

            // Main while loop conditional
            bool running = true;

            // URI prefixes are required
            var prefixes = new List<string>() { "http://*:40402/" };

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
                HttpListenerContext context = listener.GetContext();

                Thread newThread = new Thread(createWorker);
                newThread.Start(context);
            }
        }

        private static void createWorker(Object listener)
        {
            Worker worker = new Worker((HttpListenerContext)listener);
        }
    }
}
