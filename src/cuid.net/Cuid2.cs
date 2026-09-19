namespace Visus.Cuid;

using System.Buffers.Binary;
using System.Collections;
using System.Runtime.InteropServices;
using CommunityToolkit.Diagnostics;
using Org.BouncyCastle.Crypto.Digests;
#if !NETSTANDARD
using System.Security.Cryptography;
#endif

/// <summary>
///     Represents a collision resistant unique identifier (CUID).
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct Cuid2 : IEquatable<Cuid2>
{
    private const int s_defaultLength = 24;

    [ThreadStatic]
    private static Sha3Digest s_digest;

#if !NETSTANDARD
    [ThreadStatic]
    private static IncrementalHash s_nativeDigest;
#endif

    private readonly long _counter;

    private readonly byte[] _fingerprint;

    private readonly int _maxLength;

    private readonly char _prefix;

    private readonly byte[] _random;

    private readonly long _timestamp;

    private readonly string _value;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Cuid2" /> structure.
    /// </summary>
    /// <remarks>The structure will initialize with a default maximum length of 24.</remarks>
    /// <returns>A new CUID object.</returns>
    public Cuid2()
        : this(s_defaultLength)
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
        Guard.IsInRange(maxLength, 4, 33);

#if NETSTANDARD
        #pragma warning disable S6588 // DateTimeOffset.UnixEpoch is not available in .NET Standard 2.0
        #pragma warning disable MA0114 // Use DateTimeOffset.UnixEpoch where available
        long unixEpochTicks = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).Ticks;
        #pragma warning restore MA0114 // Use DateTimeOffset.UnixEpoch where available
        #pragma warning restore S6588 // DateTimeOffset.UnixEpoch is not available in .NET Standard 2.0

        _timestamp = DateTimeOffset.UtcNow.Ticks - unixEpochTicks;
#else
        _timestamp = ( DateTimeOffset.UtcNow - DateTimeOffset.UnixEpoch ).Ticks;
#endif

        _counter = Counter.Instance.Value;
        _maxLength = maxLength;
        _fingerprint = Context.IdentityFingerprint;
        _prefix = Utils.GenerateCharacterPrefix();
        _random = Utils.GenerateRandom(maxLength);

        _value = ComputeValue();
    }

    /// <summary>
    ///     Indicates whether the values of two specified <see cref="Cuid2" /> objects are equal.
    /// </summary>
    /// <param name="left">The first object to compare.</param>
    /// <param name="right">The second object to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator ==(Cuid2 left, Cuid2 right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Indicates whether the values of two specified <see cref="Cuid2" /> objects are not equal.
    /// </summary>
    /// <param name="left">The first object to compare.</param>
    /// <param name="right">The second object to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator !=(Cuid2 left, Cuid2 right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public bool Equals(Cuid2 other)
    {
        // Check all scalar fields first (fast path)
        if ( _counter != other._counter ||
             _maxLength != other._maxLength ||
             _prefix != other._prefix ||
             _timestamp != other._timestamp )
        {
            return false;
        }

        // Handle null arrays (default struct case)
        bool bothNull = _fingerprint == null && other._fingerprint == null &&
                        _random == null && other._random == null;

        if ( bothNull )
        {
            return true;
        }

        // If only one side has null arrays, they're not equal
        bool eitherNull = _fingerprint == null || other._fingerprint == null ||
                          _random == null || other._random == null;

        if ( eitherNull )
        {
            return false;
        }

        // Check reference equality (optimization) or compare array contents
        return ( ReferenceEquals(_fingerprint, other._fingerprint) || _fingerprint.SequenceEqual(other._fingerprint) ) &&
               ( ReferenceEquals(_random, other._random) || _random.SequenceEqual(other._random) );
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        return obj is Cuid2 other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        int fingerprintHash = _fingerprint != null
                                  ? StructuralComparisons.StructuralEqualityComparer.GetHashCode(_fingerprint)
                                  : 0;

        int randomHash = _random != null
                             ? StructuralComparisons.StructuralEqualityComparer.GetHashCode(_random)
                             : 0;

        return HashCode.Combine(_counter, _maxLength, fingerprintHash, _prefix, randomHash, _timestamp);
    }

    /// <summary>
    ///     Returns a string representation of the value of this instance.
    /// </summary>
    /// <returns>The value of this <see cref="Cuid2" />.</returns>
    public override string ToString()
    {
        return _value ?? new string('0', s_defaultLength);
    }

    private static Sha3Digest GetOrCreateDigest()
    {
        return s_digest ??= new Sha3Digest(512);
    }

    private string ComputeValue()
    {
#if NETSTANDARD
        return ComputeValueFallback();
#else
        return SHA3_512.IsSupported
                   ? ComputeValueNative()
                   : ComputeValueFallback();
#endif
    }

    private string ComputeValueFallback()
    {
        Span<byte> buffer = stackalloc byte[16];

        BinaryPrimitives.WriteInt64LittleEndian(buffer[..8], _timestamp);
        BinaryPrimitives.WriteInt64LittleEndian(buffer[^8..], _counter);

        Sha3Digest digest = GetOrCreateDigest();

#if NETSTANDARD
        digest.BlockUpdate(buffer.ToArray(), 0, buffer.Length);
        digest.BlockUpdate(_fingerprint, 0, _fingerprint.Length);
        digest.BlockUpdate(_random, 0, _random.Length);

        byte[] hash = new byte[digest.GetDigestSize()];
        digest.DoFinal(hash, 0);
#else
        digest.BlockUpdate(buffer);
        digest.BlockUpdate(_fingerprint);
        digest.BlockUpdate(_random);

        Span<byte> hash = stackalloc byte[digest.GetDigestSize()];
        digest.DoFinal(hash);
#endif

        return _prefix + Utils.Encode(hash)[..( _maxLength - 1 )];
    }

    private static class Context
    {
        public static readonly byte[] IdentityFingerprint = Fingerprint.Generate();
    }

    private sealed class Counter
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Lazy<Counter> s_counter = new(() => new Counter());

        private long _value;

        private Counter()
        {
            _value = BinaryPrimitives.ReadInt64LittleEndian(Utils.GenerateRandom()) * 476782367;
        }

        public static Counter Instance => s_counter.Value;

        public long Value => Interlocked.Increment(ref _value);
    }

#if !NETSTANDARD
    private static IncrementalHash GetOrCreateNativeDigest()
    {
        return s_nativeDigest ??= IncrementalHash.CreateHash(HashAlgorithmName.SHA3_512);
    }

    private string ComputeValueNative()
    {
        Span<byte> buffer = stackalloc byte[16];

        BinaryPrimitives.WriteInt64LittleEndian(buffer[..8], _timestamp);
        BinaryPrimitives.WriteInt64LittleEndian(buffer[^8..], _counter);

        IncrementalHash incrementalHash = GetOrCreateNativeDigest();

        incrementalHash.AppendData(buffer);
        incrementalHash.AppendData(_fingerprint);
        incrementalHash.AppendData(_random);

        Span<byte> hash = stackalloc byte[incrementalHash.HashLengthInBytes];
        if ( incrementalHash.TryGetHashAndReset(hash, out int bytesWritten) && bytesWritten == hash.Length )
        {
            return _prefix + Utils.Encode(hash)[..( _maxLength - 1 )];
        }

        return string.Empty;
    }
#endif
}
