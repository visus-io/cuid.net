namespace Xaevik.Cuid;

using System.Buffers.Binary;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Abstractions;
using Extensions;

internal static class Fingerprint
{
	public static byte[] Generate(FingerprintVersion version = FingerprintVersion.Two)
	{
		return version == FingerprintVersion.One
			? GenerateLegacyIdentity()
			: GenerateIdentity();
	}

	private static byte[] GenerateIdentity()
	{
		byte[] system = Encoding.UTF8.GetBytes(RetrieveSystemName());
		byte[] process = new byte[4];
		byte[] thread = new byte[4];

		Span<byte> buffer = stackalloc byte[system.Length + process.Length + thread.Length];

		if ( !BinaryPrimitives.TryWriteInt32LittleEndian(process, Environment.ProcessId) )
		{
			RandomNumberGenerator.Fill(process);
		}

		if ( !BinaryPrimitives.TryWriteInt32LittleEndian(thread, Environment.CurrentManagedThreadId) )
		{
			RandomNumberGenerator.Fill(thread);
		}

		system.CopyTo(buffer[..system.Length]);
		process.CopyTo(buffer.Slice(system.Length + 1, process.Length));
		thread.CopyTo(buffer[^thread.Length..]);

		if ( buffer.Length > 32 )
		{
			return buffer[..32].ToArray();
		}

		int diff = 32 - buffer.Length;

		Span<byte> result = stackalloc byte[buffer.Length + diff];
		Span<byte> random = stackalloc byte[diff];

		RandomNumberGenerator.Fill(random);

		buffer.CopyTo(result[..buffer.Length]);
		random.CopyTo(result[^diff..]);

		return result.ToArray();

	}

	private static byte[] GenerateLegacyIdentity()
	{
		string machineName = RetrieveSystemName();

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
	private static string GenerateSystemName()
	{
		byte[] bytes = Utils.GenerateInsecureRandom(24);
		return Convert.ToHexString(bytes).ToUpperInvariant()[..15];
	}

	private static string RetrieveSystemName()
	{
		string machineName;
		try
		{
			machineName = !string.IsNullOrWhiteSpace(Environment.MachineName)
				? Environment.MachineName
				: GenerateSystemName();
		}
		catch ( InvalidOperationException )
		{
			machineName = GenerateSystemName();
		}

		return machineName;
	}
}