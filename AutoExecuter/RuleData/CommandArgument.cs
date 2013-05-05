using System;

namespace SilentOrbit.AutoExecuter.RuleData
{
    public class CommandArgument
    {
        public string Command { get; set; }
        public string Arguments { get; set; }

        public override string ToString()
        {
            return Command + " " + Arguments;
        }
    }
}

