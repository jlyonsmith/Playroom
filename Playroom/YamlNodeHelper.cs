using System;
using YamlDotNet.RepresentationModel;
using ToolBelt;

namespace Playroom
{
	public static class YamlNodeHelper
	{
		public static void CastNode(this YamlNode node, out YamlMappingNode mapping)
		{
			mapping = node as YamlMappingNode;
			
			if (mapping == null)
				throw new ContentFileException(node, "Expected node to be a map");
		}
		
		public static void CastNode(this YamlNode node, out YamlSequenceNode sequence)
		{
			sequence = node as YamlSequenceNode;
			
			if (sequence == null)
				throw new ContentFileException(node, "Expected node to be a sequence");
		}
		
		public static void CastNode(this YamlNode node, out YamlScalarNode scalar)
		{
			scalar = node as YamlScalarNode;
			
			if (scalar == null)
				throw new ContentFileException(node, "Expected node to be a scalar");
		}

		public static void GetChildNode(this YamlMappingNode mapping, string name, out YamlNode childNode)
		{
			if (!mapping.Children.TryGetValue(new YamlScalarNode(name), out childNode))
			{
				throw new ContentFileException(
					mapping, "Required child node '{0}' was not found".CultureFormat(name));
			}
		}
		
		public static void GetChildNode(this YamlMappingNode mapping, string name, out YamlMappingNode childMapping)
		{
			YamlNode node;
			
			if (!mapping.Children.TryGetValue(new YamlScalarNode(name), out node))
			{
				throw new ContentFileException(
					mapping, "Required child mapping node '{0}' was not found".CultureFormat(name));
			}
			
			CastNode(node, out childMapping);
		}

		public static void GetChildNode(this YamlMappingNode mapping, string name, out YamlSequenceNode childSequence)
		{
			YamlNode node;
			
			if (!mapping.Children.TryGetValue(new YamlScalarNode(name), out node))
			{
				throw new ContentFileException(
					mapping, "Required child sequence node '{0}' was not found".CultureFormat(name));
			}
			
			CastNode(node, out childSequence);
		}
		
		public static void GetChildNode(this YamlMappingNode mapping, string name, out YamlScalarNode childScalar)
		{
			YamlNode node;
			
			if (!mapping.Children.TryGetValue(new YamlScalarNode(name), out node))
			{
				throw new ContentFileException(
					mapping, "Required child scalar node '{0}' was not found".CultureFormat(name));
			}
			
			CastNode(node, out childScalar);
		}
		
		public static void GetOptionalChildNode(this YamlMappingNode mapping, string name, out YamlMappingNode childMapping)
		{
			YamlNode node;
			
			if (mapping.Children.TryGetValue(new YamlScalarNode(name), out node))
				CastNode(node, out childMapping);
			else
				childMapping = null;
		}
		
		public static void GetOptionalChildNode(this YamlMappingNode mapping, string name, out YamlSequenceNode childSequence)
		{
			YamlNode node;
			
			if (mapping.Children.TryGetValue(new YamlScalarNode(name), out node))
				CastNode(node, out childSequence);
			else 
				childSequence = null;
		}
		
		public static void GetOptionalChildNode(this YamlMappingNode mapping, string name, out YamlScalarNode childScalar)
		{
			YamlNode node;
			
			if (mapping.Children.TryGetValue(new YamlScalarNode(name), out node))
				CastNode(node, out childScalar);
			else
				childScalar = null;
		}
	}
}

