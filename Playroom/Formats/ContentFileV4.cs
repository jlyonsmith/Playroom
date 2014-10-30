using System;
using System.IO;
using System.Collections.Generic;
using ToolBelt;
using TsonLibrary;

namespace Playroom
{
    public class ContentFileV4
    {
        public class Target
        {
			public TsonStringNode Name;
            public List<TsonStringNode> Inputs;
            public List<TsonStringNode> Outputs;
			public TsonStringNode Compiler;
			public TsonObjectNode Parameters;
        }

		public class CompilerSetting
		{
			public CompilerSetting(TsonStringNode name, TsonObjectNode parameters, List<ContentFileV4.CompilerExtension> extensions)
			{
				Name = name;
				Parameters = parameters;
				CompilerExtensions = extensions;
			}
			public TsonStringNode Name;
			public List<ContentFileV4.CompilerExtension> CompilerExtensions;
			public TsonObjectNode Parameters;
		}

		public class CompilerExtension
		{
            public CompilerExtension(List<TsonStringNode> inputs, List<TsonStringNode> outputs)
			{
				Inputs = inputs;
				Outputs = outputs;
			}

            public List<TsonStringNode> Inputs;
            public List<TsonStringNode> Outputs;
		}

        public class NameAndString
        {
            public TsonStringNode Name { get; set; }
            public TsonStringNode Value { get; set; }
        }

        public List<TsonStringNode> CompilerAssemblies;
		public List<ContentFileV4.CompilerSetting> CompilerSettings;
        public List<ContentFileV4.NameAndString> Properties;
		public List<ContentFileV4.Target> Targets;

		public void Load(ParsedPath contentPath)
		{
			try
			{
                var node = new TsonParser().Parse(File.ReadAllText(contentPath));

                throw new NotImplementedException();
			}
			catch (Exception e)
			{
				TsonParseException ye = e as TsonParseException;
				ContentFileException cfe = e as ContentFileException;

				if (ye != null)
				{
                    throw new ContentFileException("Bad TSON", ye);
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
	}
}
