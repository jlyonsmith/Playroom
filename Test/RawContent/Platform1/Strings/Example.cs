using System;

namespace Playroom
{
	public static class SpiderStrings
	{
		public static string[] Strings { get; set; }
		
		public static string MessageNoParams
		{
			get
			{
				return Strings[0];
			}
		}
		
		public static string MessageOneParam(object param1)
		{
			return String.Format(Strings[1], param1);
		}
	}
}