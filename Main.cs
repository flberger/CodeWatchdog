using System;

namespace code_watchdog
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine ("Hello World!");

            Watchdog wd = new Watchdog();

            wd.Check(args[0]);

            return;
        }
    }
}
