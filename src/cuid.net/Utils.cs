namespace Xaevik.Cuid;

using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Extensions;

internal static class Utils
{
	private static readonly Random Random = new();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static ulong Decode(ReadOnlySpan<char> input)
	{
		return input.ToString()
			.Select(s => s is >= '0' and <= '9' ? s - '0' : 10 + s - 'a')
			.Aggregate((ulong) 0, (i, c) => ( i * 36 ) + (uint) c);
	}

	internal static string Encode(byte[] value)
	{
		if ( value.Length is 0 or > 64 )
		{
			return string.Empty;
		}

		return string.Create(128, value, (dest, buffer) =>
		{
			Encode(BinaryPrimitives.ReadUInt64LittleEndian(buffer.AsSpan()[..8])).WriteTo(ref dest);
			Encode(BinaryPrimitives.ReadUInt64LittleEndian(buffer.AsSpan()[8..16])).WriteTo(ref dest);
			Encode(BinaryPrimitives.ReadUInt64LittleEndian(buffer.AsSpan()[16..24])).WriteTo(ref dest);
			Encode(BinaryPrimitives.ReadUInt64LittleEndian(buffer.AsSpan()[24..32])).WriteTo(ref dest);
			Encode(BinaryPrimitives.ReadUInt64LittleEndian(buffer.AsSpan()[32..40])).WriteTo(ref dest);
			Encode(BinaryPrimitives.ReadUInt64LittleEndian(buffer.AsSpan()[40..48])).WriteTo(ref dest);
			Encode(BinaryPrimitives.ReadUInt64LittleEndian(buffer.AsSpan()[48..56])).WriteTo(ref dest);
			Encode(BinaryPrimitives.ReadUInt64LittleEndian(buffer.AsSpan()[^56..])).WriteTo(ref dest);
		});
	}
	internal static string Encode(ulong value)
	{
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