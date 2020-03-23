using System;

namespace ada.rest
{
    class Debugger
    {
        private static bool debugging = false; //if debugging is enabled or disabled, default false
        private static int debugging_level = 1; //debugging level, 1=FATAL | 5=DEBUG, default 1

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
        /// Sets the debugger level
        /// </summary>
        /// <param name="level"></param>
        public static void setLevel(int level)
        {
            Debugger.debugging_level = level;
        }

        /// <summary>
        /// Writes out to the console if debugging is enabled - level agnostic (USE CAREFULLY)
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
        /// Write out to the console if debugging is enabled and level is low enough  (1=FATAL | 2=MAJOR | 3=MINOR | 4=WARN | 5=DEBUG)
        /// </summary>
        /// <param name="toWrite"></param>
        /// <param name="loggingLevel"></param>
        /// <returns></returns>
        public static bool Write(object toWrite, int loggingLevel)
        {
            if(loggingLevel <= debugging_level)
            {
                return Write(toWrite);
            }
            else
            {
                return false;
            }
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
