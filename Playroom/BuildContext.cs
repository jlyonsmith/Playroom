using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using ToolBelt;
using System.Reflection;
using System.Security.Cryptography;
using System.Linq;

namespace Playroom
{
    public class BuildContext
    {
		public ParsedPath ContentFilePath { get; private set; }
		internal string GlobalHash { get; private set; }
		internal Dictionary<string, string> TargetHashes { get; private set; }
		internal PropertyCollection Properties { get; set; }
		internal List<CompilerClass> CompilerClasses { get; set; }
		internal ContentFileV4 ContentFile { get; set; }
		internal ParsedPath ContentFileHashesPath { get; set; }
		internal DateTime ContentFileWriteTime { get; set; }
		internal DateTime NewestAssemblyWriteTime { get; set; }

		public BuildContext(String properties, ParsedPath contentFilePath)
		{
			ContentFilePath = contentFilePath;

			ContentFile = new ContentFileV4();
			ContentFile.Load(ContentFilePath);
			
			WriteMessage("Read content file '{0}'", ContentFilePath);
			
			Properties = new PropertyCollection();

			// Start with the environment
			Properties.AddFromEnvironment();

			// Set defaults for important locations
			Properties.Set("BuildContentDir", new ParsedPath(Assembly.GetExecutingAssembly().Location, PathType.File).VolumeAndDirectory.ToString());
			Properties.Set("ContentFileDir", contentFilePath.VolumeAndDirectory);
			Properties.Set("InputDir", contentFilePath.VolumeAndDirectory);
			Properties.Set("OutputDir", contentFilePath.VolumeAndDirectory);

			// Now override with from content file
            Properties.AddFromList(ContentFile.Properties.Select(nv => new KeyValuePair<string, string>(nv.Name.Value, nv.Value.Value)));

			// Now override with command line
			Properties.AddFromString(properties);

			// Add content file hash location if not set already
			if (!Properties.Contains("ContentFileHashes"))
			{
				ParsedPath path = new ParsedPath(Properties.GetRequiredValue("OutputDir"), PathType.Directory);

				Properties.Set("ContentHashesFile", path.Append(ContentFilePath.FileAndExtension + ".hashes", PathType.File));
			}

			ContentFileHashesPath = new ParsedPath(Properties.GetRequiredValue("ContentHashesFile"), PathType.File).MakeFullPath();
			ContentFileWriteTime = File.GetLastWriteTime(this.ContentFilePath);

            // Create a hash of all the things than can affect the build

			SHA1 sha1 = SHA1.Create();
			StringBuilder sb = new StringBuilder();
			
			foreach (var rawAssembly in ContentFile.CompilerAssemblies)
			{
				sb.Append(rawAssembly.Value);
			}

			foreach (var rawProperty in ContentFile.Properties)
			{
				sb.Append(rawProperty.Name.Value);
				sb.Append(rawProperty.Value.Value);
			}
			foreach (var rawCompilerSettings in this.ContentFile.CompilerSettings)
			{
				sb.Append(rawCompilerSettings.Name);

				foreach (var extension in rawCompilerSettings.CompilerExtensions)
				{
                    foreach (var input in extension.Inputs)
                        sb.Append(input.Value);

                    foreach (var output in extension.Outputs)
                        sb.Append(output.Value);
				}

				foreach (var parameter in rawCompilerSettings.Parameters.KeyValues)
				{
					sb.Append(parameter.Key.Value);
                    sb.Append(parameter.Value.ToString());
				}
			}
			
			GlobalHash = BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()))).Replace("-", "");

			LoadCompilerClasses();
		}

        public void WriteMessage (string format, params object[] args)
        {
            ConsoleUtility.WriteMessage(MessageType.Normal, format, args);
        }

        public void WriteError (string format, params object[] args)
        {
            ConsoleUtility.WriteMessage(MessageType.Error, format, args);
        }

        public void WriteWarning (string format, params object[] args)
        {
            ConsoleUtility.WriteMessage(MessageType.Warning, format, args);
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
					
					if (interfaceType == null)
						continue;

					CompilerClass compilerClass = new CompilerClass(this.ContentFile.CompilerAssemblies[i], assembly, type, interfaceType);
						
					CompilerClasses.Add(compilerClass);
					compilerCount++;
				}

				DateTime dateTime = File.GetLastWriteTime(assembly.Location);
				
				if (dateTime > NewestAssemblyWriteTime)
					NewestAssemblyWriteTime = dateTime;

                WriteMessage("Loaded {0} compilers from assembly '{1}'".CultureFormat(compilerCount, assembly.Location));
			}
		}
	}
}
