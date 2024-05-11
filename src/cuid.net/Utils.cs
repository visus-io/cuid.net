namespace Visus.Cuid
{
	using System;
	using System.Numerics;
	using System.Security.Cryptography;
#if NET8_0_OR_GREATER
	using System.Linq;
	using System.Runtime.CompilerServices;
#endif

	internal static class Utils
	{
#if NET8_0_OR_GREATER
		private static readonly BigInteger BigRadix = new(36);
#else
		private static readonly BigInteger BigRadix = new BigInteger(36);
#endif

		private static readonly double BitsPerDigit = Math.Log(36, 2);

		private const int Radix = 36;

#if NET8_0_OR_GREATER
		private static readonly Random Random = new();
#else
		private static readonly Random Random = new Random();
#endif

#if NET8_0_OR_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static long Decode(ReadOnlySpan<char> input)
		{
			return input.ToString()
						.Select(s => s is >= '0' and <= '9' ? s - '0' : 10 + s - 'a')
						.Aggregate((long) 0, (i, c) => ( i * Radix ) + c);
		}
#else
		internal static long Decode(ReadOnlySpan<char> input)
		{
			return 0;
		}
#endif

#if NET8_0_OR_GREATER
		internal static string Encode(ReadOnlySpan<byte> value)
		{
			if ( value.IsEmpty )
			{
				return string.Empty;
			}

			int length = (int) Math.Ceiling(value.Length * 8 / BitsPerDigit);
			int i = length;
			Span<char> buffer = stackalloc char[length];

			BigInteger d = new(value, true);
			while ( !d.IsZero )
			{
				d = BigInteger.DivRem(d, BigRadix, out BigInteger r);
				int c = (int) r;
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
				ulong c = value % Radix;
				buffer[--i] = (char) ( c is >= 0 and <= 9 ? c + 48 : c + 'a' - 10 );
				value /= Radix;
			} while ( value > 0 );

			return new string(buffer.Slice(i, length - i));
		}
#else
		internal static string Encode(ReadOnlySpan<byte> value)
		{
			return string.Empty;
		}

		internal static string Encode(ulong value)
		{
			return string.Empty;
		}
#endif

		internal static char GenerateCharacterPrefix()
		{
			int c = Random.Next(26);
			return c > 13 ? char.ToLowerInvariant((char) ( 'a' + c )) : (char) ( 'a' + c );
		}

#if NET8_0_OR_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static byte[] GenerateRandom(int length = 8)
		{
			return RandomNumberGenerator.GetBytes(length);
		}
#else
		internal static byte[] GenerateRandom(int length = 8)
		{
			byte[] seed = new byte[length];
			RandomNumberGenerator.Create().GetBytes(seed);

			return seed;
		}
#endif
	}
}
