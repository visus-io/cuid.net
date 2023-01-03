namespace Xaevik.Cuid;

using System.Runtime.CompilerServices;
using System.Security.Cryptography;

internal static class Utils
{
	private static readonly Random Random = new();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static char GenerateCharacterPrefix()
	{
		int c = Random.Next(26);
		return c > 13 ? char.ToLowerInvariant((char) ( 'a' + c )) : (char) ( 'a' + c );
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static string GenerateMachineName()
	{
		byte[] bytes = GenerateInsecureRandom(24);
		return Convert.ToHexString(bytes).ToUpperInvariant()[..15];
	}

	internal static byte[] GenerateSecureRandom(int length)
	{
		if ( length <= 0 )
		{
			return Array.Empty<byte>();
		}

		Span<byte> bytes = stackalloc byte[length];
		RandomNumberGenerator.Fill(bytes);

		if ( BitConverter.IsLittleEndian )
		{
			bytes.Reverse();
		}

		return bytes.ToArray();
	}

	private static byte[] GenerateInsecureRandom(int length)
	{
		if ( length <= 0 )
		{
			return Array.Empty<byte>();
		}

		Span<byte> bytes = stackalloc byte[length];
		Random.NextBytes(bytes);

		if ( BitConverter.IsLittleEndian )
		{
			bytes.Reverse();
		}

		return bytes.ToArray();
	}
}