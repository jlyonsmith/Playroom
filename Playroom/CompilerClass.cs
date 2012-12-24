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
        private string[] inputExtensions;
        private string[] outputExtensions;
        
        public CompilerClass(Assembly assembly, Type type)
        {
            this.Assembly = assembly;
            this.Type = type;
            this.Instance = Activator.CreateInstance(this.Type);
            this.CompileMethod = this.Type.GetMethod(
                "Compile", BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Instance);

			if (this.CompileMethod == null)
				throw new ApplicationException("'Compile' method not found in class {0}".CultureFormat(this.Name));

            this.ContextProperty = this.Type.GetProperty("Context");

			if (this.ContextProperty == null)
				throw new ApplicationException("'Context' property not found in class {0}".CultureFormat(this.Name));
			
			this.TargetProperty = this.Type.GetProperty("Target");
		
			if (this.TargetProperty == null)
				throw new ApplicationException("'Target' property not found in class {0}".CultureFormat(this.Name));
		}

        public Assembly Assembly { get; private set; }
        public Type Type { get; private set; }
        public string Name { get { return this.Type.FullName; } }
        public string[] InputExtensions 
        { 
            get
            {
                if (inputExtensions == null)
                {
                    try
                    {
                        inputExtensions = ((string[])this.Type.GetProperty("InputExtensions").GetValue(this.Instance, null))
                            .OrderBy(s => s, StringComparer.CurrentCultureIgnoreCase).ToArray<string>();
                    }
                    catch (Exception e)
                    {
                        if (e is TargetInvocationException)
                            throw new ApplicationException(String.Format("Invalid result returned by compiler '{0}' InputExtensions property", this.Name));
                        else
                            throw;
                    }
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
                    try
                    {
                        outputExtensions = ((string[])this.Type.GetProperty("OutputExtensions").GetValue(this.Instance, null))
                            .OrderBy(s => s, StringComparer.CurrentCultureIgnoreCase).ToArray<string>();
                    }
                    catch (Exception e)
                    {
                        if (e is TargetInvocationException)
                            throw new ApplicationException(String.Format("Invalid result returned by compiler '{0}' OutputExtensions property", this.Name));
                        else
                            throw;
                    }
                }

                return outputExtensions;
            }
        }
		internal Object Instance { get; private set; }
		internal MethodInfo CompileMethod { get; private set; }
		internal PropertyInfo ContextProperty { get; private set; }
		internal PropertyInfo TargetProperty { get; private set; }
	}
}
