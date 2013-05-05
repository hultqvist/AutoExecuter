using System;
using System.IO;
using System.Threading;
using SilentOrbit.AutoExecuter.RuleData;
using System.Collections.Generic;

namespace SilentOrbit.AutoExecuter
{
    public static class FileSystem
    {
        /// <summary>
        /// Return the next step of DateTime.UtcNow in filesystem resolution.
        /// This will hold until the filsystem timestamp is later than the time this method was called.
        /// </summary>
        public static DateTime GetUtcNow(string dir)
        {
            string path = Path.Combine(dir, TimeTestFilename);

            File.WriteAllText(path, "");

            DateTime un = DateTime.UtcNow;
            DateTime date = DateTime.UtcNow;
            while (true)
            {
                var info = new FileInfo(path);
                date = info.LastWriteTimeUtc;
                if (date > un)
                    break;

                //Console.WriteLine("Waiting");
                Thread.Sleep(1000);
                File.SetLastWriteTimeUtc(path, DateTime.UtcNow);
            }
            File.Delete(path);
            return date;
        }
        public const string TimeTestFilename = ".AutoExecuterTimeTest";


        
        /// <summary>
        /// Return true if any of the files been modified since last run
        /// </summary>
        /// <param name="filter">Filter.</param>
        public static bool Modified(Rule rule, DateTime last)
        {
            foreach (var filter in rule.Files)
            {
                var watched = filter.Watched;
                filter.Watched = new List<string>();

                string[] files = Directory.GetFiles(filter.Path, filter.Pattern, SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    watched.Remove(file);
                    filter.Watched.Add(file);

                    if(Modified(file, last))
                        return true;
                }
                if (watched.Count > 0)
                    return true;
            }
            return false;
        }

        public static bool Modified(string path, DateTime last)
        {
            FileInfo info = new FileInfo(path);
            return info.LastWriteTimeUtc >= last;
        }
    }
}

