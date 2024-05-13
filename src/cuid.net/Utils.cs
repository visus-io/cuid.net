namespace Visus.Cuid
{
	using System;
	using System.Linq;
	using System.Numerics;
	using System.Runtime.CompilerServices;
	using System.Security.Cryptography;

	internal static class Utils
	{
#if NET6_0_OR_GREATER
		private static readonly BigInteger BigRadix = new(36);
#else
		private static readonly BigInteger BigRadix = new BigInteger(36);
#endif

		private static readonly double BitsPerDigit = Math.Log(36, 2);

		private const int Radix = 36;

#if NET6_0_OR_GREATER
		private static readonly Random Random = new();
#else
		private static readonly Random Random = new Random();
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static long Decode(ReadOnlySpan<char> input)
		{
#if NET6_0_OR_GREATER
			return input.ToString()
						.Select(s => s is >= '0' and <= '9' ? s - '0' : 10 + s - 'a')
						.Aggregate((long) 0, (i, c) => ( i * Radix ) + c);
#else
			return input.ToString()
						.Select(s => s >= '0' && s <= '9' ? s - '0' : 10 + s - 'a')
						.Aggregate((long) 0, (i, c) => ( i * Radix ) + c);
#endif
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
			ulong unsigned = BitConverter.ToUInt64(value.ToArray(), 0);
			BigInteger d = new BigInteger(unsigned);
#endif
			while ( !d.IsZero )
			{
				d = BigInteger.DivRem(d, BigRadix, out BigInteger r);
				int c = (int) r;

#if NET6_0_OR_GREATER
				buffer[--i] = (char) ( c is >= 0 and <= 9 ? c + 48 : c + 'a' - 10 );
#else
				c += c >= 0 && c <= 9 ? 48 : 'a' - 10;
				buffer[--i] = (char) c;
#endif
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

#if NET6_0_OR_GREATER
				buffer[--i] = (char) ( c is >= 0 and <= 9 ? c + 48 : c + 'a' - 10 );
#else
				c += (ulong) ( c <= 9 ? 48 : 'a' - 10 );
				buffer[--i] = (char) c;
#endif

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
			int c = Random.Next(26);
			return c > 13 ? char.ToLowerInvariant((char) ( 'a' + c )) : (char) ( 'a' + c );
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
			RandomNumberGenerator.Create().GetBytes(seed);

			return seed;
#endif
		}
	}
}
