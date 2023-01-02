namespace Xaevik.Cuid;

using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

/// <summary>
///     Represents a collision resistant unique identifier (CUID).
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct Cuid2
{
	private const int DefaultLength = 24;

	private readonly uint _c = Counter.Instance.Value;

	private readonly byte[] _f;

	private readonly int _maxLength;

	private readonly char _p;

	private readonly ulong _r;

	private readonly long _t;

	/// <summary>
	///     Initializes a new instance of the <see cref="Cuid2" /> structure.
	/// </summary>
	/// <remarks>The structure will initialize with a default maximum length of 24.</remarks>
	/// <returns>A new CUID object.</returns>
	public Cuid2()
		: this(DefaultLength)
	{
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="Cuid2" /> structure.
	/// </summary>
	/// <param name="maxLength">Defines the maximum string length value of <see cref="Cuid2" />.</param>
	/// <remarks>The value defined for <paramref name="maxLength" /> cannot be less than 4 or greater than 32.</remarks>
	/// <exception cref="ArgumentOutOfRangeException">
	///     The value of <paramref name="maxLength" /> was less than 4 or greater
	///     than 32.
	/// </exception>
	public Cuid2(int maxLength)
	{
		if ( maxLength is < 4 or > 32 )
		{
			throw new ArgumentOutOfRangeException(nameof(maxLength),
				string.Format(Resources.Resources.Arg_Cuid2IntCtor, "4", "32"));
		}

		_p = GeneratePrefix();
		_t = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		_r = GenerateSecureRandom();
		_f = Context.Fingerprint;

		_maxLength = maxLength;
	}

	/// <summary>
	///     Returns a string representation of the value of this instance.
	/// </summary>
	/// <returns>The value of this <see cref="Cuid2" />.</returns>
	public override string ToString()
	{
		int bytesWritten;
		bool success;

		Span<byte> buffer = stackalloc byte[20];
		Span<byte> salt = stackalloc byte[8];
		
		Span<byte> result = stackalloc byte[64];

		RandomNumberGenerator.Fill(salt);
		
		BinaryPrimitives.WriteInt64LittleEndian(buffer[..8], _t);
		BinaryPrimitives.WriteUInt64LittleEndian(buffer.Slice(8, 8), _r);
		BinaryPrimitives.WriteUInt32LittleEndian(buffer.Slice(16, 4), _c);
		
		using ( IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA512) )
		{
			//var x = buffer.Length + _f.Length + salt.Length;
			
			hash.AppendData(buffer);
			hash.AppendData(_f);
			hash.AppendData(salt);
			
			success = hash.TryGetHashAndReset(result, out bytesWritten);
		}

		if ( !success || bytesWritten != 64 )
		{
			throw new InvalidOperationException();
		}

		return _p + Encode(result)[..( _maxLength - 1 )];
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
		public static readonly double BitsPerDigit = Math.Log(36, 2);

		public const int ByteBitCount = sizeof(byte) * 8;
		
		public static readonly byte[] Fingerprint = HardwareIdentity.Generate();

		public static readonly Random InsecureRandomSource = new();

		public static readonly BigInteger Radix = new(36);
	}

	private sealed class Counter
	{
		// ReSharper disable once InconsistentNaming
		private static readonly Lazy<Counter> _counter = new(() => new Counter());

		private volatile uint _value;

		private Counter()
		{
			Random random = new(Guid.NewGuid().GetHashCode());
			_value = (uint) random.Next() * 2057;
		}

		public static Counter Instance => _counter.Value;

		public uint Value => Interlocked.Increment(ref _value);
	}
}