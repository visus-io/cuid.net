namespace Xaevik.Cuid;

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

internal static class Utils
{
	private static readonly double BitsPerDigit = Math.Log(36, 2);

	private const int ByteBitCount = sizeof(byte) * 8;

	private static readonly BigInteger Radix = new(36);

	private static readonly Random Random = new();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static ulong Decode(ReadOnlySpan<char> input)
	{
		return input.ToString()
			.Select(s => s is >= '0' and <= '9' ? s - '0' : 10 + s - 'a')
			.Aggregate((ulong) 0, (i, c) => ( i * 36 ) + (uint) c);
	}

	internal static string Encode(ReadOnlySpan<byte> value)
	{
		if ( value.IsEmpty || value.Length > 64 )
		{
			return string.Empty;
		}

		int length = (int) Math.Ceiling(value.Length * ByteBitCount / BitsPerDigit);
		int i = length;
		Span<char> buffer = stackalloc char[length];

		BigInteger d = new(value);
		while ( !d.IsZero )
		{
			d = BigInteger.DivRem(d, Radix, out BigInteger r);
			int c = (int) BigInteger.Abs(r);
			buffer[--i] = (char) ( c is >= 0 and <= 9 ? c + 48 : c + 'a' - 10 );
		}

		return new string(buffer.Slice(i, length - i));
	}
	
	internal static string Encode(ulong value)
	{
		if ( value is 0 )
		{
			return string.Empty;
		}
		
		const int length = 32;
		int i = length;
		Span<char> buffer = stackalloc char[length];

		do
		{
			ulong c = value % 36;
			buffer[--i] = (char) ( c is >= 0 and <= 9 ? c + 48 : c + 'a' - 10 );
			value /= 36;
		} while ( value > 0 );

		return new string(buffer.Slice(i, length - i));
	}

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