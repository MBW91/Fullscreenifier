using System;

namespace Fullscreenifier
{
    public class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            if (args.Length != 1 || string.IsNullOrEmpty(args[0]))
            {
#if DEBUG
                args = new string[] { "notepad" };
#else
                "The executable path is missing!".Log();
#endif
            }

            new Fullscreenifier(args[0]).Run();
        }
    }
}
