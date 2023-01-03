namespace Xaevik.Cuid;

using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
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
			_r = Random(),
			_t = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
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

	/// <summary>
	///     Returns a string representation of the value of this instance.
	/// </summary>
	/// <returns>The value of this <see cref="Cuid" />.</returns>
	public override string ToString()
	{
		return string.Create(25, ( _t, _c, _f, _r ), (dest, buffer) =>
		{
			Prefix.WriteTo(ref dest);

			Encode((ulong) buffer._t).WriteTo(ref dest);

			Encode(buffer._c)
				.TrimPad(BlockSize)
				.WriteTo(ref dest);

			buffer._f.WriteTo(ref dest);


			Encode(buffer._r)
				.TrimPad(BlockSize * 2)
				.WriteTo(ref dest);
		});
	}

	private static ulong Decode(ReadOnlySpan<char> input)
	{
		return input.ToString()
			.Select(s => s is >= '0' and <= '9' ? s - '0' : 10 + s - 'a')
			.Aggregate((ulong) 0, (i, c) => ( i * Base ) + (uint) c);
	}

	private static string Encode(ulong value)
	{
		const int length = 32;
		int i = length;
		Span<char> buffer = stackalloc char[length];

		do
		{
			ulong c = value % Base;
			buffer[--i] = (char) ( c is >= 0 and <= 9 ? c + 48 : c + 'a' - 10 );
			value /= Base;
		} while ( value > 0 );

		return new string(buffer.Slice(i, length - i));
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

	private static ulong Random()
	{
		Span<byte> bytes = stackalloc byte[16];
		Context.InsecureRandomSource.NextBytes(bytes);

		ulong item = BinaryPrimitives.ReadUInt64LittleEndian(bytes);
		item *= Context.DiscreteValues;

		return item;
	}

	private static ulong SafeCounter()
	{
		_synchronizedCounter = _synchronizedCounter < Context.DiscreteValues ? _synchronizedCounter : 0;
		_synchronizedCounter++;

		return _synchronizedCounter - 1;
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

		public static readonly Random InsecureRandomSource = new();

		private static string GenerateFingerprint()
		{
			string machineName;
			try
			{
				machineName = Environment.MachineName;
				if ( string.IsNullOrWhiteSpace(machineName) )
				{
					machineName = GenerateMachineName();
				}
			}
			catch ( InvalidOperationException )
			{
				machineName = GenerateMachineName();
			}

			int processIdentifier = Environment.ProcessId;

			int machineIdentifier = machineName.Length + Base;
			machineIdentifier = machineName.Aggregate(machineIdentifier, (i, c) => i + c);

			return string.Create(4, ( processIdentifier, machineIdentifier ), (dest, _) =>
			{
				processIdentifier.ToString(CultureInfo.InvariantCulture).TrimPad(2).WriteTo(ref dest);
				machineIdentifier.ToString(CultureInfo.InvariantCulture).TrimPad(2).WriteTo(ref dest);
			});
		}

		private static string GenerateMachineName()
		{
			Span<byte> bytes = new byte[24];
			BinaryPrimitives.WriteUInt64LittleEndian(bytes, (ulong) InsecureRandomSource.NextInt64());

			return Convert.ToHexString(bytes).ToUpperInvariant()[..15];
		}
	}
}