using System;
using System.IO;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;
using ToolBelt;

namespace Playroom
{
    public class ContentFileV4
    {
        public class Target
        {
			public YamlScalarNode Name;
			public List<YamlScalarNode> Inputs;
			public List<YamlScalarNode> Outputs;
			public YamlScalarNode Compiler;
			public List<ContentFileV4.NameValue> Parameters;
        }

		public class CompilerSettings
		{
			public CompilerSettings(YamlScalarNode name, List<ContentFileV4.NameValue> parameters, List<ContentFileV4.CompilerExtensions> extensions)
			{
				Name = name;
				Parameters = parameters;
				Extensions = extensions;
			}
			public YamlScalarNode Name;
			public List<ContentFileV4.CompilerExtensions> Extensions;
			public List<ContentFileV4.NameValue> Parameters;
		}

		public class CompilerExtensions
		{
			public CompilerExtensions(YamlScalarNode inputs, YamlScalarNode outputs)
			{
				Inputs = inputs;
				Outputs = outputs;
			}

			public YamlScalarNode Inputs;
			public YamlScalarNode Outputs;
		}

		public class NameValue
		{
			public NameValue(YamlScalarNode name, YamlScalarNode value)
			{
				Name = name;
				Value = value;
			}
			public YamlScalarNode Name;
			public YamlScalarNode Value;
		}

		public List<YamlScalarNode> Assemblies;
		public List<ContentFileV4.CompilerSettings> Settings;
		public List<NameValue> Properties;
		public List<ContentFileV4.Target> Targets;

		public void Load(ParsedPath contentPath)
		{
			try
			{
				using (StreamReader reader = new StreamReader(contentPath))
				{
					YamlStream yamlStream = new YamlStream();

					yamlStream.Load(reader);
					ReadDocument(yamlStream.Documents[0]);
				}
			}
			catch (Exception e)
			{
				YamlException ye = e as YamlException;
				ContentFileException cfe = e as ContentFileException;

				if (ye != null)
				{
					throw ContentFileException.New(ye);
				}
				else if (cfe != null)
				{
					throw;
				}
				else
				{
					throw new ContentFileException(e);
				}
			}
		}

		private void ReadDocument(YamlDocument document)
		{
			YamlScalarNode scalar1;
			YamlMappingNode mapping1, mapping2, mapping3;
			YamlSequenceNode sequence1, sequence2;

			document.RootNode.CastNode(out mapping1);
			mapping1.GetChildNode("Version", out scalar1);

			if (scalar1.Value != "4")
				throw new ContentFileException(mapping1, "Expected 'version' node with value 4");

			mapping1.GetChildNode("CompilerAssemblies", out sequence1);

			this.Assemblies = new List<YamlScalarNode>();

			foreach (YamlNode node in sequence1)
			{
				node.CastNode(out scalar1);

				this.Assemblies.Add(scalar1);
			}

			mapping1.GetOptionalChildNode("CompilerSettings", out sequence1);

			this.Settings = ReadCompilerSettings(sequence1);

			mapping1.GetOptionalChildNode("Properties", out mapping2);
			this.Properties = ReadOptionalNameValues(mapping2);

			this.Targets = new List<Target>();
			mapping1.GetChildNode("Targets", out sequence1);

			foreach (var node1 in sequence1)
			{
				ContentFileV4.Target target = new ContentFileV4.Target();

				node1.CastNode(out mapping2);
				mapping2.GetChildNode("Name", out scalar1);
				target.Name = scalar1;
				mapping2.GetChildNode("Inputs", out sequence2);
				target.Inputs = new List<YamlScalarNode>();
				sequence2.ForEach(n => { n.CastNode(out scalar1); target.Inputs.Add(scalar1); });
				mapping2.GetChildNode("Outputs", out sequence2);
				target.Outputs = new List<YamlScalarNode>();
				sequence2.ForEach(n => { n.CastNode(out scalar1); target.Outputs.Add(scalar1); });
				mapping2.GetOptionalChildNode("Compiler", out scalar1);
				target.Compiler = scalar1;
				mapping2.GetOptionalChildNode("Parameters", out mapping3);
				target.Parameters = ReadOptionalNameValues(mapping3);

				Targets.Add(target);
			}
		}

		private List<CompilerSettings> ReadCompilerSettings(YamlSequenceNode sequence1)
		{
			List<CompilerSettings> compilerSettings = new List<CompilerSettings>();

			if (sequence1 == null)
				return compilerSettings;

			YamlMappingNode mapping1, mapping2;
			YamlScalarNode scalar1, scalar2, scalar3;
			YamlSequenceNode sequence2;

			foreach (YamlNode node1 in sequence1)
			{
				node1.CastNode(out mapping1);
				mapping1.GetChildNode("Name", out scalar1);

				List<ContentFileV4.CompilerExtensions> extensions = new List<ContentFileV4.CompilerExtensions>();
				mapping1.GetOptionalChildNode("Extensions", out sequence2);

				if (sequence2 != null)
				{
					foreach (var node2 in sequence2)
					{
						node2.CastNode(out mapping2);
						mapping2.GetChildNode("Inputs", out scalar2);
						mapping2.GetChildNode("Outputs", out scalar3);
						
						extensions.Add(new ContentFileV4.CompilerExtensions(scalar2, scalar3));
					}
				}

				mapping1.GetOptionalChildNode("Parameters", out mapping2);

				List<ContentFileV4.NameValue> settings = ReadOptionalNameValues(mapping2);

				compilerSettings.Add(new ContentFileV4.CompilerSettings(scalar1, settings, extensions));
			}

			return compilerSettings;
		}

		private List<ContentFileV4.NameValue> ReadOptionalNameValues(YamlMappingNode mapping1)
		{
			List<NameValue> nameValues = new List<NameValue>();

			if (mapping1 == null)
				return nameValues;

			YamlScalarNode scalar1, scalar2;

			if (mapping1 != null)
			{
				foreach (var pair in mapping1)
				{
					pair.Key.CastNode(out scalar1);
					pair.Value.CastNode(out scalar2);
					
					nameValues.Add(new NameValue(scalar1, scalar2));
				}
			}

			return nameValues;
		}
	}
}
