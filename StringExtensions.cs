using System;
using System.IO;

namespace Fullscreenifier
{
    public static class StringExtensions
    {
        public static void Log(this string log)
        {
            try
            {
                File.AppendAllText("error.log", $"{DateTime.Now}: {log}\n");
            }
            catch
            {
                return;
            }
        }
    }
}
