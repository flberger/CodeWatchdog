using System;

namespace CodeWatchdog
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Watchdog wd = new Watchdog();

            // TODO: Check if arg is present

            wd.Check(args[0]);

            return;
        }
    }
}
