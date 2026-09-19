namespace Visus.Cuid;

#if NETSTANDARD
using System.Buffers.Binary;
#endif
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

internal static class Utils
{
    private static readonly double s_bitsPerDigit = Math.Log(36, 2);

    private const int s_radix = 36;

    internal static long Decode(ReadOnlySpan<char> input)
    {
        long result = 0;

        foreach ( char c in input )
        {
            int digit = c is >= '0' and <= '9' ? c - '0' : 10 + c - 'a';
            result = ( result * s_radix ) + digit;
        }

        return result;
    }

    internal static ulong DecodeUlong(ReadOnlySpan<char> input)
    {
        ulong result = 0;

        foreach ( char c in input )
        {
            ulong digit = c is >= '0' and <= '9' ? (ulong)( c - '0' ) : (ulong)( 10 + c - 'a' );
            result = ( result * s_radix ) + digit;
        }

        return result;
    }

    internal static string Encode(ReadOnlySpan<byte> value)
    {
        if ( value.IsEmpty )
        {
            return string.Empty;
        }

        int limbCount = ( value.Length + 3 ) / 4;

        Span<uint> limbs = stackalloc uint[limbCount];

        PackLimbs(value, limbs);

        int length = (int)Math.Ceiling(value.Length * 8 / s_bitsPerDigit);
        int i = length;

        Span<char> buffer = stackalloc char[length];

        int end = limbCount;

        while ( end > 0 )
        {
            int digit = DivModRadix(limbs, ref end);

            buffer[--i] = (char)( digit is >= 0 and <= 9 ? digit + 48 : digit + 'a' - 10 );
        }

        if ( length - i == 1 && buffer[i] == '0' )
        {
            return string.Empty;
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
            ulong c = value % s_radix;
            buffer[--i] = (char)( c <= 9 ? c + 48 : c + 'a' - 10 );

            value /= s_radix;
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

    private static int DivModRadix(Span<uint> limbs, ref int end)
    {
        ulong remainder = 0;
        int newEnd = 0;

        for ( int j = end - 1; j >= 0; j-- )
        {
            ulong acc = ( remainder << 32 ) | limbs[j];
            uint q = (uint)( acc / s_radix );
            
            limbs[j] = q;
            remainder = acc % s_radix;

            if ( q != 0 && newEnd == 0 )
            {
                newEnd = j + 1;
            }
        }

        end = newEnd;
        return (int)remainder;
    }

    private static void PackLimbs(ReadOnlySpan<byte> value, Span<uint> limbs)
    {
        for ( int limbIndex = 0; limbIndex < limbs.Length; limbIndex++ )
        {
            int byteOffset = limbIndex * 4;
            int limbByteCount = Math.Min(4, value.Length - byteOffset);
            uint limb = 0;

            for ( int b = 0; b < limbByteCount; b++ )
            {
                limb |= (uint)value[byteOffset + b] << ( 8 * b );
            }

            limbs[limbIndex] = limb;
        }
    }
}
