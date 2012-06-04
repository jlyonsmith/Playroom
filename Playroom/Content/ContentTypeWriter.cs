using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Playroom
{
    public abstract class ContentTypeWriter
    {
        public Type TargetType { get; private set; }
        public bool IsValueTargetType { get; private set; }
        protected List<ContentTypeWriter> genericArgumentWriters;

        public ContentTypeWriter(Type type)
        {
            if (type.IsByRef || type.IsPointer)
            {
                throw new ArgumentException("Target type cannot be reference or pointer type");
            }
            
            if (type.ContainsGenericParameters)
            {
                throw new ArgumentException("Target type cannot be generic");
            }

            this.TargetType = type;
            this.IsValueTargetType = type.IsValueType;
        }

        public virtual void Initialize(XnbFileWriterV5 xnbWriter)
        {
            Type writerType = this.GetType();

            if (genericArgumentWriters == null && writerType.IsGenericType)
            {
                genericArgumentWriters = new List<ContentTypeWriter>();

                foreach (Type type in writerType.GetGenericArguments())
                {
                    genericArgumentWriters.Add(xnbWriter.GetTypeWriter(type));
                }
            }
        }

        public abstract void Write(ContentWriter writer, object value);

        public virtual string GetWriterTypeName()
        {
            return GetStrongTypeName(this.GetType());
        }

        public virtual string GetReaderTypeName()
        {
            return GetWriterTypeName().Replace("Writer", "Reader");
        }

        public virtual string GetTargetTypeName()
        {
            return GetStrongTypeName(TargetType);
        }
 
        protected static string GetShortTypeName(Type type)
        {
            string name = type.Name;
            Type declaringType = type.DeclaringType;
            
            if (declaringType != null)
            {
                name = GetShortTypeName(declaringType) + '+' + name;
            }
            
            return name;
        }

        protected static string GetTypeName(Type type)
        {
            string name = GetShortTypeName(type);

            if (!String.IsNullOrEmpty(type.Namespace))
            {
                name = type.Namespace + '.' + name;
            }

            return name;
        }

        protected static string GetAssemblyFullName(Assembly assembly)
        {
            return assembly.GetName().FullName;
        }

        protected string GetGenericArgumentRuntimeTypes()
        {
            if (this.genericArgumentWriters == null)
            {
                return string.Empty;
            }

            string str = string.Empty;
            
            for (int i = 0; i < this.genericArgumentWriters.Count; i++)
            {
                if (i > 0)
                {
                    str = str + ',';
                }
                object obj2 = str;
                
                str = string.Concat(new object[] { obj2, '[', this.genericArgumentWriters[i].GetTargetTypeName(), ']' });
            }
            return ('[' + str + ']');
        }

        protected string GetStrongTypeName(Type type)
        {
            return GetTypeName(type) + GetGenericArgumentRuntimeTypes() + ", " + GetAssemblyFullName(type.Assembly);
        }
    }

    public abstract class ContentTypeWriter<T> : ContentTypeWriter
    {
        public ContentTypeWriter() : base(typeof(T))
        {
        }

        public override void Write(ContentWriter writer, object value)
        {
            this.Write(writer, (T)value);
        }

        public abstract void Write(ContentWriter writer, T value);
    }
}
