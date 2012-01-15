using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using ToolBelt;
using Playroom;

namespace Prism
{
    class Program
    {
        static int Main(string[] args)
        {
            PrismTool tool = new PrismTool(new ConsoleOutputter());

            if (!((IProcessCommandLine)tool).ProcessCommandLine(args))
                return 1;

            try
            {
                tool.Execute();
            }
            catch (CommandLineArgumentException e)
            {
                tool.Output.Error(e.Message);
            }

            return tool.Output.HasOutputErrors ? 1 : 0;
        }
    }
}
