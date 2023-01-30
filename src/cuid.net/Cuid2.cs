namespace Xaevik.Cuid;

using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Digests;

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

	private readonly byte[] _r;

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

		_maxLength = maxLength;

		_f = Context.IdentityFingerprint;
		_p = Utils.GenerateCharacterPrefix();
		_r = Utils.GenerateRandom(maxLength * 2);
		_t = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
	}

	/// <summary>
	///     Returns a string representation of the value of this instance.
	/// </summary>
	/// <returns>The value of this <see cref="Cuid2" />.</returns>
	public override string ToString()
	{
		Span<byte> buffer = stackalloc byte[16];
		Span<byte> result = stackalloc byte[64];

		BinaryPrimitives.WriteInt64LittleEndian(buffer, _t);
		BinaryPrimitives.WriteUInt32LittleEndian(buffer[..8], _c);

		Sha3Digest digest = new(512);

		digest.BlockUpdate(buffer);
		digest.BlockUpdate(_f);
		digest.BlockUpdate(_r);

		int bytesWritten = digest.DoFinal(result);

		if ( bytesWritten != 64 )
		{
			return string.Empty;
		}

		return _p + Utils.Encode(result.ToArray())[..( _maxLength - 1 )];
	}

	private static class Context
	{
		public static readonly byte[] IdentityFingerprint = Fingerprint.Generate();
	}

	private sealed class Counter
	{
		// ReSharper disable once InconsistentNaming
		private static readonly Lazy<Counter> _counter = new(() => new Counter());

		private volatile uint _value;

		private Counter()
		{
			_value = BitConverter.ToUInt32(RandomNumberGenerator.GetBytes(sizeof(uint)));
		}

		public static Counter Instance => _counter.Value;

		public uint Value => Interlocked.Increment(ref _value);
	}
}