using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using ToolBelt;
using System.Reflection;
using System.Security.Cryptography;
using YamlDotNet.RepresentationModel;
using System.Linq;

namespace Playroom
{
    public class BuildContext
    {
		public ParsedPath ContentFilePath { get; private set; }
		public OutputHelper Output { get; private set; }
		internal string GlobalHash { get; private set; }
		internal Dictionary<string, string> TargetHashes { get; private set; }
		internal PropertyCollection Properties { get; set; }
		internal List<CompilerClass> CompilerClasses { get; set; }
		internal ContentFileV4 ContentFile { get; set; }
		internal ParsedPath ContentFileHashesPath { get; set; }
		internal DateTime ContentFileWriteTime { get; set; }
		internal DateTime NewestAssemblyWriteTime { get; set; }

		public BuildContext(OutputHelper output, string properties, ParsedPath contentFilePath)
        {
            Output = output;
            ContentFilePath = contentFilePath;

			Properties = new PropertyCollection();
			Properties.AddFromEnvironment();
			Properties.AddWellKnown(
				new ParsedPath(Assembly.GetExecutingAssembly().Location, PathType.File).VolumeAndDirectory,
				contentFilePath.VolumeAndDirectory);
			Properties.AddFromString(properties);
			ContentFile = new ContentFileV4();

			ContentFile.Load(ContentFilePath);

			Output.Message(MessageImportance.Low, "Read content file '{0}'", ContentFilePath);

			Properties.AddFromList(ContentFile.Properties.Select(p => new KeyValuePair<string, string>(p.Name.Value, p.Value.Value)));

			ContentFileHashesPath = new ParsedPath(
				Properties.GetOptionalValue(
				"TargetHashesFile", Properties.ExpandVariables("$(OutputDir)/" + ContentFilePath.FileAndExtension + ".hashes")), PathType.File).MakeFullPath();

			ContentFileWriteTime = File.GetLastWriteTime(this.ContentFilePath);

			SHA1 sha1 = SHA1.Create();
			StringBuilder sb = new StringBuilder();
			
			foreach (var rawAssembly in ContentFile.CompilerAssemblies)
			{
				sb.Append(rawAssembly);
			}
			foreach (var rawProperty in ContentFile.Properties)
			{
				sb.Append(rawProperty.Name);
				sb.Append(rawProperty.Value);
			}
			foreach (var rawCopyTarget in ContentFile.CompilerSettings)
			{
				sb.Append(rawCopyTarget.Name);
				sb.Append(rawCopyTarget.Settings);
			}
			
			GlobalHash = BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()))).Replace("-", "");

			LoadCompilerClasses();
		}

		private void LoadCompilerClasses()
		{
			CompilerClasses = new List<CompilerClass>();
			NewestAssemblyWriteTime = DateTime.MinValue;

			ParsedPathList assemblyPaths = new ParsedPathList();
			
			foreach (var rawAssembly in ContentFile.CompilerAssemblies)
			{
				ParsedPath pathSpec = null;
				
				try
				{
					pathSpec = new ParsedPath(Properties.ExpandVariables(rawAssembly.Value), PathType.File);
				}
				catch (KeyNotFoundException e)
				{
					throw new ContentFileException(rawAssembly, e);
				}
				
				assemblyPaths.Add(pathSpec);
			}
			
			for (int i = 0; i < assemblyPaths.Count; i++)
			{
				var assemblyPath = assemblyPaths[i];
				Assembly assembly = null;
				
				try
				{
					// We use Assembly.Load so that the test assembly and subsequently loaded
					// assemblies end up in the correct load context.  If the assembly cannot be
					// found it will raise a AssemblyResolve event where we will search for the 
					// assembly.
					assembly = Assembly.LoadFrom(assemblyPath);
				}
				catch (Exception e)
				{
					throw new ContentFileException(this.ContentFile.CompilerAssemblies[i], e);
				}
				
				Type[] types;
				
				// We won't get dependency errors until we actually try to reflect on all the types in the assembly
				try
				{
					types = assembly.GetTypes();
				}
				catch (ReflectionTypeLoadException e)
				{
					string message = "Unable to reflect on assembly '{0}'".CultureFormat(assemblyPath);
					
					// There is one entry in the exceptions array for each null in the types array,
					// and they correspond positionally.
					foreach (Exception ex in e.LoaderExceptions)
						message += Environment.NewLine + "   " + ex.Message;
					
					// Not being able to reflect on classes in the compiler assembly is a critical error
					throw new ContentFileException(this.ContentFile.CompilerAssemblies[i], message, e);
				}
				
				int compilerCount = 0;
				
				// Go through all the types in the test assembly and find all the 
				// compiler classes, those that inherit from IContentCompiler.
				foreach (var type in types)
				{
					Type interfaceType = type.GetInterface(typeof(IContentCompiler).ToString());
					
					if (interfaceType != null)
					{
						CompilerClass compilerClass = new CompilerClass(assembly, type, interfaceType);
						
						CompilerClasses.Add(compilerClass);
						compilerCount++;
						
						// See if any compiler settings apply to this compiler
						YamlMappingNode settingsNode = null;
						
						foreach (var rawSettings in this.ContentFile.CompilerSettings)
						{
							if (compilerClass.Name.EndsWith(rawSettings.Name.Value))
							{
								settingsNode = rawSettings.Settings;
								break;
							}
						}
						
						try 
						{
							compilerClass.SettingsMethod.Invoke(compilerClass.Instance, new object[] { settingsNode });
						}
						catch (Exception e)
						{
							if (settingsNode == null)
								throw new ApplicationException("Failed to setup compiler '{0}'".CultureFormat(compilerClass.Name));
							else
								throw new ContentFileException(settingsNode, "Error setting up compiler with given settings", e);
						}
					}
				}

				DateTime dateTime = File.GetLastWriteTime(assembly.Location);
				
				if (dateTime > NewestAssemblyWriteTime)
					NewestAssemblyWriteTime = dateTime;

				Output.Message(MessageImportance.Normal, "Loaded {0} compilers from assembly '{1}'".CultureFormat(compilerCount, assembly.Location));
			}
		}
	}
}
