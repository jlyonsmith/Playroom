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
                RealExecute();
            }
            catch (Exception e)
            {
                ContentFileException contentEx = e as ContentFileException;

                if (contentEx != null)
                {
                    Output.Error(contentEx.FileName, contentEx.LineNumber, 0, e.Message);
                }
                else
                {
#if DEBUG
                    Output.Error(e.ToString());
#else
                    Output.Error(e.Message);
#endif
                }
            }
        }

        private void RealExecute ()
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

            PropertyCollection propCollection = new PropertyCollection();

            // Initialize properties from the environment and command line
            propCollection.AddFromEnvironment();
            propCollection.AddWellKnownProperties(
                new ParsedPath(Assembly.GetExecutingAssembly().Location, PathType.File).VolumeAndDirectory,
                ContentFile.VolumeAndDirectory);
            propCollection.AddFromPropertyString(this.Properties);

            BuildContext buildContext = new BuildContext(this.Output, propCollection, this.ContentFile);

            ContentFileV1 contentData = null;

            try
            {
                contentData = ContentFileReaderV1.ReadFile(this.ContentFile);
            }
            catch (Exception e)
            {
                throw new ContentFileException(this.ContentFile, (int)e.Data["LineNumber"], e.Message, e);
            }
            
            Output.Message(MessageImportance.Low, "Read content file '{0}'", this.ContentFile);

            List<CompilerClass> compilerClasses = LoadCompilerClasses(contentData, propCollection);
            List<BuildItem> buildItems = PrepareBuildItems(contentData, propCollection);

            foreach (var buildItem in buildItems)
            {
                foreach (var compilerClass in compilerClasses)
                {
                    if (buildItem.InputExtensions.SequenceEqual(compilerClass.InputExtensions) &&
                        buildItem.OutputExtensions.SequenceEqual(compilerClass.OutputExtensions))
                    {
                        // TODO: More output...
                        Output.Message("Building item with '{0}' compiler", compilerClass.Name);

                        compilerClass.ContextProperty.SetValue(compilerClass.Instance, buildContext, null);
                        compilerClass.ItemProperty.SetValue(compilerClass.Instance, buildItem, null);

                        try
                        {
                            compilerClass.CompileMethod.Invoke(compilerClass.Instance, null);
                        }
                        catch (TargetInvocationException e)
                        {
                            ContentFileException contentEx = e.InnerException as ContentFileException;
                            
                            if (contentEx != null)
                            {
                                contentEx.EnsureFileNameAndLineNumber(buildContext.ContentFile, buildItem.LineNumber);
                                throw contentEx;
                            }
                            else
                            {
                                throw new ContentFileException(this.ContentFile, buildItem.LineNumber, e.InnerException);
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
        private List<BuildItem> PrepareBuildItems(ContentFileV1 contentData, PropertyCollection propCollection)
        {
            List<BuildItem> buildItems = new List<BuildItem>();

            foreach (var item in contentData.Items)
            {
                BuildItem buildItem = new BuildItem();

                buildItem.LineNumber = item.LineNumber;

                buildItem.Properties = new PropertyCollection();

                buildItem.Properties.AddFromTupleList(item.Properties);

                buildItem.InputFiles = new List<ParsedPath>();

                foreach (var rawInputFile in item.InputFiles)
                {
                    ParsedPath inputFile = new ParsedPath(propCollection.ReplaceVariables(rawInputFile), PathType.File);

                    if (inputFile.HasWildcards)
                    {
                        IList<ParsedPath> files = DirectoryUtility.GetFiles(inputFile, SearchScope.RecurseSubDirectoriesBreadthFirst);

                        if (files.Count == 0)
                        {
                            throw new ContentFileException(this.ContentFile, buildItem.LineNumber, "Wildcard input refers to no files after expansion");
                        }

                        buildItem.InputFiles = buildItem.InputFiles.Concat(files).ToList<ParsedPath>();
                    }
                    else
                    {
                        if (!File.Exists(inputFile))
                        {
                            throw new ContentFileException(this.ContentFile, buildItem.LineNumber, String.Format("Input file '{0}' does not exist", inputFile));
                        }

                        buildItem.InputFiles.Add(inputFile);
                    }
                }

                buildItem.OutputFiles = new List<ParsedPath>();

                foreach (var rawOutputFile in item.OutputFiles)
                {
                    ParsedPath outputFile = new ParsedPath(propCollection.ReplaceVariables(rawOutputFile), PathType.File);

                    if (outputFile.HasWildcards)
                    {
                        throw new ContentFileException(this.ContentFile, buildItem.LineNumber, "Output item cannot have wildcards");
                    }

                    buildItem.OutputFiles.Add(outputFile);
                }

                bool needsRebuild = IsCompileRequired(buildItem.InputFiles, buildItem.OutputFiles);

                if (!needsRebuild)
                    continue;

                Func<IList<ParsedPath>, IEnumerable<string>> extractAndSortExtensions = (files) =>
                {
                    return files.Select<ParsedPath, string>(f => f.Extension).Distinct<string>().OrderBy(s => s, StringComparer.CurrentCultureIgnoreCase);
                };

                buildItem.InputExtensions = extractAndSortExtensions(buildItem.InputFiles);
                buildItem.OutputExtensions = extractAndSortExtensions(buildItem.OutputFiles);

                buildItems.Add(buildItem);
            }

            return buildItems;
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

        private List<CompilerClass> LoadCompilerClasses(ContentFileV1 contentData, PropertyCollection propCollection)
        {
            List<CompilerClass> compilerClasses = new List<CompilerClass>();

            foreach (var rawAssemblyFile in contentData.CompilerAssemblyFiles)
            {
                Assembly assembly = null;
                ParsedPath assemblyFile = new ParsedPath(propCollection.ReplaceVariables(rawAssemblyFile), PathType.File);

                try
                {
                    // We use Assembly.Load so that the test assembly and subsequently loaded
                    // assemblies end up in the correct load context.  If the assembly cannot be
                    // found it will raise a AssemblyResolve event where we will search for the 
                    // assembly.
                    assembly = Assembly.LoadFrom(assemblyFile);
                }
                catch (Exception e)
                {
                    throw new ApplicationException(String.Format("Unable to load content compiler assembly file '{0}'. {1}", assemblyFile, e.ToString()), e);
                }

                Type[] types;

                // We won't get dependency errors until we actually try to reflect on all the types in the assembly
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    string message = String.Format("Unable to reflect on assembly '{0}'", assemblyFile);

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
