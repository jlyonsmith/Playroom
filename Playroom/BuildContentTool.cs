using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;
using System.Xml;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace Playroom
{
	[CommandLineTitle("Playroom Content Builder")]
	[CommandLineDescription("A tool for compiling game and application content from raw resources")]
	public class BuildContentTool : ITool, IProcessCommandLine
	{
        #region Fields
		private bool runningFromCommandLine = false;
		private HashSet<string> targetHashes;
		// TODO: Set this after reading the content file
		private string globalContentHash;
		private ParsedPath targetHashesPath;
		private DateTime contentFileWriteTime;
		private DateTime newestAssemblyWriteTime;

        #endregion

        #region Construction
		public BuildContentTool(IOutputter outputter)
		{
			this.Output = new OutputHelper(outputter);
		}

        #endregion

		[DefaultCommandLineArgument("default", Description = "Input .content data file", ValueHint = "<content-file>", 
            Initializer = typeof(BuildContentTool), MethodName="ParseCommandLineFilePath")]
		public ParsedPath ContentPath { get; set; }

		[CommandLineArgument("properties", ShortName = "p", Description = "Additional properties to set", ValueHint = "<prop1=val1;prop2=val2>")]
		public string Properties { get; set; }

		[CommandLineArgument("help", Description = "Displays this help", ShortName = "?")]
		public bool ShowHelp { get; set; }

		[CommandLineArgument("nologo", Description = "Suppress logo banner")]
		public bool NoLogo { get; set; }

		[CommandLineArgument("rebuild", Description = "Force a rebuild as if all files were out-of-date")]
		public bool RebuildAll { get; set; }
		
		[CommandLineArgument("clean", Description = "Clean all outputs")]
		public bool Clean { get; set; }

		[CommandLineArgument("test", Description = "Test mode. Indicates what would be done but does not actually do it")]
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
				if (e is ContentFileException)
				{
					ContentFileException contentEx = (ContentFileException)e;

					Output.Error(contentEx.FileName, contentEx.LineNumber, 0, e.Message);

					while ((e = e.InnerException) != null)
					{
						string message = e.Message;

						if (e is XmlException)
						{
							int n = message.IndexOf("file://");

							if (n != -1)
								message = message.Substring(0, n);
						}

						Output.Error(contentEx.FileName, contentEx.LineNumber, 0, message);

#if DEBUG
						Console.WriteLine(e.StackTrace);
#endif
					}
				}
				else
				{
					Output.Error(e.Message);
				}
			}
		}

		private void SafeExecute()
		{
			if (!NoLogo)
				Console.WriteLine(Parser.LogoBanner);

			if (!runningFromCommandLine)
			{
				Parser.GetTargetArguments(this);
				Output.Message(MessageImportance.Normal, Parser.CommandName + Parser.Arguments);
			}

			if (ShowHelp)
			{
				Console.WriteLine(Parser.Usage);
				return;
			}

			if (String.IsNullOrEmpty(ContentPath))
			{
				Output.Error("A .content file must be specified");
				return;
			}

			this.ContentPath = this.ContentPath.MakeFullPath();

			if (!File.Exists(this.ContentPath))
			{
				Output.Error("Content file '{0}' does not exist", this.ContentPath);
				return;
			}

			// Initialize properties from the environment and command line
			// TODO: This should be a field, and removed as a parameter from methods
			PropertyGroup globalProps = new PropertyGroup();

			globalProps.AddFromEnvironment();
			globalProps.AddWellKnownProperties(
                new ParsedPath(Assembly.GetExecutingAssembly().Location, PathType.File).VolumeAndDirectory,
                ContentPath.VolumeAndDirectory);
			globalProps.AddFromPropertyString(this.Properties);

			BuildContext buildContext = new BuildContext(this.Output, this.ContentPath);

			ContentFileV3 contentFile = null;

			try
			{
				contentFile = ContentFileReaderV3.ReadFile(this.ContentPath);
			}
			catch (Exception e)
			{
				throw new ContentFileException(this.ContentPath, (int)e.Data["LineNumber"], "Problem reading content file", e);
			}

			Output.Message(MessageImportance.Low, "Read content file '{0}'", this.ContentPath);

			targetHashesPath = new ParsedPath(
				globalProps.GetOptionalValue("TargetHashesFile", globalProps.ReplaceVariables("$(OutputDir)" + ContentPath.FileAndExtension + ".hashes")), 
				PathType.File);

			ReadContentFileHashes();

			FilePathGroup globalFilePaths = new FilePathGroup();

			globalFilePaths.ExpandAndAddFromList(contentFile.FilePaths, globalProps);

			List<CompilerClass> compilerClasses = LoadCompilerClasses(globalFilePaths, globalProps);

			SetNewestAssemblyWriteTime(compilerClasses);
			contentFileWriteTime = File.GetLastWriteTime(this.ContentPath);

			GenerateOutsideTargetsHash(globalProps, globalFilePaths);

			List<BuildTarget> buildTargets = PrepareBuildTargets(
				contentFile.Targets, globalFilePaths, globalProps, compilerClasses);

			HashSet<string> newTargetHashes = new HashSet<string>();
			string newGlobalContentHash;

			// TODO: Set newGlobalContent hash from filepaths and props

			foreach (var buildTarget in buildTargets)
			{
				if (!IsCompileRequired(buildTarget))
					continue;

				CompilerClass compilerClass = buildTarget.CompilerClass;

				string msg = String.Format("Building target '{0}' with '{1}' compiler", buildTarget.Name, compilerClass.Name);

				foreach (var input in buildTarget.InputPaths)
				{
					msg += Environment.NewLine + "\t" + input;
				}

				msg += Environment.NewLine + "\t->";

				foreach (var output in buildTarget.OutputPaths)
				{
					msg += Environment.NewLine + "\t" + output;
				}

				Output.Message(MessageImportance.Normal, msg);

				if (TestMode)
					continue;

				// Set the Context and Target properties on the Compiler class instance
				compilerClass.ContextProperty.SetValue(compilerClass.Instance, buildContext, null);
				compilerClass.TargetProperty.SetValue(compilerClass.Instance, buildTarget, null);

				try
				{
					compilerClass.CompileMethod.Invoke(compilerClass.Instance, null);
				}
				catch (TargetInvocationException e)
				{
					ContentFileException contentEx = e.InnerException as ContentFileException;
                    
					if (contentEx != null)
					{
						contentEx.EnsureFileNameAndLineNumber(buildContext.ContentFilePath, buildTarget.LineNumber);
						throw contentEx;
					}
					else
					{
						throw new ContentFileException(
							this.ContentPath, buildTarget.LineNumber, "Unable to compile target '{0}'".CultureFormat(buildTarget.Name), e.InnerException);
					}
				}

				// Ensure that the output files were generated
				foreach (var outputFile in buildTarget.OutputPaths)
				{
					if (!File.Exists(outputFile))
					{
						throw new ContentFileException(this.ContentPath, buildTarget.LineNumber, 
							"Output file '{0}' was not generated".CultureFormat(outputFile));
                   	}
				}

				newTargetHashes.Add(buildTarget.Hash);
			}

			WriteContentFileHashes(newGlobalContentHash, newTargetHashes);

			Output.Message(MessageImportance.Normal, "Done");
		}

		public static ParsedPath ParseCommandLineFilePath(string value)
		{
			return new ParsedPath(value, PathType.File);
		}

        #region Private Methods
		private List<BuildTarget> PrepareBuildTargets(
			List<ContentFileV3.Target> rawTargets, 
			FilePathGroup globalItems, 
			PropertyGroup globalProps,
			IList<CompilerClass> compilerClasses)
		{
			List<BuildTarget> buildTargets = new List<BuildTarget>();

			foreach (var rawTarget in rawTargets)
			{
				try
				{
					buildTargets.Add(new BuildTarget(rawTarget, globalItems, globalProps, compilerClasses));
				}
				catch (Exception e)
				{
					throw new ContentFileException(this.ContentPath, rawTarget.LineNumber, "Error preparing targets", e);
				}
			}

			return TopologicallySortBuildTargets(buildTargets);
		}

		private List<BuildTarget> TopologicallySortBuildTargets(List<BuildTarget> targets)
		{
			// Create a dictionary of paths -> targets for which they are an input to speed up building the graph
			Dictionary<ParsedPath, List<BuildTarget>> inputPaths = new Dictionary<ParsedPath, List<BuildTarget>>();

			foreach (var buildTarget in targets)
			{
				foreach (var path in buildTarget.InputPaths)
				{
					List<BuildTarget> inputTargets;

					if (!inputPaths.TryGetValue(path, out inputTargets))
					{
						inputTargets = new List<BuildTarget>();
						inputPaths.Add(path, targets);
					}

					targets.Add(buildTarget);
				}
			}

			// Create an adjacency list to represent the graph of from -> to targets
			Dictionary<BuildTarget, HashSet<BuildTarget>> graph = new Dictionary<BuildTarget, HashSet<BuildTarget>>();
			Dictionary<BuildTarget, int> inputEdgeCounts = new Dictionary<BuildTarget, int>();

			targets.ForEach(item => graph.Add(item, new HashSet<BuildTarget>()));
			targets.ForEach(item => inputEdgeCounts[item] = 0);

			foreach (var fromTarget in targets)
			{
				foreach (var outputPath in fromTarget.OutputPaths)
				{
					List<BuildTarget> outputTargets;

					if (inputPaths.TryGetValue(outputPath, out outputTargets))
				    {
						// The from target has an output path which is an input path to other target(s),
						// so add edges from the from target to each of those other targets
						foreach (var outputTarget in outputTargets)
						{
							HashSet<BuildTarget> toTargets = graph[fromTarget];
							toTargets.Add(outputTarget);
							inputEdgeCounts[outputTarget]++;
						}
					}
				}
			}

			Queue<BuildTarget> rootTargets = new Queue<BuildTarget>();
			List<BuildTarget> orderedTargets = new List<BuildTarget>();

			foreach (var buildTarget in targets)
			{
				if (inputEdgeCounts[buildTarget] == 0)
					rootTargets.Enqueue(buildTarget);
			}

			// Do the sort
			while (rootTargets.Count != 0)
			{
				BuildTarget fromTarget = rootTargets.Dequeue();

				orderedTargets.Add(fromTarget);

				HashSet<BuildTarget> toTargets = graph[fromTarget];

				graph.Remove(fromTarget);

				foreach (var toTarget in toTargets)
				{
					inputEdgeCounts[toTarget]--;

					if (inputEdgeCounts[toTarget] == 0)
					{
						rootTargets.Enqueue(toTarget);
					}
				}
			}

			if (graph.Count != 0)
			{
				throw new ArgumentException("A circular target dependency exists starting at target '{0}'", graph.First().Key.Name);
			}

			return orderedTargets;
		}

		private void GenerateOutsideTargetsHash(PropertyGroup globalProps, FilePathGroup globalFilePaths)
		{
			SHA1 sha1 = new SHA1();
			StringBuilder sb = new StringBuilder();

			foreach (var property in globalProps)
			{
				// TODO: Finish this off - see BuildTarget constructor
			}

			return sha1.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
		}

		private void ReadContentFileHashes()
		{
			if (File.Exists(targetHashesPath))
			{
				try
				{
					BinaryFormatter formatter = new BinaryFormatter();
					using (Stream stream = new FileStream(targetHashesPath, FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						// TODO: Read globalContent hash here too
						targetHashes = (HashSet<string>)formatter.Deserialize(stream);
					}
				}
				catch
				{
					// Bad file, don't use it again
					File.Delete(targetHashesPath);
				}
			}

			if (targetHashes == null)
			{
				targetHashes = new HashSet<string>();
			}
		}

		private void WriteContentFileHashes(string globalContentHash, HashSet<string> targetHashes)
		{
			BinaryFormatter formatter = new BinaryFormatter();

			try
			{
				using (Stream stream = new FileStream(targetHashesPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
				{
					// TODO: Write globalContentHash too
					formatter.Serialize(stream, targetHashes);
				}
			}
			catch (Exception ex)
			{
				Output.Warning("Unable to write content hash file '{0}'".CultureFormat(targetHashesPath));
			}
		}

		private void SetNewestAssemblyWriteTime(List<CompilerClass> compilerClasses)
		{
			newestAssemblyWriteTime = DateTime.MinValue;

			foreach (var compilerClass in compilerClasses)
			{
				DateTime dateTime = File.GetLastWriteTime(compilerClass.Assembly.Location);

				if (dateTime > newestAssemblyWriteTime)
					newestAssemblyWriteTime = dateTime;
			}
		}

		private bool IsCompileRequired(BuildTarget buildTarget)
		{
			if (RebuildAll)
				return true;

			DateTime lastWriteTime;
			DateTime newestInputFile = newestAssemblyWriteTime;

			foreach (var inputPath in buildTarget.InputPaths)
			{
				lastWriteTime = File.GetLastWriteTime(inputPath);

				if (lastWriteTime > newestInputFile)
					newestInputFile = lastWriteTime;
			}

			DateTime oldestOutputFile = DateTime.MaxValue;

			foreach (var outputPath in buildTarget.OutputPaths)
			{
				lastWriteTime = File.GetLastWriteTime(outputPath);

				if (lastWriteTime < oldestOutputFile)
					oldestOutputFile = lastWriteTime;
			}

			// If the content file is newer than all inputs but it's hash is not present in the hash file
			// then this targets definition changed or was added so consider the content file write time too.
			if (contentFileWriteTime > newestInputFile && !targetHashes.Contains(buildTarget.Hash))
				newestInputFile = contentFileWriteTime;

			return newestInputFile > oldestOutputFile;
		}

		private List<CompilerClass> LoadCompilerClasses(FilePathGroup pathGroup, PropertyGroup propGroup)
		{
			List<CompilerClass> compilerClasses = new List<CompilerClass>();
			IList<ParsedPath> paths = pathGroup.GetRequiredValue("CompilerAssembly");

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

		public void ProcessCommandLine(string[] args)
		{
			this.runningFromCommandLine = true;

#if MACOS
			Parser.CommandName = "mono BuildContent.exe";
#endif
			Parser.ParseAndSetTarget(args, this);
		}

        #endregion
	}
}
