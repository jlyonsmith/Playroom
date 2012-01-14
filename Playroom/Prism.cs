using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using ToolBelt;

namespace Playroom
{
    public class Prism : ITask
    {
        #region Fields
        private IBuildEngine buildEngine;
        private ITaskHost taskHost;
        private readonly string taskName = "Prism";
        
        #endregion

        #region Public Properties
        [Required]
        public string InputFile { get; set; }
        [Required]
        public string PngDirectory { get; set; }
        [Required]
        public string SvgDirectory { get; set; }
        [Required]
        public string RsvgConvertExe { get; set; }
        public string InkscapeExe { get; set; }
        public bool Force { get; set; }
        public bool Extend { get; set; }

        #endregion

        #region ITask Members

        public bool Execute()
        {
            PrismTool tool = new PrismTool(new MSBuildOutputter(buildEngine, taskName));

            tool.Parser.CommandName = taskName;
            tool.Force = this.Force;
            tool.PrismFile = new ParsedPath(this.InputFile, PathType.File);
            tool.PngDir = new ParsedPath(this.PngDirectory, PathType.Directory);
            tool.SvgDir = new ParsedPath(this.SvgDirectory, PathType.Directory);
            tool.RsvgConvertExe = new ParsedPath(this.RsvgConvertExe, PathType.File);
            tool.Extend = this.Extend;
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
