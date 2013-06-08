using System;
using System.Collections.Generic;

namespace SilentOrbit.AutoExecuter.RuleData
{
	public class Rule
	{
		/// <summary>
		/// Status of last run
		/// </summary>
		public int ExitCode { get; set; }

		public List<PathFilter> Files { get; set; }

		public List<CommandArgument> Commands { get; set; }

		public Rule()
		{
			Files = new List<PathFilter>();
			Commands = new List<CommandArgument>();
		}

		public override string ToString()
		{
			return string.Format("[Rule: ExitCode={0}, Files={1}, Commands={2}]", ExitCode, Files.Count, Commands.Count);
		}
	}
}

