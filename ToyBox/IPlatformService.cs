using System;

namespace ToyBox
{
	public enum PlatformType
	{
		Unknown,
		Windows7,
		WindowsPhone7,
		iPhone3,
		iPhone4,
		iPad2,
		iPad3
	}
	
	public interface IPlatformService
	{
		PlatformType Platform { get; }
	}
}

