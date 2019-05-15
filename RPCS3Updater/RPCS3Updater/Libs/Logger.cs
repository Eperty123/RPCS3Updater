using System;
using System.Windows.Forms;

namespace RPCS3Updater.Libs
{
    public static class Logger
    {
        public static void Write(string format, params object[] args)
        {
            Console.WriteLine("[{0}]: {1}", Application.ProductName, string.Format(format, args));
        }
    }
}
