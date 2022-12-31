namespace Xaevik.Cuid;

using System.Buffers.Binary;
using System.Numerics;
using System.Security.Cryptography;

/// <summary>
///     Represents a collision resistant unique identifier (CUID).
/// </summary>
public readonly struct Cuid2
{
	//private const int LengthCeiling = 32;

	private const int ValueLength = 24;

	private readonly uint _c;

	private readonly string _f = default!;

	private readonly char _p;

	private readonly ulong _r;

	private readonly long _t;

	/// <summary>
	///     Initializes a new instance of the <see cref="Cuid2" /> structure.
	/// </summary>
	/// <returns>A new CUID object.</returns>
	public Cuid2()
	{
		_p = GeneratePrefix();
		_t = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		_r = GenerateSecureRandom();
		_c = Context.Counter++;
		// _f = Context.Fingerprint;
	}

	/// <summary>
	///     Returns a string representation of the value of this instance.
	/// </summary>
	/// <returns>The value of this <see cref="Cuid2" />.</returns>
	public override string ToString()
	{
		int bytesWritten;
		bool success;

		Span<byte> result = stackalloc byte[64];
		Span<byte> buffer = stackalloc byte[64];
		
		BinaryPrimitives.WriteInt64LittleEndian(buffer, _t);
		BinaryPrimitives.WriteUInt64LittleEndian(buffer, _r);
		BinaryPrimitives.WriteUInt32LittleEndian(buffer, _c);
		// _f

		using ( SHA512 hash = SHA512.Create() )
		{
			success = hash.TryComputeHash(buffer, result, out bytesWritten);
		}

		if ( !success || bytesWritten != 64 )
		{
			throw new InvalidOperationException();
		}

		return _p + Encode(result)[..( ValueLength - 1 )];
	}

	private static string Encode(ReadOnlySpan<byte> input)
	{
		if ( input.IsEmpty )
		{
			return string.Empty;
		}
		
		int length = (int) Math.Ceiling(input.Length * Context.ByteBitCount / Context.BitsPerDigit);
		int i = length;

		Span<char> buffer = stackalloc char[length];

		BigInteger d = new(input);
		while ( !d.IsZero )
		{
			d = BigInteger.DivRem(d, Context.Radix, out BigInteger r);
			int c = (int) ( r > 0 ? r : -r );
			buffer[--i] = (char) ( c is >= 0 and <= 9 ? c + 48 : c + 'a' - 10 );
		}

		return new string(buffer.Slice(i, length - i));
	}

	private static char GeneratePrefix()
	{
		int c = Context.InsecureRandomSource.Next(26);

		return c > 13 ? char.ToLowerInvariant((char) ( 'a' + c )) : (char) ( 'a' + c );
	}

	private static ulong GenerateSecureRandom()
	{
		Span<byte> bytes = stackalloc byte[8];
		RandomNumberGenerator.Fill(bytes);

		return BinaryPrimitives.ReadUInt64LittleEndian(bytes);
	}

	private static class Context
	{
		public static readonly string Fingerprint = GenerateFingerprint();
		public static readonly double BitsPerDigit = Math.Log(36, 2);

		public const int ByteBitCount = sizeof(byte) * 8;

		public static readonly Random InsecureRandomSource = new();

		public static readonly BigInteger Radix = new(36);

		public static uint Counter;

		static Context()
		{
			Counter = (uint) InsecureRandomSource.Next() * 2057;
		}

		private static string GenerateFingerprint()
		{
			return default!;
			// string machineName = Environment.MachineName;
			// int processIdentifier = Environment.ProcessId;
			//
			// int machineIdentifier = machineName.Length + Base;
			// machineIdentifier = machineName.Aggregate(machineIdentifier, (i, c) => i + c);
			//
			// string id = Pad(processIdentifier.ToString(CultureInfo.InvariantCulture), 2);
			// string name = Pad(machineIdentifier.ToString(CultureInfo.InvariantCulture), 2);
			//
			// return $"{id}{name}";
		}
	}
}