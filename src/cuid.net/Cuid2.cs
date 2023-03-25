namespace Visus.Cuid;

using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Org.BouncyCastle.Crypto.Digests;

/// <summary>
///     Represents a collision resistant unique identifier (CUID).
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct Cuid2 : IEquatable<Cuid2>
{
	private const int DefaultLength = 24;

	private readonly ulong _counter = Counter.Instance.Value;

	private readonly byte[] _fingerprint;

	private readonly int _maxLength;

	private readonly char _prefix;

	private readonly byte[] _random;

	private readonly long _timestamp;

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

		_fingerprint = Context.IdentityFingerprint;
		_prefix = Utils.GenerateCharacterPrefix();
		_random = Utils.GenerateRandom(maxLength);
		_timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
	}

	/// <summary>
	///     Indicates whether the values of two specified <see cref="Cuid2" /> objects are equal.
	/// </summary>
	/// <param name="left">The first object to compare.</param>
	/// <param name="right">The second object to compare.</param>
	/// <returns><c>true</c> if <c>left</c> and <c>right</c> are equal; otherwise, <c>false</c>.</returns>
	public static bool operator ==(Cuid2 left, Cuid2 right)
	{
		return left.Equals(right);
	}

	/// <summary>
	///     Indicates whether the values of two specified <see cref="Cuid2" /> objects are not equal.
	/// </summary>
	/// <param name="left">The first object to compare.</param>
	/// <param name="right">The second object to compare.</param>
	/// <returns><c>true</c> if <c>left</c> and <c>right</c> are not equal; otherwise, <c>false</c>.</returns>
	public static bool operator !=(Cuid2 left, Cuid2 right)
	{
		return !left.Equals(right);
	}

	/// <inheritdoc />
	public bool Equals(Cuid2 other)
	{
		return _counter == other._counter &&
		       _fingerprint.Equals(other._fingerprint) &&
		       _prefix == other._prefix &&
		       _random.Equals(other._random) &&
		       _timestamp == other._timestamp;
	}

	/// <inheritdoc />
	public override bool Equals(object? obj)
	{
		return obj is Cuid2 other && Equals(other);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		return HashCode.Combine(_counter, _fingerprint, _prefix, _random, _timestamp);
	}

	/// <summary>
	///     Returns a string representation of the value of this instance.
	/// </summary>
	/// <returns>The value of this <see cref="Cuid2" />.</returns>
	public override string ToString()
	{
		Span<byte> data = stackalloc byte[16];
		Span<byte> result = stackalloc byte[64];

		BinaryPrimitives.WriteInt64LittleEndian(data[..8], _timestamp);
		BinaryPrimitives.WriteUInt64LittleEndian(data[^8..], _counter);

		Sha3Digest digest = new(512);

		digest.BlockUpdate(data);
		digest.BlockUpdate(_fingerprint);
		digest.BlockUpdate(_random);

		int bytesWritten = digest.DoFinal(result);

		if ( bytesWritten != 64 )
		{
			return string.Empty;
		}

		return _prefix + Utils.Encode(result.ToArray())[..( _maxLength - 1 )];
	}

	private static class Context
	{
		public static readonly byte[] IdentityFingerprint = Fingerprint.Generate();
	}

	private sealed class Counter
	{
		// ReSharper disable once InconsistentNaming
		private static readonly Lazy<Counter> _counter = new(() => new Counter());

		private ulong _value;

		private Counter()
		{
			_value = BinaryPrimitives.ReadUInt64LittleEndian(Utils.GenerateRandom()) * 476782367;
		}

		public static Counter Instance => _counter.Value;

		public ulong Value => Interlocked.Increment(ref _value);
	}
}