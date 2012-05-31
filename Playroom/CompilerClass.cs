using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Playroom
{
    class CompilerClass
    {
        private string[] inputExtensions;
        private string[] outputExtensions;
        
        public CompilerClass(Assembly assembly, Type type)
        {
            this.Assembly = assembly;
            this.Type = type;
            this.Instance = Activator.CreateInstance(this.Type);
            this.CompileMethod = this.Type.GetMethod(
                "Compile", BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Instance);
        }

        public Assembly Assembly { get; private set; }
        public Type Type { get; private set; }
        public Object Instance { get; private set; }
        public MethodInfo CompileMethod { get; set; }
        public string Name { get { return this.Type.FullName; } }
        public string[] InputExtensions 
        { 
            get
            {
                if (inputExtensions == null)
                {
                    inputExtensions = ((string[])this.Type.GetProperty("InputExtensions").GetValue(this.Instance, null))
                        .OrderBy(s => s, StringComparer.CurrentCultureIgnoreCase).ToArray<string>();
                }

                return inputExtensions;
            }
        }
        public string[] OutputExtensions
        {
            get
            {
                if (outputExtensions == null)
                {
                    outputExtensions = ((string[])this.Type.GetProperty("OutputExtensions").GetValue(this.Instance, null))
                        .OrderBy(s => s, StringComparer.CurrentCultureIgnoreCase).ToArray<string>();
                }

                return outputExtensions;
            }
        }
    }
}
