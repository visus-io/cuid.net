namespace Visus.Cuid;

using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

internal static class Utils
{
	private static readonly BigInteger BigRadix = new(36);

	private static readonly double BitsPerDigit = Math.Log(36, 2);

	private const int Radix = 36;

#if NETSTANDARD2_0 || NET472
	private static readonly Random Random = new();
#endif

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static long Decode(ReadOnlySpan<char> input)
	{
		return input.ToString()
					.Select(s => s is >= '0' and <= '9' ? s - '0' : 10 + s - 'a')
					.Aggregate((long) 0, (i, c) => ( i * Radix ) + c);
	}

	internal static string Encode(ReadOnlySpan<byte> value)
	{
		if ( value.IsEmpty )
		{
			return string.Empty;
		}

		int length = (int) Math.Ceiling(value.Length * 8 / BitsPerDigit);
		int i = length;
		Span<char> buffer = stackalloc char[length];

#if NET6_0_OR_GREATER
			BigInteger d = new(value, true);
#else
		byte[] unsigned = value.ToArray().Concat(new byte[] { 00 }).ToArray();
		BigInteger d = new(unsigned);
#endif
		while ( !d.IsZero )
		{
			d = BigInteger.DivRem(d, BigRadix, out BigInteger r);
			int c = (int) r;

			buffer[--i] = (char) ( c is >= 0 and <= 9 ? c + 48 : c + 'a' - 10 );
		}

#if NET6_0_OR_GREATER
			return new string(buffer.Slice(i, length - i));
#else
		return new string(buffer.Slice(i, length - i).ToArray());
#endif
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
			ulong c = value % Radix;
			buffer[--i] = (char) ( c <= 9 ? c + 48 : c + 'a' - 10 );

			value /= Radix;
		} while ( value > 0 );

#if NET6_0_OR_GREATER
			return new string(buffer.Slice(i, length - i));
#else
		return new string(buffer.Slice(i, length - i).ToArray());
#endif
	}

	internal static char GenerateCharacterPrefix()
	{
#if NET6_0_OR_GREATER
			int c = RandomNumberGenerator.GetInt32(97, 122);
#else
		int c = Random.Next(97, 122);
#endif
		return (char) c;
	}

#if NET6_0_OR_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	internal static byte[] GenerateRandom(int length = 8)
	{
#if NET6_0_OR_GREATER
			return RandomNumberGenerator.GetBytes(length);
#else
		byte[] seed = new byte[length];

		using RandomNumberGenerator crypto = RandomNumberGenerator.Create();
		crypto.GetBytes(seed);

		return seed;
#endif
	}
}
