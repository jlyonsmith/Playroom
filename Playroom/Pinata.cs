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
        public string PinataFile { get; set; }

        [Required]
        public string CsFile { get; set; }

        public bool Force { get; set; }

        #region ITask Members

        public bool Execute()
        {
            PinataTool tool = new PinataTool(new MSBuildOutputter(buildEngine, taskName));

            tool.Parser.CommandName = taskName;
            tool.Rebuild = this.Force;
            tool.PinataFile = new ParsedPath(this.PinataFile, PathType.File);
            tool.CsFile = new ParsedPath(this.CsFile, PathType.File);
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
