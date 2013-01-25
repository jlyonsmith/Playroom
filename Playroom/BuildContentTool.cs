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
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;
using YamlDotNet.RepresentationModel.Serialization;

namespace Playroom
{
	[CommandLineTitle("Playroom Content Builder")]
	[CommandLineDescription("A tool for compiling game and application content from raw resources")]
	[CommandLineCopyright("Copyright (c) 2013, Jamoki LLC")]
	public class BuildContentTool : ITool, IProcessCommandLine
	{
        #region Fields
		private bool runningFromCommandLine = false;
		private BuildContext buildContext = null;

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

		[CommandLineArgument("nologo", Description = "Suppress display of logo/banner")]
		public bool NoLogo { get; set; }

		[CommandLineArgument("rebuild", Description = "Rebuild all outputs")]
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
				
				buildContext = new BuildContext(this.Output, this.Properties, this.ContentPath);
				List<BuildTarget> buildTargets;

				PrepareBuildTargets(out buildTargets);
				Build(buildTargets);

				Output.Message(MessageImportance.Normal, "Done");
			}
			catch (Exception e)
			{
				ContentFileException cfe = e as ContentFileException;

				if (cfe != null)
				{
					int line = cfe.Start.Line + 1;
					int column = cfe.Start.Column + 1;

					do
					{
						Output.Error(ContentPath, line, column, e.Message);
#if DEBUG
						Console.WriteLine(e.StackTrace);
#endif
					}
					while ((e = e.InnerException) != null);
				}
				else
				{
					do 
					{
						Output.Error(e.Message);
#if DEBUG
						Console.WriteLine(e.StackTrace);
#endif
					}
					while ((e = e.InnerException) != null);
				}
			}
		}

		private void PrepareBuildTargets(out List<BuildTarget> buildTargets)
		{
			buildTargets = new List<BuildTarget>();
			
			foreach (var rawTarget in buildContext.ContentFile.Targets)
			{
				try
				{
					buildTargets.Add(new BuildTarget(rawTarget, buildContext));
				}
				catch (Exception e)
				{
					throw new ContentFileException(rawTarget.Name, "Error preparing to build targets", e);
				}
			}
			
			buildTargets = TopologicallySortBuildTargets(buildTargets);
		}
		
		private List<BuildTarget> TopologicallySortBuildTargets(List<BuildTarget> targets)
		{
			// Create a dictionary of paths -> targets for which they are an input to speed up building the graph
			Dictionary<ParsedPath, List<BuildTarget>> inputPaths = new Dictionary<ParsedPath, List<BuildTarget>>();
			
			foreach (var target in targets)
			{
				foreach (var path in target.InputPaths)
				{
					List<BuildTarget> inputTargets;
					
					if (!inputPaths.TryGetValue(path, out inputTargets))
					{
						inputTargets = new List<BuildTarget>();
						inputPaths.Add(path, inputTargets);
					}
					
					inputTargets.Add(target);
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
				throw new ArgumentException("A circular target dependency exists starting at target '{0}'".CultureFormat(graph.First().Key.Name));
			}
			
			return orderedTargets;
		}

		public static ParsedPath ParseCommandLineFilePath(string value)
		{
			return new ParsedPath(value, PathType.File);
		}

        #region Private Methods

		private void Build(List<BuildTarget> buildTargets)
		{
			string oldGlobalHash;
			HashSet<string> oldTargetHashes;

			ReadOldContentFileHashes(out oldGlobalHash, out oldTargetHashes);

			foreach (var buildTarget in buildTargets)
			{
				//Output.Message(MessageImportance.Low, "Checking if target '{0}' needs building".CultureFormat(buildTarget.Name));

				foreach (var inputPath in buildTarget.InputPaths)
				{
					if (!File.Exists(inputPath))
					{
						throw new ContentFileException("Required input file '{0}' does not exist".CultureFormat(inputPath));
					}
				}

				if (!IsCompileRequired(buildTarget, oldGlobalHash, oldTargetHashes))
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
					throw new ContentFileException(
						buildTarget.Start, "Unable to compile target '{0}'".CultureFormat(buildTarget.Name), e.InnerException);
				}

				// Ensure that the output files were generated
				foreach (var outputFile in buildTarget.OutputPaths)
				{
					if (!File.Exists(outputFile))
					{
						throw new ContentFileException(
							buildTarget.Start, "Output file '{0}' was not generated".CultureFormat(outputFile));
					}
				}
			}

			WriteNewContentFileHashes(buildTargets);
		}

		private void ReadOldContentFileHashes(out string oldGlobalHash, out HashSet<string> oldTargetHashes)
		{
			oldGlobalHash = String.Empty;
			oldTargetHashes = new HashSet<string>();

			if (File.Exists(buildContext.ContentFileHashesPath))
			{
				try
				{
					var serializer = new YamlSerializer<ContentFileHashesFile>();
					ContentFileHashesFile hashes;
					
					using (StreamReader reader = new StreamReader(buildContext.ContentFileHashesPath))
					{
						hashes = serializer.Deserialize(reader);
					}

					oldGlobalHash = hashes.Global;

					foreach (var hash in hashes.Targets)
					{
						oldTargetHashes.Add(hash);
					}
				}
				catch
				{
					// Bad file, don't use it again
					File.Delete(buildContext.ContentFileHashesPath);
				}
			}
		}

		private void WriteNewContentFileHashes(List<BuildTarget> buildTargets)
		{
			var serializer = new Serializer();

			ContentFileHashesFile hashes = new ContentFileHashesFile()
			{
				Global = buildContext.GlobalHash,
				Targets = buildTargets.Select(t => t.Hash).ToArray()
			};

			try
			{
				using (StreamWriter writer = new StreamWriter(buildContext.ContentFileHashesPath))
				{
					serializer.Serialize(writer, hashes, SerializationOptions.Roundtrip);
				}
			}
			catch
			{
				Output.Warning("Unable to write content hash file '{0}'".CultureFormat(buildContext.ContentFileHashesPath));
			}
		}

		private bool IsCompileRequired(BuildTarget buildTarget, string oldGlobalHash, HashSet<string> oldTargetHashes)
		{
			if (RebuildAll)
				return true;

			DateTime lastWriteTime;
			DateTime newestInputFile = buildContext.NewestAssemblyWriteTime;

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

			// And last but not least, if the content file is newer than all inputs so far and this targets hash 
			// is not present in the hash file then the definition changed or was added,
			// OR if the global hash has changed then consider the content file write time.

			if (buildContext.ContentFileWriteTime > newestInputFile && 
			    (oldGlobalHash != buildContext.GlobalHash || !oldTargetHashes.Contains(buildTarget.Hash)))
			{
				newestInputFile = buildContext.ContentFileWriteTime;
			}

			return newestInputFile > oldestOutputFile;
		}

        #endregion

        #region IProcessCommandLine Members

		public void ProcessCommandLine(string[] args)
		{
			this.runningFromCommandLine = true;

#if OSX
			Parser.CommandName = "mono BuildContent.exe";
#endif
			Parser.ParseAndSetTarget(args, this);
		}

        #endregion
	}
}
