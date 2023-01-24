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

	internal static byte[] GenerateRandom(int length, bool secure = true)
	{
		if ( length <= 0 )
		{
			return Array.Empty<byte>();
		}

		Span<byte> bytes = stackalloc byte[length];

		if ( !secure )
		{
			Random.NextBytes(bytes);

			if ( BitConverter.IsLittleEndian )
			{
				bytes.Reverse();
			}
		}
		else
		{
			RandomNumberGenerator.Fill(bytes);
		}

		if ( BitConverter.IsLittleEndian )
		{
			bytes.Reverse();
		}

		return bytes.ToArray();
	}
}