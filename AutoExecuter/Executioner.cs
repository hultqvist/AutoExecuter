using System;
using SilentOrbit.AutoExecuter.RuleData;
using System.Diagnostics;
using System.IO;

namespace SilentOrbit.AutoExecuter
{
    public class Executioner
    {
        readonly string basePath;

        public Executioner(string basePath)
        {
            this.basePath = basePath;
        }

        public void Run(Rule r)
        {
            if (r.Commands.Count == 0)
            {
                r.ExitCode = -1;
                Console.WriteLine("Warning: no commands for rule");
                return;
            }

            r.ExitCode = 0;

            string first = r.Commands [0].Command;

            if(first.StartsWith("#!"))
                RunWithInterpreter(r);
            else
                RunOneByOne(r);
        }

        void RunWithInterpreter(Rule r)
        {
            var psi = new ProcessStartInfo();
            psi.FileName = r.Commands[0].Command.Substring(2);
            psi.Arguments = r.Commands[0].Arguments;
            psi.WorkingDirectory = basePath;
            psi.RedirectStandardInput = true;
            psi.UseShellExecute = false;

            Console.WriteLine("Executing Interpreter: " + psi.FileName + " " + psi.Arguments);

            Process p = new Process();
            p.StartInfo = psi;

            try{
                p.Start();

                //Pass remaining commands to pipe
                using(TextWriter tw = p.StandardInput)
                {
                    bool first = true;
                    foreach(var c in r.Commands)
                    {
                        if(first)
                        {
                            first = false;
                            continue;
                        }

                        tw.WriteLine(c.Command + " " + c.Arguments);
                    }
                }

                p.WaitForExit();
                if(p.ExitCode != 0)
                {
                    r.ExitCode = p.ExitCode;
                    Console.WriteLine("Failed: " + p.ExitCode);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Failed: " + e.Message);
                r.ExitCode = -1;
            }

        }

        void RunOneByOne(Rule r)
        {
            foreach (CommandArgument args in r.Commands)
            {
                var psi = new ProcessStartInfo();
                psi.FileName = args.Command;
                psi.Arguments = args.Arguments;
                psi.WorkingDirectory = basePath;

                Console.WriteLine("Executing: " + psi.FileName + " " + psi.Arguments);
                Process p = new Process();
                p.StartInfo = psi;

                try{
                    p.Start();
                    p.WaitForExit();
                    if(p.ExitCode != 0)
                        r.ExitCode = p.ExitCode;
                }
                catch(Exception e)
                {
                    Console.Error.WriteLine(e.Message);
                    r.ExitCode = -1;
                }
                if(r.ExitCode != 0)
                    Console.WriteLine("Failed: " + p.ExitCode);
            }
        }
    }
}

