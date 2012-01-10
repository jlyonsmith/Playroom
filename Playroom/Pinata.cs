using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using ToolBelt;

namespace Playroom
{
    public class Pinata : ITask
    {
        IBuildEngine buildEngine;
        ITaskHost taskHost;
        readonly string taskName = "Pinata";

        [Required]
        public string InputFile { get; set; }

        [Required]
        public string OutputFile { get; set; }

        public bool Force { get; set; }

        #region ITask Members

        public bool Execute()
        {
            PinataTool tool = new PinataTool(new MSBuildOutputter(buildEngine, taskName));

            tool.Parser.CommandName = taskName;
            tool.Force = this.Force;
            tool.PinataFile = new ParsedPath(this.InputFile, PathType.File);
            tool.OutputFile = new ParsedPath(this.OutputFile, PathType.File);
            tool.NoLogo = true;

            try
            {
                tool.Execute();
            }
            catch (Exception e)
            {
                tool.Output.Error(e.Message);
            }

            return !tool.Output.HasOutputErrors;
        }

        public IBuildEngine BuildEngine
        {
            get
            {
                return buildEngine;
            }
            set
            {
                buildEngine = value;
            }
        }

        public ITaskHost HostObject
        {
            get
            {
                return taskHost;
            }
            set
            {
                this.taskHost = value;
            }
        }

        #endregion
    }
}
