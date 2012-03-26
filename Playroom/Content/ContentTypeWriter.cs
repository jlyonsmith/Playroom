using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Playroom
{
    public abstract class ContentTypeWriter
    {
        private Type targetType;

        protected ContentTypeWriter(Type targetType)
        {
            this.targetType = targetType;
        }

        internal static string GetAssemblyFullName(Assembly assembly, TargetPlatform targetPlatform)
        {
            AssemblyName name = assembly.GetName();
            foreach (NetCFPlatformDescription description in NetCFDescs)
            {
                if (description.TargetPlatform == targetPlatform)
                {
                    name = (AssemblyName)name.Clone();
                    if (KeysAreEqual(name.GetPublicKeyToken(), WindowsPublicKeyToken))
                    {
                        name.SetPublicKeyToken(description.PublicKeyToken);
                        break;
                    }
                    foreach (string str in description.NetCFAssemblies)
                    {
                        if (name.Name.Equals(str, StringComparison.InvariantCulture))
                        {
                            name.Version = description.NetCFAssemblyVersion;
                            name.SetPublicKeyToken(description.NetCFPublicKeyToken);
                            break;
                        }
                    }
                }
            }
            return name.FullName;
        }

        public virtual string GetRuntimeType(TargetPlatform targetPlatform)
        {
            string typeName = GetTypeName(this.targetType);
            
            if (!string.IsNullOrEmpty(this.targetType.Namespace))
            {
                typeName = this.targetType.Namespace + '.' + typeName;
            }

            return (typeName /* this.GetGenericArgumentRuntimeTypes(targetPlatform)) */ + ", " + 
                GetAssemblyFullName(this.targetType.Assembly, targetPlatform));
        }

        private static string GetTypeName(Type type)
        {
            string name = type.Name;
            Type declaringType = type.DeclaringType;
            
            if (declaringType != null)
            {
                name = GetTypeName(declaringType) + '+' + name;
            }

            return name;
        }

        public abstract string GetRuntimeReader(TargetPlatform targetPlatform);
    }

    public class ContentTypeWriter<T> : ContentTypeWriter
    {

    }
}
