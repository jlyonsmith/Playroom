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
            BuildContentTool tool = new BuildContentTool();

            try
            {
		        ((IProcessCommandLine)tool).ProcessCommandLine(args);

				tool.Execute();

                return tool.HasOutputErrors ? 1 : 0;
            }
            catch (Exception e)
            {
                while (e != null)
                {
                    ConsoleUtility.WriteMessage(MessageType.Error, e.ToString());
                    e = e.InnerException;
                }

                return 1;
            }

        }
    }
}
