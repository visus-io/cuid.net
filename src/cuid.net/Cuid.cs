namespace Xaevik.Cuid;

using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Abstractions;
using Extensions;
using Serialization.Json.Converters;

/// <summary>
///     Represents a collision resistant unique identifier (CUID).
/// </summary>
[StructLayout(LayoutKind.Sequential)]
[JsonConverter(typeof(CuidConverter))]
[XmlRoot("cuid")]
[Obsolete(Obsoletions.CuidMessage, DiagnosticId = Obsoletions.CuidDiagId)]
public readonly struct Cuid : IComparable, IComparable<Cuid>, IEquatable<Cuid>, IXmlSerializable
{
	/// <summary>
	///     A read-only instance of <see cref="Cuid" /> structure whose values are all zeros.
	/// </summary>
	public static readonly Cuid Empty;
	
	private const int BlockSize = 4;

	private const string Prefix = "c";

	private const int ValueLength = 25;

	private readonly ulong _counter;

	private readonly string _fingerprint = default!;

	private readonly ulong _random;

	private readonly long _timestamp;

	/// <summary>
	///     Initializes a new instance of the <see cref="Cuid" /> structure by using the value represented by the specified
	///     string.
	/// </summary>
	/// <param name="c">
	///     A string that contains a CUID.
	/// </param>
	public Cuid(string c)
	{
		ArgumentNullException.ThrowIfNull(c);

		CuidResult result = new();

		_ = TryParseCuid(c, true, ref result);

		this = result.ToCuid();
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="Cuid" /> structure.
	/// </summary>
	/// <returns>A new CUID object.</returns>
	public static Cuid NewCuid()
	{
		CuidResult result = new()
		{
			_counter = Counter.Instance.Value,
			_fingerprint = Context.IdentityFingerprint,
			_random = BinaryPrimitives.ReadUInt64LittleEndian(Utils.GenerateRandom()),
			_timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
		};

		return result.ToCuid();
	}

	/// <summary>
	///     Indicates whether the values of two specified <see cref="Cuid" /> objects are equal.
	/// </summary>
	/// <param name="left">The first object to compare.</param>
	/// <param name="right">The second object to compare.</param>
	/// <returns><c>true</c> if <c>left</c> and <c>right</c> are equal; otherwise, <c>false</c>.</returns>
	public static bool operator ==(Cuid left, Cuid right)
	{
		return left.Equals(right);
	}

	/// <summary>
	///     Compares two values to determine which is greater.
	/// </summary>
	/// <param name="left">The first object to compare.</param>
	/// <param name="right">The second object to compare.</param>
	/// <returns><c>true</c> if <c>left</c> is greater than <c>right</c>; otherwise, <c>false</c>.</returns>
	public static bool operator >(Cuid left, Cuid right)
	{
		return left.CompareTo(right) > 0;
	}

	/// <summary>
	///     Compares two values to determine which is greater or equal.
	/// </summary>
	/// <param name="left">The first object to compare.</param>
	/// <param name="right">The second object to compare.</param>
	/// <returns><c>true</c> if <c>left</c> is greater than or equal to <c>right</c>; otherwise, <c>false</c>.</returns>
	public static bool operator >=(Cuid left, Cuid right)
	{
		return left.CompareTo(right) >= 0;
	}

	/// <summary>
	///     Indicates whether the values of two specified <see cref="Cuid" /> objects are not equal.
	/// </summary>
	/// <param name="left">The first object to compare.</param>
	/// <param name="right">The second object to compare.</param>
	/// <returns><c>true</c> if <c>left</c> and <c>right</c> are not equal; otherwise, <c>false</c>.</returns>
	public static bool operator !=(Cuid left, Cuid right)
	{
		return !left.Equals(right);
	}

	/// <summary>
	///     Compares two values to determine which is less.
	/// </summary>
	/// <param name="left">The first object to compare.</param>
	/// <param name="right">The second object to compare.</param>
	/// <returns><c>true</c> if <c>left</c> is less than <c>right</c>; otherwise, <c>false</c>.</returns>
	public static bool operator <(Cuid left, Cuid right)
	{
		return left.CompareTo(right) < 0;
	}

	/// <summary>
	///     Compares two values to determine which is less or equal.
	/// </summary>
	/// <param name="left">The first object to compare.</param>
	/// <param name="right">The second object to compare.</param>
	/// <returns><c>true</c> if <c>left</c> is less than or equal to <c>right</c>; otherwise, <c>false</c>.</returns>
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
		ArgumentNullException.ThrowIfNull(input);

		return Parse((ReadOnlySpan<char>) input);
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
	///     When this method returns, contains the parsed value. If the method returns <c>true</c>,
	///     <c>result</c> contains a valid Guid. If the method returns <c>false</c>, result equals <see cref="Empty" />.
	/// </param>
	/// <returns><c>true</c> if the parse operation was successful; otherwise, <c>false</c>.</returns>
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
	///     When this method returns, contains the parsed value. If the method returns <c>true</c>,
	///     <c>result</c> contains a valid Guid. If the method returns <c>false</c>, result equals <see cref="Empty" />.
	/// </param>
	/// <returns><c>true</c> if the parse operation was successful; otherwise, <c>false</c>.</returns>
	public static bool TryParse([NotNullWhen(true)] string? input, out Cuid result)
	{
		if ( input is not null )
		{
			return TryParse((ReadOnlySpan<char>) input, out result);
		}

		result = default;
		return false;
	}

	/// <inheritdoc />
	public int CompareTo(Cuid other)
	{
		int cComparison = _counter.CompareTo(other._counter);
		if ( cComparison != 0 )
		{
			return cComparison;
		}

		int fComparison = string.Compare(_fingerprint, other._fingerprint, StringComparison.OrdinalIgnoreCase);
		if ( fComparison != 0 )
		{
			return fComparison;
		}

		int rComparison = _random.CompareTo(other._random);
		return rComparison != 0 ? rComparison : _timestamp.CompareTo(other._timestamp);
	}

	/// <inheritdoc />
	public int CompareTo(object? obj)
	{
		if ( ReferenceEquals(null, obj) )
		{
			return 1;
		}

		return obj is Cuid other
			? CompareTo(other)
			: throw new ArgumentException($"Object must be of type {nameof(Cuid)}");
	}

	/// <inheritdoc />
	public bool Equals(Cuid other)
	{
		return _counter == other._counter &&
		       string.Equals(_fingerprint, other._fingerprint, StringComparison.OrdinalIgnoreCase) &&
		       _random == other._random &&
		       _timestamp == other._timestamp;
	}

	/// <inheritdoc />
	public override bool Equals(object? obj)
	{
		return obj is Cuid other && Equals(other);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		HashCode hashCode = new();

		hashCode.Add(_counter);
		hashCode.Add(_fingerprint, StringComparer.OrdinalIgnoreCase);
		hashCode.Add(_random);
		hashCode.Add(_timestamp);

		return hashCode.ToHashCode();
	}

	/// <summary>
	///     Returns a string representation of the value of this instance.
	/// </summary>
	/// <returns>The value of this <see cref="Cuid" />.</returns>
	public override string ToString()
	{
		return string.Create(25, ( _t: _timestamp, _c: _counter, _f: _fingerprint, _r: _random ), (dest, buffer) =>
		{
			Prefix.WriteTo(ref dest);

			Utils.Encode((ulong) buffer._t).WriteTo(ref dest);

			Utils.Encode(buffer._c)
				.TrimPad(BlockSize)
				.WriteTo(ref dest);

			buffer._f.WriteTo(ref dest);

			Utils.Encode(buffer._r)
				.TrimPad(BlockSize * 2)
				.WriteTo(ref dest);
		});
	}
	
	private static bool IsAlphaNum(ReadOnlySpan<char> input)
	{
		foreach ( char t in input )
		{
			if ( !char.IsLetterOrDigit(t) )
			{
				return false;
			}
		}

		return true;
	}

	private static bool TryParseCuid(ReadOnlySpan<char> cuidString, bool throwException, ref CuidResult result)
	{
		cuidString = cuidString.Trim();
		if ( cuidString.Length != ValueLength || !cuidString.StartsWith(Prefix) || !IsAlphaNum(cuidString) )
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

		result._counter = Utils.Decode(counter);
		result._fingerprint = fingerprint.ToString();
		result._random = Utils.Decode(random);
		result._timestamp = (long) Utils.Decode(timestamp);

		return true;
	}

	[ExcludeFromCodeCoverage]
	XmlSchema? IXmlSerializable.GetSchema()
	{
		return null;
	}

	void IXmlSerializable.ReadXml(XmlReader reader)
	{
		reader.Read();

		CuidResult result = new();

		_ = TryParseCuid(reader.Value, true, ref result);

		Unsafe.AsRef(this) = result.ToCuid();
	}

	void IXmlSerializable.WriteXml(XmlWriter writer)
	{
		writer.WriteString(ToString());
	}

	[SuppressMessage("ReSharper", "InconsistentNaming")]
	[StructLayout(LayoutKind.Explicit)]
	private struct CuidResult
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly Cuid ToCuid()
		{
			return Unsafe.As<CuidResult, Cuid>(ref Unsafe.AsRef(in this));
		}

#pragma warning disable CA1822
		// ReSharper disable once MemberCanBeMadeStatic.Local
		internal readonly void SetFailure(string message)
#pragma warning restore CA1822
		{
			throw new FormatException(message);
		}
#pragma warning disable S4487
		[FieldOffset(8)] internal ulong _counter;

		[FieldOffset(0)] internal string _fingerprint;

		[FieldOffset(16)] internal ulong _random;

		[FieldOffset(24)] internal long _timestamp;
#pragma warning restore S4487
	}

	private static class Context
	{
		public static readonly string IdentityFingerprint = GenerateFingerprint();

		private static string GenerateFingerprint()
		{
			byte[] identity = Fingerprint.Generate(FingerprintVersion.One);

			return Encoding.UTF8.GetString(identity);
		}
	}

	private sealed class Counter
	{
		// ReSharper disable once InconsistentNaming
		private static readonly Lazy<Counter> _counter = new(() => new Counter());
		private static readonly ulong DiscreteValues = (ulong) Math.Pow(36, 4);

		private volatile uint _value;

		public static Counter Instance => _counter.Value;

		public uint Value
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