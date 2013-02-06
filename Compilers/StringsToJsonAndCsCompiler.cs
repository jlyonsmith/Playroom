using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;
using System.IO;
using YamlDotNet.RepresentationModel.Serialization;

namespace Playroom
{
    public class StringsToJsonAndCsCompiler : IContentCompiler
    {
        #region Classes
        class StringsContent
        {
            public class String
            {
                public string Name { get; set; }
                public string Value { get; set; }
                public int ArgCount { get; set; }
            }

            public string Namespace { get; set; }
            public string ClassName { get; set; }
            public List<StringsContent.String> Strings { get; set; }
        }

        #endregion

		#region Fields
		private CompilerExtension[] extensions = new CompilerExtension[]
		{
			new CompilerExtension(".strings", ".json:.cs")
		};
		#endregion 

		#region Properties
		[ContentCompilerParameterAttribute("Class name for the generated C# file.  Xxx will be replaced with the base file name of the input strings file.", Optional = true)]
		public string ClassName { get; set; }

		[ContentCompilerParameterAttribute("Namespace for the generated C# file.", Optional = false)]
		public string Namespace { get; set; }
		#endregion

		#region Construction
		public StringsToJsonAndCsCompiler()
		{
			ClassName = "XxxStrings";
		}

		#endregion
		
		#region IContentCompiler
		public IList<CompilerExtension> Extensions { get { return extensions; } }
		public BuildContext Context { get; set; }
        public BuildTarget Target { get; set; }

		public void Compile()
		{
			if (Target.InputPaths.Count != 1)
				throw new ContentFileException("Only one input file expected");
			
			if (Target.OutputPaths.Count != 2)
				throw new ContentFileException("Only two output files expected");
			
			ParsedPath stringsFilePath = Target.InputPaths[0];
			ParsedPath csFilePath = Target.OutputPaths[0];
			ParsedPath jsonFilePath = Target.OutputPaths[1];

			if (ClassName.StartsWith("Xxx"))
				this.ClassName = this.ClassName.Replace("Xxx", stringsFilePath.File + "Strings");

			StringsContent stringsData = CreateStringsData(this.ClassName, ReadStringsFile(stringsFilePath));

            string[] strings = stringsData.Strings.Select(s => s.Value).ToArray();

			if (!Directory.Exists(jsonFilePath.VolumeAndDirectory))
				Directory.CreateDirectory(jsonFilePath.VolumeAndDirectory);

			WriteJsonFile(jsonFilePath, strings);

			if (!Directory.Exists(csFilePath.VolumeAndDirectory))
				Directory.CreateDirectory(csFilePath.VolumeAndDirectory);

            using (TextWriter writer = new StreamWriter(csFilePath))
            {
                WriteCsOutput(writer, stringsData);
            }
        }

        #endregion

		private void WriteJsonFile(ParsedPath jsonFilePath, object data)
		{
			var serializer = new Serializer();

			using (StreamWriter writer = new StreamWriter(jsonFilePath))
			{
				serializer.Serialize(writer, data, SerializationOptions.JsonCompatible | SerializationOptions.Roundtrip);
			}
		}

		private Dictionary<string, string> ReadStringsFile(ParsedPath stringsFilePath)
		{
			var serializer = new YamlSerializer<Dictionary<string, string>>();
			Dictionary<string, string> stringDict;
			
			using (StreamReader reader = new StreamReader(stringsFilePath))
			{
				stringDict = serializer.Deserialize(reader);
			}

			return stringDict;
		}

        private StringsContent CreateStringsData(string className, Dictionary<string, string> stringDict)
        {
            StringsContent stringsData = new StringsContent();

            stringsData.ClassName = className;
            stringsData.Strings = new List<StringsContent.String>();
            stringsData.Namespace = this.Namespace;

            foreach (var pair in stringDict)
            {
                StringsContent.String d = new StringsContent.String();

                d.Name = pair.Key;
                d.Value = pair.Value;

                // Count the args in the string
                int n = 0;

                for (int i = 0; i < d.Value.Length - 1; i++)
                {
                    if (d.Value[i] == '{' && d.Value[i + 1] != '{')
                    {
                        n++;
                    }
                }

                d.ArgCount = n;

                stringsData.Strings.Add(d);
            }

            return stringsData;
        }

        private void WriteCsOutput(TextWriter writer, StringsContent stringsData)
        {
            writer.WriteLine("//");
            writer.WriteLine("// This file was generated on {0}.", DateTime.Now);
            writer.WriteLine("//");
            writer.WriteLine();
            writer.WriteLine("using System;");
            writer.WriteLine("");
            writer.WriteLine("namespace {0}", stringsData.Namespace);
            writer.WriteLine("{");
			writer.WriteLine("\tpublic class {0}", stringsData.ClassName);
			writer.WriteLine("\t{");
			writer.WriteLine("\t\tprivate string[] strings;");
			writer.WriteLine("");
			writer.WriteLine("\t\tpublic {0}(string[] strings)", stringsData.ClassName);
			writer.WriteLine("\t\t{");
			writer.WriteLine("\t\t\tthis.strings = strings;");
			writer.WriteLine("\t\t}");
			writer.WriteLine();

            for (int i = 0; i < stringsData.Strings.Count; i++)
            {
                StringsContent.String s = stringsData.Strings[i];

                if (s.ArgCount == 0)
                {
                    writer.WriteLine("\t\tpublic string {0} {{ get {{ return strings[{1}]; }} }}",
                        s.Name, i);
                }
                else
                {
                    StringBuilder sb1 = new StringBuilder();
                    StringBuilder sb2 = new StringBuilder();

                    for (int j = 0; j < s.ArgCount; j++)
                    {
                        sb1.Append("arg" + j.ToString());
                        sb2.Append("object arg" + j.ToString());

                        if (j < s.ArgCount - 1)
                        {
                            sb1.Append(", ");
                            sb2.Append(", ");
                        }
                    }

                    writer.WriteLine("\t\tpublic string {0}({1}) {{ return String.Format(strings[{2}], {3}); }}",
                        s.Name, sb2.ToString(), i, sb1.ToString());
                }
            }

            writer.WriteLine("\t}");
            writer.WriteLine("}");
        }
    }
}
