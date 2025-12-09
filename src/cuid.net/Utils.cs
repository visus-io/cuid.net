namespace Visus.Cuid;

using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

internal static class Utils
{
    private static readonly BigInteger BigRadix = new(36);

    private static readonly double BitsPerDigit = Math.Log(36, 2);

    private const int Radix = 36;

    internal static long Decode(ReadOnlySpan<char> input)
    {
        long result = 0;

        foreach ( char c in input )
        {
            int digit = c is >= '0' and <= '9' ? c - '0' : 10 + c - 'a';
            result = ( result * Radix ) + digit;
        }

        return result;
    }

    internal static ulong DecodeUlong(ReadOnlySpan<char> input)
    {
        ulong result = 0;

        foreach ( char c in input )
        {
            ulong digit = c is >= '0' and <= '9' ? (ulong)( c - '0' ) : (ulong)( 10 + c - 'a' );
            result = ( result * Radix ) + digit;
        }

        return result;
    }

    internal static string Encode(ReadOnlySpan<byte> value)
    {
        if ( value.IsEmpty )
        {
            return string.Empty;
        }

        int length = (int)Math.Ceiling(value.Length * 8 / BitsPerDigit);
        int i = length;

        Span<char> buffer = stackalloc char[length];

#if NETSTANDARD2_0
        byte[] unsigned = new byte[value.Length + 1];
        value.CopyTo(unsigned);

        BigInteger d = new(unsigned);
#else
        BigInteger d = new(value, true);
#endif

        while ( !d.IsZero )
        {
            d = BigInteger.DivRem(d, BigRadix, out BigInteger r);
            int c = (int)r;

            buffer[--i] = (char)( c is >= 0 and <= 9 ? c + 48 : c + 'a' - 10 );
        }

#if NETSTANDARD2_0
        return new string(buffer[i..length].ToArray());
#else
        return new string(buffer[i..length]);
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
            buffer[--i] = (char)( c <= 9 ? c + 48 : c + 'a' - 10 );

            value /= Radix;
        } while ( value > 0 );

#if NETSTANDARD2_0
        return new string(buffer[i..length].ToArray());
#else
        return new string(buffer[i..length]);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static char GenerateCharacterPrefix()
    {
#if NETSTANDARD
        byte[] buffer = new byte[4];

        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        rng.GetBytes(buffer);

        uint value = BinaryPrimitives.ReadUInt32LittleEndian(buffer);
        int c = (int)( value % 26 ) + 97;
#else
        int c = RandomNumberGenerator.GetInt32(97, 123);
#endif

        return (char)c;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static byte[] GenerateRandom(int length = 8)
    {
#if NETSTANDARD
        byte[] buffer = new byte[length];

        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        rng.GetBytes(buffer);
        return buffer;
#else
        return RandomNumberGenerator.GetBytes(length);
#endif
    }
}
