#pragma warning disable CA1724 // Type name conflicts with namespace name
#pragma warning disable MA0049 // Type names should not match namespaces
#pragma warning disable S1133 // Deprecated code should not be used

namespace Visus.Cuid;

using System.Buffers.Binary;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Abstractions;
using CommunityToolkit.Diagnostics;
using Extensions;
using Serialization.Json.Converters;

/// <summary>
///     Represents a collision resistant unique identifier (CUID).
/// </summary>
[StructLayout(LayoutKind.Sequential)]
[JsonConverter(typeof(CuidConverter))]
[XmlRoot("cuid")]
#if NETSTANDARD
[Obsolete(Obsoletions.CuidMessage)]
#else
[Obsolete(Obsoletions.CuidMessage, DiagnosticId = Obsoletions.CuidDiagId)]
#endif
public readonly struct Cuid : IComparable, IComparable<Cuid>, IEquatable<Cuid>, IXmlSerializable
{
    /// <summary>
    ///     A read-only instance of <see cref="Cuid" /> structure whose values are all zeros.
    /// </summary>
    public static readonly Cuid Empty;

    private const int BlockSize = 4;

    // Maximum value that fits in 8 base-36 characters (BlockSize * 2)
    // This is 36^8 - 1 = 2,821,109,907,455
    private const ulong MaxRandomValue = 2821109907455UL;

    private const string Prefix = "c";

    private const int ValueLength = 25;

    private readonly ulong _counter;

    private readonly byte[] _fingerprint;

    private readonly ulong _random;

    private readonly long _timestamp;

    private readonly string _value;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Cuid" /> structure by using the value represented by the specified
    ///     string.
    /// </summary>
    /// <param name="c">
    ///     A string that contains a CUID.
    /// </param>
    public Cuid(string c)
    {
        Guard.IsNotNullOrWhiteSpace(c);
        CuidResult result = new();

        _ = TryParseCuid(c.AsSpan(), true, ref result);

        this = result.ToCuid();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Cuid" /> structure.
    /// </summary>
    /// <returns>A new CUID object.</returns>
    public static Cuid NewCuid()
    {
        // Use 10-microsecond precision (ticks / 10000) to fit in 8 base-36 characters
#if NETSTANDARD
        #pragma warning disable S6588 // DateTimeOffset.UnixEpoch is not available in .NET Standard 2.0
        #pragma warning disable MA0114 // Use DateTimeOffset.UnixEpoch where available
        long unixEpochTicks = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).Ticks;
        #pragma warning restore MA0114 // Use DateTimeOffset.UnixEpoch where available
        #pragma warning restore S6588 // DateTimeOffset.UnixEpoch is not available in .NET Standard 2.0

        long timestamp = ( DateTimeOffset.UtcNow.Ticks - unixEpochTicks ) / 10000;
#else
        long timestamp = ( DateTimeOffset.UtcNow - DateTimeOffset.UnixEpoch ).Ticks / 10000;
#endif

        CuidResult result = new()
        {
            _timestamp = timestamp,
            _counter = (ulong)Counter.Instance.Value,
            _fingerprint = Context.IdentityFingerprint,
            _random = BinaryPrimitives.ReadUInt64LittleEndian(Utils.GenerateRandom()) % MaxRandomValue,
        };

        return result.ToCuid();
    }

    /// <summary>
    ///     Indicates whether the values of two specified <see cref="Cuid" /> objects are equal.
    /// </summary>
    /// <param name="left">The first object to compare.</param>
    /// <param name="right">The second object to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator ==(Cuid left, Cuid right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Compares two values to determine which is greater.
    /// </summary>
    /// <param name="left">The first object to compare.</param>
    /// <param name="right">The second object to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> is greater than <paramref name="right" />; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator >(Cuid left, Cuid right)
    {
        return left.CompareTo(right) > 0;
    }

    /// <summary>
    ///     Compares two values to determine which is greater or equal.
    /// </summary>
    /// <param name="left">The first object to compare.</param>
    /// <param name="right">The second object to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> is greater than or equal to <paramref name="right" />;
    ///     otherwise, <see langword="false" />.
    /// </returns>
    public static bool operator >=(Cuid left, Cuid right)
    {
        return left.CompareTo(right) >= 0;
    }

    /// <summary>
    ///     Indicates whether the values of two specified <see cref="Cuid" /> objects are not equal.
    /// </summary>
    /// <param name="left">The first object to compare.</param>
    /// <param name="right">The second object to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator !=(Cuid left, Cuid right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    ///     Compares two values to determine which is less.
    /// </summary>
    /// <param name="left">The first object to compare.</param>
    /// <param name="right">The second object to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> is less than <paramref name="right" />; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool operator <(Cuid left, Cuid right)
    {
        return left.CompareTo(right) < 0;
    }

    /// <summary>
    ///     Compares two values to determine which is less or equal.
    /// </summary>
    /// <param name="left">The first object to compare.</param>
    /// <param name="right">The second object to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="left" /> is less than or equal to <paramref name="right" />;
    ///     otherwise, <see langword="false" />.
    /// </returns>
    public static bool operator <=(Cuid left, Cuid right)
    {
        return left.CompareTo(right) <= 0;
    }

    /// <summary>
    ///     Converts the string representation of a CUID to the equivalent <see cref="Cuid" /> structure.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>A structure that contains the value that was parsed.</returns>
    public static Cuid Parse(string input)
    {
        return string.IsNullOrWhiteSpace(input) ? Empty : Parse(input.AsSpan());
    }

    /// <summary>
    ///     Converts a read-only character span that represents a CUID to the equivalent <see cref="Cuid" /> structure.
    /// </summary>
    /// <param name="input">A read-only span containing the bytes representing a CUID.</param>
    /// <returns>A structure that contains the value that was parsed.</returns>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static Cuid Parse(ReadOnlySpan<char> input)
    {
        CuidResult result = new();

        _ = TryParseCuid(input, true, ref result);

        return result.ToCuid();
    }

    /// <summary>
    ///     Converts the specified read-only span of characters containing the representation of a CUID to the equivalent
    ///     <see cref="Cuid" /> structure.
    /// </summary>
    /// <param name="input">A span containing the characters representing the CUID to convert.</param>
    /// <param name="result">
    ///     When this method returns, contains the parsed value. If the method returns <see langword="true" />,
    ///     <c>result</c> contains a valid Guid. If the method returns <see langword="false" />, result equals
    ///     <see cref="Empty" />.
    /// </param>
    /// <returns><see langword="true" /> if the parse operation was successful; otherwise, <see langword="false" />.</returns>
    public static bool TryParse(ReadOnlySpan<char> input, out Cuid result)
    {
        CuidResult parseResult = new();

        if ( TryParseCuid(input, false, ref parseResult) )
        {
            result = parseResult.ToCuid();
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    ///     Converts the string representation of a CUID to the equivalent <see cref="Cuid" /> structure.
    /// </summary>
    /// <param name="input">A string containing the CUID to convert.</param>
    /// <param name="result">
    ///     When this method returns, contains the parsed value. If the method returns <see langword="true" />,
    ///     <c>result</c> contains a valid Guid. If the method returns <see langword="false" />, result equals
    ///     <see cref="Empty" />.
    /// </param>
    /// <returns><see langword="true" /> if the parse operation was successful; otherwise, <see langword="false" />.</returns>
    public static bool TryParse([NotNullWhen(true)] string input, out Cuid result)
    {
        if ( !string.IsNullOrWhiteSpace(input) )
        {
            return TryParse(input.AsSpan(), out result);
        }

        result = default;
        return false;
    }

    /// <inheritdoc />
    public int CompareTo(object obj)
    {
        if ( obj is null )
        {
            return 1;
        }

        return obj is Cuid other
                   ? CompareTo(other)
                   : throw new ArgumentException($@"must be of type {nameof(Cuid)}", nameof(obj));
    }

    /// <inheritdoc />
    public int CompareTo(Cuid other)
    {
        int counterComparison = _counter.CompareTo(other._counter);
        if ( counterComparison != 0 )
        {
            return counterComparison;
        }

        int randomComparison = _random.CompareTo(other._random);
        return randomComparison != 0 ? randomComparison : _timestamp.CompareTo(other._timestamp);
    }

    /// <inheritdoc />
    public bool Equals(Cuid other)
    {
        if ( _fingerprint == null )
        {
            return _counter == other._counter &&
                   _random == other._random &&
                   _timestamp == other._timestamp;
        }

        return _counter == other._counter &&
               _fingerprint.SequenceEqual(other._fingerprint) &&
               _random == other._random &&
               _timestamp == other._timestamp;
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        return obj is Cuid other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(_counter, StructuralComparisons.StructuralEqualityComparer.GetHashCode(_fingerprint), _random, _timestamp);
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public XmlSchema GetSchema()
    {
        return null;
    }

    /// <inheritdoc />
    public void ReadXml([NotNull] XmlReader reader)
    {
        Guard.IsNotNull(reader);

        reader.Read();

        CuidResult result = new();

        _ = TryParseCuid(reader.Value.AsSpan(), true, ref result);

        Unsafe.AsRef(in this) = result.ToCuid();
    }

    /// <summary>
    ///     Returns a string representation of the value of this instance.
    /// </summary>
    /// <returns>The value of this <see cref="Cuid" />.</returns>
    public override string ToString()
    {
        return _value ?? string.Empty;
    }

    /// <inheritdoc />
    public void WriteXml([NotNull] XmlWriter writer)
    {
        if ( Equals(Empty) )
        {
            return; // Write nothing for empty Cuid (results in xsi:nil="true")
        }

        writer.WriteString(ToString());
    }

    private static bool IsAlphaNum(ReadOnlySpan<char> input)
    {
        foreach ( char t in input )
        {
            if ( !char.IsLetterOrDigit(t) || char.IsUpper(t) )
            {
                return false;
            }
        }

        return true;
    }

    private static bool TryParseCuid(ReadOnlySpan<char> cuidString, bool throwException, ref CuidResult result)
    {
        cuidString = cuidString.Trim();
        if ( cuidString.Length != ValueLength ||
             !cuidString.StartsWith(Prefix.AsSpan(), StringComparison.Ordinal) ||
             !IsAlphaNum(cuidString) )
        {
            if ( throwException )
            {
                result.SetFailure(Resources.Resources.Format_CuidUnrecognized);
            }

            return false;
        }

        ReadOnlySpan<char> timestamp = cuidString[1..9];
        ReadOnlySpan<char> counter = cuidString[9..^12];
        ReadOnlySpan<char> fingerprint = cuidString[13..^8];
        ReadOnlySpan<char> random = cuidString[^8..];

        result._counter = Utils.DecodeUlong(counter);
        result._fingerprint = Encoding.UTF8.GetBytes(fingerprint.ToString());
        result._random = Utils.DecodeUlong(random);
        result._timestamp = Utils.Decode(timestamp);

        return true;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [StructLayout(LayoutKind.Sequential)]
    private struct CuidResult : IEquatable<CuidResult>
    {
        internal ulong _counter;

        internal byte[] _fingerprint;

        internal ulong _random;

        internal long _timestamp;

        internal string _value;

        public static bool operator ==(CuidResult left, CuidResult right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CuidResult left, CuidResult right)
        {
            return !left.Equals(right);
        }

        public readonly bool Equals(CuidResult other)
        {
            return _counter == other._counter
                && _fingerprint.Equals(other._fingerprint)
                && _random == other._random
                && _timestamp == other._timestamp
                && string.Equals(_value, other._value, StringComparison.Ordinal);
        }

        public override readonly bool Equals(object obj)
        {
            return obj is CuidResult other && Equals(other);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(_counter, _fingerprint, _random, _timestamp, _value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Cuid ToCuid()
        {
            CuidResult result = this;
            result._value = result.ComputeValue();
            return Unsafe.As<CuidResult, Cuid>(ref Unsafe.AsRef(in result));
        }

        #pragma warning disable CA1822
        #pragma warning disable S2325
        #pragma warning disable MA0038 
        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
        internal readonly void SetFailure(string message)
        {
            throw new FormatException(message);
        }
        #pragma warning restore CA1822
        #pragma warning restore S2325
        #pragma warning restore MA0038

        private readonly string ComputeValue()
        {
            if ( _fingerprint == null )
            {
                return string.Empty;
            }

#if NETSTANDARD2_0
            char[] buffer = new char[ValueLength];
            Span<char> dest = buffer;

            Prefix.WriteTo(ref dest);

            Utils.Encode((ulong)_timestamp)
                 .WriteTo(ref dest);

            Utils.Encode(_counter)
                 .TrimPad(BlockSize)
                 .WriteTo(ref dest);

            Encoding.UTF8.GetString(_fingerprint).WriteTo(ref dest);

            Utils.Encode(_random)
                 .TrimPad(BlockSize * 2)
                 .WriteTo(ref dest);

            return new string(buffer);
#else
            return string.Create(ValueLength, ( _t: _timestamp, _c: _counter, _f: _fingerprint, _r: _random ),
                (dest, buffer) =>
                {
                    Prefix.WriteTo(ref dest);

                    Utils.Encode((ulong)buffer._t)
                         .WriteTo(ref dest);

                    Utils.Encode(buffer._c)
                         .TrimPad(BlockSize)
                         .WriteTo(ref dest);

                    Encoding.UTF8.GetString(buffer._f).WriteTo(ref dest);

                    Utils.Encode(buffer._r)
                         .TrimPad(BlockSize * 2)
                         .WriteTo(ref dest);
                });
#endif
        }
    }

    private static class Context
    {
        public static readonly byte[] IdentityFingerprint = Fingerprint.Generate(FingerprintVersion.One);
    }

    private sealed class Counter
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Lazy<Counter> _counter = new(() => new Counter());

        private static readonly long DiscreteValues = (long)Math.Pow(36, 4);

        private volatile int _value;

        public static Counter Instance => _counter.Value;

        public int Value
        {
            get
            {
                _value = _value < DiscreteValues ? _value : 0;
                Interlocked.Increment(ref _value);

                return _value;
            }
        }
    }
}

#pragma warning restore CA1724
