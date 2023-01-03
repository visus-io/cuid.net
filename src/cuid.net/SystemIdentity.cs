namespace Xaevik.Cuid;

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using Abstractions;
using Extensions;

internal static class SystemIdentity
{
	public static byte[] Generate(IdentityVersion version = IdentityVersion.Two)
	{
		if ( version == IdentityVersion.One )
		{
			return GenerateBasicIdentity();
		}

		if ( OperatingSystem.IsWindows() )
		{
			return GenerateWin32Identity();
		}

		if ( OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() )
		{
			return GenerateUnixIdentity();
		}

		return GenerateBasicIdentity();
	}

	private static byte[] GenerateBasicIdentity()
	{
		string machineName = RetrieveMachineName();

		int machineIdentifier = machineName.Length + 36;
		machineIdentifier = machineName.Aggregate(machineIdentifier, (i, c) => i + c);

		string result = string.Create(4, machineIdentifier, (dest, _) =>
		{
			Environment.ProcessId.ToString(CultureInfo.InvariantCulture).TrimPad(2).WriteTo(ref dest);
			machineIdentifier.ToString(CultureInfo.InvariantCulture).TrimPad(2).WriteTo(ref dest);
		});

		return Encoding.UTF8.GetBytes(result);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static string GenerateMachineName()
	{
		byte[] bytes = Utils.GenerateInsecureRandom(24);
		return Convert.ToHexString(bytes).ToUpperInvariant()[..15];
	}

	[SupportedOSPlatform("linux")]
	[SupportedOSPlatform("macos")]
	private static byte[] GenerateUnixIdentity()
	{
		string machineName = RetrieveMachineName();
		int processIdentifier = Environment.ProcessId;

		return Array.Empty<byte>();
	}

	[SupportedOSPlatform("windows")]
	private static byte[] GenerateWin32Identity()
	{
		string machineName = RetrieveMachineName();
		int processIdentifier = Environment.ProcessId;

		return Array.Empty<byte>();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static string RetrieveMachineName()
	{
		string machineName;
		try
		{
			machineName = !string.IsNullOrWhiteSpace(Environment.MachineName)
				? Environment.MachineName
				: GenerateMachineName();
		}
		catch ( InvalidOperationException )
		{
			machineName = GenerateMachineName();
		}

		return machineName;
	}
}