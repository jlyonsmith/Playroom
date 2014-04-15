using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ToolBelt;
using Playroom;
using System.Xml;

namespace BuildContent
{
    class Program
    {
        public static int Main(string[] args)
        {
            BuildContentTool tool = new BuildContentTool(new ConsoleOutputter());

            try
            {
		        ((IProcessCommandLine)tool).ProcessCommandLine(args);

				tool.Execute();
            }
            catch (Exception e)
            {
#if DEBUG
				tool.Output.Error(e.ToString());
#else
				tool.Output.Error(e.Message);
#endif
			}

            return tool.ExitCode;
        }
    }
}
