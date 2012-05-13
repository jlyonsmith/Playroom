<<<<<<< HEAD
using System;
using Microsoft.Xna.Framework;

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
		Size AdBannerSize { get; }
	}
	
}

=======
using System;

namespace ToyBox
{
	public enum PlatformType
	{
		Unknown,
		Windows,
		WinPhone7,
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

>>>>>>> 010d98fa093436f265905c05053e928c732ad750
