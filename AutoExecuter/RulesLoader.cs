using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SilentOrbit.AutoExecuter.RuleData;

namespace SilentOrbit.AutoExecuter
{
    public static class RulesLoader
    {
        public static List<Rule> FromFile(string path)
        {
            string dir = Path.GetDirectoryName(path);
            using(FileStream fs = new FileStream(path, FileMode.Open))
            using(TextReader reader = new StreamReader(fs, Encoding.UTF8))
                return FromTextReader(reader, dir);
        }

        /// <summary>
        /// Froms the text reader.
        /// </summary>
        /// <returns>The text reader.</returns>
        /// <param name="reader"></param>
        /// <param name="path">Base path, location of rules file</param>
        static List<Rule> FromTextReader(TextReader reader, string path)
        {
            var list = new List<Rule>();

            Rule r = null;
            string line;
            while (true)
            {
                line = reader.ReadLine();
                if (line == null) //EOF
                    return list;

                string trimLine = line.Trim(' ', '\t');
                if (trimLine == "") //End of rule
                {
                    r = null;
                    continue;
                }

                if (line.StartsWith("#")) //to ignore first #!/...
                    continue; //Ignore comments
                if (trimLine.StartsWith("//"))
                    continue; //Ignore comments

                if (r == null)
                {
                    r = new Rule();
                    list.Add(r);
                }

                if (line [0] == '\t') //Commands are indented
                    ParseCommand(r, line);
                else {
                    if (r.Commands.Count != 0)
                        throw new FormatException("All paths must be written before the commands, rule sets must be separated with an empty line");
                    ParsePath(r, line, path);
                }

            }
        }

        static void ParsePath(Rule r, string line, string rulesDir)
        {
            var filters = line.Split(' ');

            foreach (string f in filters)
            {
                var pf = new PathFilter();
                int lastsep = f.LastIndexOf(Path.DirectorySeparatorChar);
                if (lastsep < 0)
                {
                    pf.Path = rulesDir;
                    pf.Pattern = f;
                } else
                {
                    string path = f.Substring(0, lastsep);
                    if (Path.IsPathRooted(path) == false)
                        path = Path.Combine(rulesDir, path);
                    pf.Path = Path.GetFullPath(path);
                    pf.Pattern = f.Substring(lastsep + 1);
                }
                r.Files.Add(pf);
            }
        }

        static void ParseCommand(Rule r, string line)
        {
            line = line.Trim('\t', ' ');
            int sep = line.IndexOf(" ");
            var c = new CommandArgument();
            if (sep < 0)
            {
                c.Command = line;
                c.Arguments = "";
            } else
            {
                c.Command = line.Substring(0, sep);
                c.Arguments = line.Substring(sep + 1);
            }
            r.Commands.Add(c);
        }


    }
}

