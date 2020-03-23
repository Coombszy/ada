using System;


namespace ada.rest
{
    class Program
    {
        static void Main(string[] args)
        {
            Debugger.Write("TEST");
            Debugger.enabledDebug();
            Debugger.Write("1TEST1", 1);
            Debugger.Write("1TEST2", 2);
            Debugger.Write("1TEST3", 3);
            Debugger.Write("1TEST4", 4);
            Debugger.Write("1TEST5", 5);

            Debugger.setLevel(2);
            Debugger.Write("2TEST1", 1);
            Debugger.Write("2TEST2", 2);
            Debugger.Write("2TEST3", 3);
            Debugger.Write("2TEST4", 4);
            Debugger.Write("2TEST5", 5);

            Console.ReadLine();
        }
    }
}
