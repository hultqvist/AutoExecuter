using System;
using System.Threading;
using System.IO;
using SilentOrbit.AutoExecuter.RuleData;

namespace SilentOrbit.AutoExecuter
{
    class MainClass
    {
        public static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: AutoExecuter.exe configfile.txt");
                return -1;
            }

            Console.WriteLine("AutoExecuter starting...");

            //Rule for rules file
            string path = Path.GetFullPath(args [0]);
            var watch = new Watcher(path);
            watch.Run();

            return 0;
        }
    }
}
