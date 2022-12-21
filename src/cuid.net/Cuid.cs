namespace Xaevik.Cuid;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Serialization.Json.Converters;

/// <summary>
///     Represents a collision resistant unique identifier (CUID).
/// </summary>
[StructLayout(LayoutKind.Sequential)]
[JsonConverter(typeof(CuidConverter))]
[XmlRoot("cuid")]
public readonly struct Cuid : IComparable, IComparable<Cuid>, IEquatable<Cuid>, IXmlSerializable
{
	/// <summary>
	///     A read-only instance of <see cref="Cuid" /> structure whose values are all zeros.
	/// </summary>
	public static readonly Cuid Empty;

	private const int Base = 36;

	private const int BlockSize = 4;

	private const string Prefix = "c";

	private const int ValueLength = 25;

	private static ulong _synchronizedCounter;

	private readonly ulong _c;

	private readonly string _f = default!;

	private readonly ulong _r;

	private readonly long _t;

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
			_c = SafeCounter(),
			_f = Context.Fingerprint,
			_r = SecureRandom(),
			_t = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
		};

		return result.ToCuid();
	}

	public static bool operator ==(Cuid left, Cuid right)
	{
		return left.Equals(right);
	}

	public static bool operator >(Cuid left, Cuid right)
	{
		return left.CompareTo(right) > 0;
	}

	public static bool operator >=(Cuid left, Cuid right)
	{
		return left.CompareTo(right) >= 0;
	}

	public static bool operator !=(Cuid left, Cuid right)
	{
		return !left.Equals(right);
	}

	public static bool operator <(Cuid left, Cuid right)
	{
		return left.CompareTo(right) < 0;
	}

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
		int cComparison = _c.CompareTo(other._c);
		if ( cComparison != 0 )
		{
			return cComparison;
		}

		int fComparison = string.Compare(_f, other._f, StringComparison.OrdinalIgnoreCase);
		if ( fComparison != 0 )
		{
			return fComparison;
		}

		int rComparison = _r.CompareTo(other._r);
		return rComparison != 0 ? rComparison : _t.CompareTo(other._t);
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
		return _c == other._c && string.Equals(_f, other._f, StringComparison.OrdinalIgnoreCase) &&
		       _r == other._r && _t == other._t;
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

		hashCode.Add(_c);
		hashCode.Add(_f, StringComparer.OrdinalIgnoreCase);
		hashCode.Add(_r);
		hashCode.Add(_t);

		return hashCode.ToHashCode();
	}

	public override string ToString()
	{
		string[] result = new string[5];

		result[0] = Prefix;
		result[1] = Encode((ulong) _t);
		result[2] = Pad(Encode(_c), BlockSize);
		result[3] = _f;
		result[4] = Pad(Encode(_r), BlockSize * 2);

		return string.Join(string.Empty, result);
	}

	private static ulong Decode(ReadOnlySpan<char> input)
	{
		return input.ToString()
			.Select(s => s is >= '0' and <= '9' ? s - '0' : 10 + s - 'a')
			.Aggregate((ulong) 0, (i, c) => ( i * Base ) + (uint) c);
	}

	private static string Encode(ulong value)
	{
		if ( value is 0 )
		{
			return "0";
		}

		Stack<char> result = new();

		while ( value > 0 )
		{
			ulong c = value % Base;
			result.Push((char) ( c is >= 0 and <= 9 ? c + 48 : c + 'a' - 10 ));
			value /= Base;
		}

		return new string(result.ToArray());
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

	private static string Pad(string value, int size)
	{
		string result = $"000000000{value}";

		return result[^size..];
	}

	private static ulong SafeCounter()
	{
		_synchronizedCounter = _synchronizedCounter < Context.DiscreteValues ? _synchronizedCounter : 0;
		_synchronizedCounter++;

		return _synchronizedCounter - 1;
	}

	private static ulong SecureRandom()
	{
		const int size = BlockSize * 2;

		Span<byte> bytes = stackalloc byte[size];
		RandomNumberGenerator.Fill(bytes);

		if ( BitConverter.IsLittleEndian )
		{
			bytes.Reverse();
		}

		ulong item = BitConverter.ToUInt64(bytes);
		item *= Context.DiscreteValues;

		return item;
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

		result._c = Decode(counter);
		result._f = fingerprint.ToString();
		result._r = Decode(random);
		result._t = (long) Decode(timestamp);

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
		[FieldOffset(8)] internal ulong _c;

		[FieldOffset(0)] internal string _f;

		[FieldOffset(16)] internal ulong _r;

		[FieldOffset(24)] internal long _t;
#pragma warning restore S4487
	}

	private static class Context
	{
		public static readonly ulong DiscreteValues = (ulong) Math.Pow(Base, BlockSize);

		public static readonly string Fingerprint = GenerateFingerprint();

		private static string GenerateFingerprint()
		{
			string machineName = Environment.MachineName;
			int processIdentifier = Environment.ProcessId;

			int machineIdentifier = machineName.Length + Base;
			machineIdentifier = machineName.Aggregate(machineIdentifier, (i, c) => i + c);

			string id = Pad(processIdentifier.ToString(CultureInfo.InvariantCulture), 2);
			string name = Pad(machineIdentifier.ToString(CultureInfo.InvariantCulture), 2);

			return $"{id}{name}";
		}
	}
}