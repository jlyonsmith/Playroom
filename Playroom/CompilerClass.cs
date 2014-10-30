using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using ToolBelt;
using TsonLibrary;

namespace Playroom
{
	internal class AttributedProperty
	{
		public AttributedProperty(ContentCompilerParameterAttribute attribute, PropertyInfo property)
		{
			this.Attribute = attribute;
			this.Property = property;
		}
		
		public ContentCompilerParameterAttribute Attribute;
		public PropertyInfo Property;
	}

	public class CompilerClass
    {
		private Dictionary<string, AttributedProperty> compilerParameters;
		private Dictionary<string, AttributedProperty> targetParameters;

		internal CompilerClass(TsonStringNode assemblyNode, Assembly assembly, Type type, Type interfaceType)
		{
			this.Assembly = assembly;
			this.Type = type;
			this.Interface = interfaceType;
			this.Instance = Activator.CreateInstance(this.Type);

			compilerParameters = new Dictionary<string, AttributedProperty>();
			targetParameters = new Dictionary<string, AttributedProperty>();

			foreach (PropertyInfo propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
			{
				if (propertyInfo.Name == "Target")
				{
					this.TargetProperty = propertyInfo;
				}
				else if (propertyInfo.Name == "Context")
				{
					this.ContextProperty = propertyInfo;
				}
				else if (propertyInfo.Name == "Extensions")
				{
					this.ExtensionsProperty = propertyInfo;
				}
				else
				{
					object[] attributes = propertyInfo.GetCustomAttributes(typeof(ContentCompilerParameterAttribute), true);
					ContentCompilerParameterAttribute attribute;
				
					if (attributes.Length == 1)
						attribute = (ContentCompilerParameterAttribute)attributes[0];
					else
						continue;

					if (!(propertyInfo.CanRead && propertyInfo.CanWrite))
						throw new ContentFileException(
							assemblyNode, "Settings property '{0}' on '{1}' compiler must be read/write".CultureFormat(propertyInfo.Name, this.Name));

					var property = new AttributedProperty(attribute, propertyInfo);

					if (attribute.ForCompiler)
						compilerParameters.Add(propertyInfo.Name, property);
					else
						targetParameters.Add(propertyInfo.Name, property);
				}
			}
		}

        public Assembly Assembly { get; private set; }
        public Type Type { get; private set; }
        public string Name { get { return this.Type.FullName; } }
		public IList<CompilerExtension> Extensions { get { return (IList<CompilerExtension>)Interface.GetProperty("Extensions").GetValue(this.Instance, null); } }
		internal Type Interface { get; private set; }
		internal Object Instance { get; private set; }
		internal MethodInfo CompileMethod { get { return Interface.GetMethod("Compile"); } }
		internal MethodInfo SettingsMethod { get { return Interface.GetMethod("Setup"); } }
		internal PropertyInfo ContextProperty { get; private set; }
		internal PropertyInfo TargetProperty { get; private set; }
		internal PropertyInfo ExtensionsProperty { get; private set; }
		internal Dictionary<string, AttributedProperty> CompilerParameters { get { return compilerParameters; } }
		internal Dictionary<string, AttributedProperty> TargetParameters { get { return targetParameters; } }
	}
}
