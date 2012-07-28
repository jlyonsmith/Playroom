using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;
using System.Xml;
using System.Reflection;
using System.IO;

namespace Playroom
{
    [CommandLineDescription("XNA Content Builder")]
    [CommandLineTitle("Build Content Tool")]
    public class BuildContentTool : ITool, IProcessCommandLine
    {
        #region Fields
        private bool runningFromCommandLine = false;
        
        #endregion

        #region Construction
        public BuildContentTool(IOutputter outputter)
        {
            this.Output = new OutputHelper(outputter);
        }

        #endregion

        [DefaultCommandLineArgument("default", Description = "Input .content data file", ValueHint = "<content-file>", 
            Initializer = typeof(BuildContentTool), MethodName="ParseCommandLineFilePath")]
        public ParsedPath ContentFile { get; set; }

        [CommandLineArgument("properties", ShortName = "p", Description = "Additional properties to set", ValueHint = "<prop1=val1;prop2=val2>")]
        public string Properties { get; set; }

        [CommandLineArgument("help", Description = "Displays this help", ShortName = "?")]
        public bool ShowHelp { get; set; }

        [CommandLineArgument("nologo", Description = "Suppress logo banner")]
        public bool NoLogo { get; set; }

        [CommandLineArgument("rebuild", Description = "Force a rebuild even if all files are up-to-date")]
        public bool Rebuild { get; set; }

		[CommandLineArgument("test", Description = "Test mode.  Indicates what would content will be compiled, but does not actually compile it")]
		public bool TestMode { get; set; }

        public OutputHelper Output { get; set; }

        private CommandLineParser parser;

        public CommandLineParser Parser
        {
            get
            {
                if (parser == null)
                    parser = new CommandLineParser(this.GetType());

                return parser;
            }
        }

        public int ExitCode
        {
            get
            {
                return Output.HasOutputErrors ? 1 : 0;
            }
        }

        public void Execute()
        {
            try
            {
                SafeExecute();
            }
            catch (Exception e)
            {
				do 
				{
	                if (e is ContentFileException)
	                {
		                ContentFileException contentEx = (ContentFileException)e;

	                    Output.Error(contentEx.FileName, contentEx.LineNumber, 0, e.Message);
	                }
	                else
	                {
#if DEBUG
						// When debugging, output the stack trace too
	                    Output.Error(e.ToString());
#else
	                    Output.Error(e.Message);
#endif
	                }

					e = e.InnerException;
				}
				while (e != null);
            }
        }

        private void SafeExecute ()
		{
			if (!NoLogo)
				Console.WriteLine (Parser.LogoBanner);

			if (!runningFromCommandLine) {
				Parser.GetTargetArguments (this);
				Output.Message (MessageImportance.Normal, Parser.CommandName + Parser.Arguments);
			}

			if (ShowHelp) {
				Console.WriteLine (Parser.Usage);
				return;
			}

			if (String.IsNullOrEmpty (ContentFile)) {
				Output.Error ("A .content file must be specified");
				return;
			}

			this.ContentFile = this.ContentFile.MakeFullPath ();

			if (!File.Exists (this.ContentFile)) 
			{
				Output.Error("Content file '{0}' does not exist", this.ContentFile);
				return;
			}

            // Initialize properties from the environment and command line
            PropertyGroup globalProps = new PropertyGroup();

            globalProps.AddFromEnvironment();
            globalProps.AddWellKnownProperties(
                new ParsedPath(Assembly.GetExecutingAssembly().Location, PathType.File).VolumeAndDirectory,
                ContentFile.VolumeAndDirectory);
            globalProps.AddFromPropertyString(this.Properties);

            BuildContext buildContext = new BuildContext(this.Output, globalProps, this.ContentFile);

            ContentFileV2 contentFile = null;

            try
            {
                contentFile = ContentFileReaderV2.ReadFile(this.ContentFile);
            }
            catch (Exception e)
            {
                throw new ContentFileException(this.ContentFile, (int)e.Data["LineNumber"], e.Message, e);
            }
            
            Output.Message(MessageImportance.Low, "Read content file '{0}'", this.ContentFile);

            ItemGroup globalItems = new ItemGroup();

			globalItems.ExpandAndAdd(contentFile.Items, globalProps);

			List<CompilerClass> compilerClasses = LoadCompilerClasses(globalItems, globalProps);
            List<BuildTarget> BuildTargets = PrepareBuildTargets(contentFile.Targets, globalItems, globalProps);

            foreach (var buildTarget in BuildTargets)
            {
                foreach (var compilerClass in compilerClasses)
                {
                    if (buildTarget.InputExtensions.SequenceEqual(compilerClass.InputExtensions) &&
                        buildTarget.OutputExtensions.SequenceEqual(compilerClass.OutputExtensions))
                    {
						string msg = String.Format("Building target '{0}' with '{1}' compiler", buildTarget.Name, compilerClass.Name);

						foreach (var input in buildTarget.InputFiles)
						{
							msg += Environment.NewLine + "\t" + input;
						}

						msg += Environment.NewLine + "\t->";

						foreach (var output in buildTarget.OutputFiles)
						{
							msg += Environment.NewLine + "\t" + output;
						}

						Output.Message(msg);

						if (TestMode)
							continue;

                        compilerClass.ContextProperty.SetValue(compilerClass.Instance, buildContext, null);
                        compilerClass.ItemProperty.SetValue(compilerClass.Instance, buildTarget, null);

                        try
                        {
                            compilerClass.CompileMethod.Invoke(compilerClass.Instance, null);
                        }
                        catch (TargetInvocationException e)
                        {
                            ContentFileException contentEx = e.InnerException as ContentFileException;
                            
                            if (contentEx != null)
                            {
                                contentEx.EnsureFileNameAndLineNumber(buildContext.ContentFile, buildTarget.LineNumber);
                                throw contentEx;
                            }
                            else
                            {
                                throw new ContentFileException(this.ContentFile, buildTarget.LineNumber, e.InnerException);
                            }
                        }
                    }
                }
            }
        }

        public static ParsedPath ParseCommandLineFilePath(string value)
        {
            return new ParsedPath(value, PathType.File).MakeFullPath();
        }

        #region Private Methods
        private List<BuildTarget> PrepareBuildTargets(List<ContentFileV2.Target> rawTargets, ItemGroup globalItems, PropertyGroup globalProps)
        {
            List<BuildTarget> buildTargets = new List<BuildTarget>();

            foreach (var rawTarget in rawTargets)
            {
				PropertyGroup targetProps = new PropertyGroup(globalProps);

				if (rawTarget.Properties != null)
                	targetProps.ExpandAndAdd(rawTarget.Properties, globalProps);

				targetProps.Add("TargetName", rawTarget.Name);

				ItemGroup targetItems = new ItemGroup(globalItems);

				List<ParsedPath> inputFiles = new List<ParsedPath>();
				string[] list = rawTarget.Inputs.Split(';');

                foreach (var rawInputFile in list)
                {
                    ParsedPath pathSpec = new ParsedPath(targetProps.ReplaceVariables(rawInputFile), PathType.File);

                    if (pathSpec.HasWildcards && Directory.Exists(pathSpec.VolumeAndDirectory))
                    {
                        IList<ParsedPath> files = DirectoryUtility.GetFiles(pathSpec, SearchScope.DirectoryOnly);

                        if (files.Count == 0)
                        {
                            throw new ContentFileException(this.ContentFile, rawTarget.LineNumber, "Wildcard input refers to no files after expansion");
                        }

                        inputFiles = inputFiles.Concat(files).ToList<ParsedPath>();
                    }
                    else
                    {
                        if (!File.Exists(pathSpec))
                        {
                            throw new ContentFileException(this.ContentFile, rawTarget.LineNumber, String.Format("Input file '{0}' does not exist", pathSpec));
                        }

                        inputFiles.Add(pathSpec);
                    }
                }

                List<ParsedPath> outputFiles = new List<ParsedPath>();

				list = rawTarget.Outputs.Split(';');

                foreach (var rawOutputFile in list)
                {
                    ParsedPath outputFile = new ParsedPath(targetProps.ReplaceVariables(rawOutputFile), PathType.File);

                    outputFiles.Add(outputFile);
                }

				targetItems["TargetInputs"] = inputFiles;
				targetItems["TargetOutputs"] = outputFiles;

                bool needsRebuild = IsCompileRequired(inputFiles, outputFiles);

                if (!needsRebuild)
                    continue;

                buildTargets.Add(new BuildTarget(rawTarget.LineNumber, targetProps, targetItems));
            }

            return buildTargets;
        }

        private bool IsCompileRequired(IList<ParsedPath> inputFiles, IList<ParsedPath> outputFiles)
        {
            if (Rebuild)
                return true;

            DateTime newestInputFile = DateTime.MinValue;

            foreach (var inputFile in inputFiles)
            {
                DateTime lastWriteTime = File.GetLastWriteTime(inputFile);

                if (lastWriteTime > newestInputFile)
                    newestInputFile = lastWriteTime;
            }

            DateTime oldestOutputFile = DateTime.MaxValue;

            foreach (var outputFile in outputFiles)
            {
                DateTime lastWriteTime = File.GetLastWriteTime(outputFile);

                if (lastWriteTime < oldestOutputFile)
                    oldestOutputFile = lastWriteTime;
            }

            return newestInputFile > oldestOutputFile;
        }

        private List<CompilerClass> LoadCompilerClasses(ItemGroup itemGroup, PropertyGroup propGroup)
        {
            List<CompilerClass> compilerClasses = new List<CompilerClass>();
			IList<ParsedPath> paths = itemGroup["CompilerAssembly"];

            foreach (var path in paths)
            {
                Assembly assembly = null;

                try
                {
                    // We use Assembly.Load so that the test assembly and subsequently loaded
                    // assemblies end up in the correct load context.  If the assembly cannot be
                    // found it will raise a AssemblyResolve event where we will search for the 
                    // assembly.
                    assembly = Assembly.LoadFrom(path);
                }
                catch (Exception e)
                {
                    throw new ApplicationException(String.Format("Unable to load content compiler assembly file '{0}'. {1}", path, e.ToString()), e);
                }

                Type[] types;

                // We won't get dependency errors until we actually try to reflect on all the types in the assembly
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    string message = String.Format("Unable to reflect on assembly '{0}'", path);

                    // There is one entry in the exceptions array for each null in the types array,
                    // and they correspond positionally.
                    foreach (Exception ex in e.LoaderExceptions)
                        message += Environment.NewLine + "   " + ex.Message;

                    // Not being able to reflect on classes in the test assembly is a critical error
                    throw new ApplicationException(message, e);
                }

                // Go through all the types in the test assembly and find all the 
                // compiler classes, those that inherit from IContentCompiler.
                foreach (var type in types)
                {
                    Type interfaceType = type.GetInterface(typeof(IContentCompiler).ToString());

                    if (interfaceType != null)
                    {
                        CompilerClass compilerClass = new CompilerClass(assembly, type);

                        compilerClasses.Add(compilerClass);
                    }
                }
            }

            return compilerClasses;
        }

        #endregion

        #region IProcessCommandLine Members

        public bool ProcessCommandLine(string[] args)
        {
            this.runningFromCommandLine = true;

            try
            {
                Parser.ParseAndSetTarget(args, this);
            }
            catch (CommandLineArgumentException e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        #endregion
    }
}
