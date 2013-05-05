using System;
using System.IO;
using System.Collections.Generic;
using SilentOrbit.AutoExecuter.RuleData;
using System.Threading;

namespace SilentOrbit.AutoExecuter
{
    public class Watcher
    {
        readonly Executioner exec;
        readonly FileSystemWatcher rulesWatcher;
        readonly string rulesPath;
        int rulesPathExitCode = -1;
        List<Rule> rules;

        /// <summary>
        /// Signal that a file has changed and a new check should be done
        /// </summary>
        ManualResetEvent check = new ManualResetEvent(false);
        /// <summary>
        /// Last time we run any commands, files modified after this time will trigger a new run
        /// </summary>
        DateTime lastRun = DateTime.MinValue;

        List<FileSystemWatcher> watchers = new List<FileSystemWatcher>();

        public Watcher(string path)
        {
            rulesPath = path;

            string dir = Path.GetDirectoryName(path);

            exec = new Executioner(dir);

            rulesWatcher = new FileSystemWatcher(dir);
            rulesWatcher.Changed += RulesChanged;
            rulesWatcher.Created += RulesChanged;
            rulesWatcher.Deleted += RulesChanged;
            rulesWatcher.Renamed += RulesRenamed;
            rulesWatcher.EnableRaisingEvents = true;
        }

        void RulesRenamed (object sender, RenamedEventArgs e)
        {
            if (e.FullPath != rulesPath && e.OldName != rulesPath)
                return;
            check.Set();
        }

        void RulesChanged (object sender, FileSystemEventArgs e)
        {
            if (e.FullPath != rulesPath)
                return;
            check.Set();
        }

        public void Run()
        {
            //Start watching
            while (true)
            {
                //Console.WriteLine("Checking...");
                DateTime lastCheck = FileSystem.GetUtcNow(Path.GetDirectoryName(rulesPath));
                int executed = 0;

                //Check rules file
                if (FileSystem.Modified(rulesPath, lastRun))
                {
                    LoadRules();
                    executed += 1;
                }

                //Count failures
                int failed = 0;
                if (rulesPathExitCode != 0)
                    failed += 1;

                //Scan all filters
                foreach (Rule r in rules)
                {
                    if (FileSystem.Modified(r, lastRun) == false)
                        continue;

                    exec.Run(r);
                    executed += 1;

                    if (r.ExitCode != 0)
                        failed += 1;
                }

                lastRun = lastCheck;

                //Print result
                if(executed > 0)
                {
                    //Console.WriteLine("Last run " + lastRun.ToString("HH:mm:ss"));
                    if(failed == 0)
                        Console.WriteLine("Successfully executed " + executed + " rules");
                    else
                        Console.WriteLine("Failed " + failed + " out of " + executed + " executed rules");
                }

                //Wait for newt
                check.WaitOne(TimeSpan.FromSeconds(10));
                Thread.Sleep(100); //Allow multiple file saves to complete
                check.Reset();
            }

        }

        void LoadRules()
        {
            Console.WriteLine("Loading rules: " + rulesPath);
            try
            {
                rules = RulesLoader.FromFile(rulesPath);
                rulesPathExitCode = 0;
            }
            catch(Exception e)
            {
                Console.Error.WriteLine(e.Message);
                rulesPathExitCode = -1;
                rules = new List<Rule>();
            }

            //Stop previous watchers
            foreach(var w in watchers)
                w.Dispose();

            //Set new filewatchers
            List<string> watched = new List<string>();
            foreach(var r in rules)
            {
                foreach(var f in r.Files)
                {
                    if(watched.Contains(f.Path))
                        continue;
                    var w = new FileSystemWatcher(f.Path);
                    w.IncludeSubdirectories = f.IncludeSubdirectories;
                    w.Changed += FileSystemEvent;
                    w.Created += FileSystemEvent;
                    w.Deleted += FileSystemEvent;
                    w.Renamed += FileRenamed;
                    w.EnableRaisingEvents = true;
                    watchers.Add(w);
                }
            }
        }

        void FileRenamed (object sender, RenamedEventArgs e)
        {
            if (e.Name == FileSystem.TimeTestFilename)
                return;
            if (e.OldName == FileSystem.TimeTestFilename)
                return;
            //Console.WriteLine(e.FullPath);
            //Console.WriteLine(e.OldFullPath);
            check.Set();
        }

        void FileSystemEvent (object sender, FileSystemEventArgs e)
        {
            if (e.Name == FileSystem.TimeTestFilename)
                return;
            //Console.WriteLine(e.FullPath);
            check.Set();
        }
    }
}

