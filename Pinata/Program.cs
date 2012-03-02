using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ToolBelt;
using Playroom;
using System.Xml;

namespace Pinata
{
    class Program
    {
        public static int Main(string[] args)
        {
            PinataTool tool = new PinataTool(new ConsoleOutputter());

            if (!((IProcessCommandLine)tool).ProcessCommandLine(args))
                return 1;

            try
            {
                tool.Execute();
            }
            catch (Exception e)
            {
                tool.Output.Error(e.Message);
            }

            return tool.Output.HasOutputErrors ? 1 : 0;
        }
    }
}
