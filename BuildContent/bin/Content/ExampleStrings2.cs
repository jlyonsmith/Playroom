//
// This file was generated on 2/6/2013 10:39:44 AM.
//

using System;

namespace 
{
	public class ExampleStringsStrings
	{
		private string[] strings;

		public ExampleStringsStrings(string[] strings)
		{
			this.strings = strings;
		}

		public string String1 { get { return strings[0]; } }
		public string String2(object arg0) { return String.Format(strings[1], arg0); }
		public string String3(object arg0, object arg1) { return String.Format(strings[2], arg0, arg1); }
	}
}
