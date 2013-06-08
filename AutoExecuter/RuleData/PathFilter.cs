using System;
using System.Collections.Generic;

namespace SilentOrbit.AutoExecuter.RuleData
{
	public class PathFilter
	{
		/// <summary>
		/// Directory to watch
		/// </summary>
		public string Path { get; set; }

		/// <summary>
		/// Filename filter
		/// </summary>
		public string Pattern { get; set; }

		public bool IncludeSubdirectories { get; set; }

		/// <summary>
		/// Files watched in last run, if any of these are missing then it qualifies as a change and the command will execute
		/// </summary>
		/// <value>The watched.</value>
		public List<string> Watched { get; set; }

		public PathFilter()
		{
			Watched = new List<string>();
		}

		public override string ToString()
		{
			return Path + System.IO.Path.DirectorySeparatorChar + Pattern + " (" + Watched.Count + ")";
		}
	}
}

