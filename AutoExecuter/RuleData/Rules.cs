using System;
using System.Collections.Generic;

namespace SilentOrbit.AutoExecuter.RuleData
{
    public class Rules
    {
        public List<Rule> List { get; set; }

        /// <summary>
        /// Run all rules when script is loaded
        /// </summary>
        public bool RunAllAtStart { get; set; }

        public Rules()
        {
            List = new List<Rule>();
        }
    }
}

