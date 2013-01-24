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
			public List<Property> Properties;
        }

		public class CompilerSetting
		{
			public CompilerSetting(YamlScalarNode name, YamlMappingNode settings)
			{
				Name = name;
				Settings = settings;
			}
			public YamlScalarNode Name;
			public YamlMappingNode Settings;
		}

		public class Property
		{
			public Property(YamlScalarNode name, YamlScalarNode value)
			{
				Name = name;
				Value = value;
			}
			public YamlScalarNode Name;
			public YamlScalarNode Value;
		}

		public List<YamlScalarNode> CompilerAssemblies;
		public List<ContentFileV4.CompilerSetting> CompilerSettings;
		public List<Property> Properties;
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
					// HACK: Strip off the extra location information in the message
					string message = ye.Message;
					int n = message.IndexOf("): ");
					
					if (n != -1)
						message = message.Substring(n + 3);

					throw new ContentFileException(message, ye);
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
			YamlScalarNode scalar1, scalar2;
			YamlMappingNode mapping1, mapping2, mapping3;
			YamlSequenceNode sequence1, sequence2;

			document.RootNode.CastNode(out mapping1);
			mapping1.GetChildNode("version", out scalar1);

			if (scalar1.Value != "4")
				throw new ContentFileException(mapping1, "Expected 'version' node with value 4");

			mapping1.GetChildNode("compiler-assemblies", out sequence1);

			this.CompilerAssemblies = new List<YamlScalarNode>();

			foreach (YamlNode node in sequence1)
			{
				node.CastNode(out scalar1);

				this.CompilerAssemblies.Add(scalar1);
			}

			this.CompilerSettings = new List<CompilerSetting>();
			mapping1.GetOptionalChildNode("compiler-settings", out sequence1);

			if (sequence1 != null)
			{
				foreach (YamlNode node in sequence1)
				{
					node.CastNode(out mapping2);
					mapping2.GetChildNode("name", out scalar1);
					mapping2.GetChildNode("settings", out mapping3);

					this.CompilerSettings.Add(new ContentFileV4.CompilerSetting(scalar1, mapping3));
				}
			}

			this.Properties = new List<Property>();
			mapping1.GetOptionalChildNode("properties", out mapping2);
			
			if (mapping2 != null)
			{
				foreach (var pair in mapping2)
				{
					pair.Key.CastNode(out scalar1);
					pair.Value.CastNode(out scalar2);

					this.Properties.Add(new Property(scalar1, scalar2));
				}
			}

			this.Targets = new List<Target>();
			mapping1.GetChildNode("targets", out sequence1);

			foreach (var node1 in sequence1)
			{
				ContentFileV4.Target target = new ContentFileV4.Target();

				node1.CastNode(out mapping2);
				mapping2.GetChildNode("name", out scalar1);
				target.Name = scalar1;
				mapping2.GetChildNode("inputs", out sequence2);
				target.Inputs = new List<YamlScalarNode>();
				sequence2.ForEach(n => { n.CastNode(out scalar1); target.Inputs.Add(scalar1); });
				mapping2.GetChildNode("outputs", out sequence2);
				target.Outputs = new List<YamlScalarNode>();
				sequence2.ForEach(n => { n.CastNode(out scalar1); target.Outputs.Add(scalar1); });
				mapping2.GetOptionalChildNode("compiler", out scalar1);
				target.Compiler = scalar1;
				mapping2.GetOptionalChildNode("properties", out mapping3);
				target.Properties = new List<Property>();

				if (mapping3 != null)
				{
					foreach (var pair in mapping3)
					{
						pair.Key.CastNode(out scalar1);
						pair.Value.CastNode(out scalar2);
						
						target.Properties.Add(new Property(scalar1, scalar2));
					}
				}

				Targets.Add(target);
			}
		}
	}
}
