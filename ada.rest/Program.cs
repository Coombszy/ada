using System;


namespace ada.rest
{
    class Program
    {
        static void Main(string[] args)
        {
            Debugger.Write("TEST1");
            Debugger.enabledDebug();
            Debugger.Write("TEST2");
            Console.ReadLine();
        }
    }
}
