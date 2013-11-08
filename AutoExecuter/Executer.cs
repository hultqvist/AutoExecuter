using System;
using SilentOrbit.AutoExecuter.RuleData;
using System.Diagnostics;
using System.IO;

namespace SilentOrbit.AutoExecuter
{
	public class Executer
	{
		readonly string basePath;

		public Executer(string basePath)
		{
			this.basePath = basePath;
		}

		public void Run(Rule r)
		{
			if (r.Commands.Count == 0)
			{
				r.ExitCode = -1;
				ColorConsole.WriteLine("Warning: no commands for rule", ConsoleColor.Red);
				return;
			}

			r.ExitCode = 0;

			string first = r.Commands[0].Command;

			if (first.StartsWith("#!"))
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
			psi.RedirectStandardOutput = true;
			psi.RedirectStandardError = true;
			psi.UseShellExecute = false;

			ColorConsole.WriteLine("Executing Interpreter: " + psi.FileName + " " + psi.Arguments, ConsoleColor.DarkGray);

			Process p = new Process();
			p.StartInfo = psi;
			p.ErrorDataReceived += HandleErrorDataReceived;
			p.OutputDataReceived += HandleOutputDataReceived;

			try
			{
				p.Start();
				p.BeginErrorReadLine();
				p.BeginOutputReadLine();

				//Pass remaining commands to pipe
				using (TextWriter tw = p.StandardInput)
				{
					bool first = true;
					foreach (var c in r.Commands)
					{
						if (first)
						{
							first = false;
							continue;
						}
						ColorConsole.WriteLine(c.Command + " " + c.Arguments, ConsoleColor.DarkGray);
						tw.WriteLine(c.Command + " " + c.Arguments);
					}
				}

				p.WaitForExit();
				if (p.ExitCode == 0)
					ColorConsole.WriteLine("Last succeeded", ConsoleColor.Green);
				else
				{
					r.ExitCode = p.ExitCode;
					ColorConsole.WriteLine("Failed: " + p.ExitCode, ConsoleColor.Red);
				}
			}
			catch (Exception e)
			{
				ColorConsole.WriteLine("Failed: " + e.Message, ConsoleColor.Red);
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
				psi.RedirectStandardInput = true;
				psi.RedirectStandardOutput = true;
				psi.RedirectStandardError = true;
				psi.UseShellExecute = false;

				ColorConsole.WriteLine("Executing: " + psi.FileName + " " + psi.Arguments, ConsoleColor.DarkGray);
				Process p = new Process();
				p.StartInfo = psi;
				p.ErrorDataReceived += HandleErrorDataReceived;
				p.OutputDataReceived += HandleOutputDataReceived;

				try
				{
					p.Start();
					p.BeginErrorReadLine();
					p.BeginOutputReadLine();
					p.WaitForExit();
					if (p.ExitCode != 0)
						r.ExitCode = p.ExitCode;
				}
				catch (Exception e)
				{
					Console.Error.WriteLine(e.Message);
					r.ExitCode = -1;
				}
				if (p.ExitCode == 0)
					ColorConsole.WriteLine("Last succeeded", ConsoleColor.Green);
				else
					ColorConsole.WriteLine("Failed: " + p.ExitCode, ConsoleColor.Red);
			}
		}

		void HandleErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (e.Data == null)
				return;
			ColorConsole.WriteLine("\t" + e.Data, ConsoleColor.Red);
		}

		void HandleOutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (e.Data == null)
				return;
			ColorConsole.WriteLine("\t" + e.Data, ConsoleColor.Blue);
		}
	}
}

