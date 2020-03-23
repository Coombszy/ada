using System;
using System.Collections.Generic;
using System.Text;

namespace ada.rest
{
    class Debugger
    {
        private static bool debugging = false; //if debugging is enabled or disabled, default false

        /// <summary>
        /// Disable and enable debuggins
        /// </summary>
        public static void enabledDebug()
        {
            Debugger.debugging = true;
        }
        public static void disableDebug()
        {
            Debugger.debugging = false;
        }

        /// <summary>
        /// Writes out to the console if debugging is enabled
        /// </summary>
        /// <param name="toWrite"></param>
        /// <returns></returns>
        public static bool Write(object toWrite)
        {
            if (debugging)
            {
                try
                {
                    Console.WriteLine(getTimeStamp() + toWrite.ToString());
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(getTimeStamp() + "DEBUGGER-FATAL:" + e.ToString());
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns timestamp for console log
        /// </summary>
        /// <returns></returns>
        private static string getTimeStamp()
        {
            return "[" + System.DateTime.UtcNow.ToLongTimeString() + " | " + System.DateTime.UtcNow.ToLongDateString() + "]:";
        }
    }
}
