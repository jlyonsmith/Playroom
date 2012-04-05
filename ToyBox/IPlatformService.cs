using System;

namespace ToyBox
{
	public enum GamePlatform
	{
		Unknown,
		Windows7,
		WindowsPhone7,
		iPhone3,
		iPhone4,
		iPad2,
		iPad4
	}
	
	public interface IPlatformService
	{
		GamePlatform Platform { get; }
	}
}

