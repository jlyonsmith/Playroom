using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using ToolBelt;

namespace Playroom
{
    public class CompilerClass
    {
        public CompilerClass(Assembly assembly, Type type, Type interfaceType)
        {
            this.Assembly = assembly;
            this.Type = type;
			this.Interface = interfaceType;
            this.Instance = Activator.CreateInstance(this.Type);
		}

        public Assembly Assembly { get; private set; }
        public Type Type { get; private set; }
        public string Name { get { return this.Type.FullName; } }
		public CompilerExtension[] Extensions { get { return (CompilerExtension[])Interface.GetProperty("Extensions").GetValue(this.Instance, null); } }
		internal Type Interface { get; private set; }
		internal Object Instance { get; private set; }
		internal MethodInfo CompileMethod { get { return Interface.GetMethod("Compile"); } }
		internal MethodInfo SettingsMethod { get { return Interface.GetMethod("Setup"); } }
		internal PropertyInfo ContextProperty { get { return Interface.GetProperty("Context"); } }
		internal PropertyInfo TargetProperty { get { return Interface.GetProperty("Target"); } }
	}
}
