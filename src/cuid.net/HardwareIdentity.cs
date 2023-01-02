namespace Xaevik.Cuid;

using System.Runtime.Versioning;

internal static class HardwareIdentity
{
	public static byte[] Generate()
	{
		if ( OperatingSystem.IsWindows() )
		{
			return GenerateWin32Identity();
		}

		if ( OperatingSystem.IsLinux() )
		{
			return GenerateUnixIdentity();
		}

		if ( OperatingSystem.IsMacOS() )
		{
			return Array.Empty<byte>();
		}

		return Array.Empty<byte>();
	}

	[SupportedOSPlatform("linux")]
	private static byte[] GenerateUnixIdentity()
	{
		return Array.Empty<byte>();
	}

	[SupportedOSPlatform("windows")]
	private static byte[] GenerateWin32Identity()
	{
		return Array.Empty<byte>();
	}
}