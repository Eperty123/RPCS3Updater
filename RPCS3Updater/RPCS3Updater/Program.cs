using System;
using static RPCS3Updater.Libs.Logger;
using static RPCS3Updater.Libs.Globals;
using static RPCS3Updater.Libs.UpdateFetcher;
using System.Threading;

namespace RPCS3Updater
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Start();
            if (UpdateAvailable())
            {
                Write("New version is up: {0}! Current is: {1}. Type 'U' to update or press any other key to skip.",
                    Build.Version, CurrentVersion);
                Update(ConsoleKey.U);
            }
            else
            {
                Write("No updates available. Starting {0}...", Executeable);
                Thread.Sleep(1600);
                Launch();
            }
            //Console.ReadKey();
        }
    }
}
