namespace Visus.Cuid
{
	using System;
	using System.Buffers.Binary;
	using System.Diagnostics.CodeAnalysis;
	using System.Runtime.CompilerServices;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Text.Json.Serialization;
	using System.Threading;
	using System.Xml;
	using System.Xml.Schema;
	using System.Xml.Serialization;
	using Abstractions;
	using CommunityToolkit.Diagnostics;
	using Extensions;
	using Serialization.Json.Converters;
#if NETSTANDARD2_0
	using System.Collections.Generic;
#endif
#if NET8_0_OR_GREATER
	using Extensions;
#endif

	/// <summary>
	///     Represents a collision resistant unique identifier (CUID).
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	[JsonConverter(typeof(CuidConverter))]
	[XmlRoot("cuid")]
#if NET8_0_OR_GREATER
	[Obsolete(Obsoletions.CuidMessage, DiagnosticId = Obsoletions.CuidDiagId)]
#else
	[Obsolete(Obsoletions.CuidMessage)]
#endif
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

		private readonly string _fingerprint;

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
			Guard.IsNotNullOrWhiteSpace(c);

#if NET8_0_OR_GREATER
			CuidResult result = new();
#else
			CuidResult result = new CuidResult();
#endif

			_ = TryParseCuid(c.AsSpan(), true, ref result);

			this = result.ToCuid();
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Cuid" /> structure.
		/// </summary>
		/// <returns>A new CUID object.</returns>
		public static Cuid NewCuid()
		{
#if NET8_0_OR_GREATER
			CuidResult result = new()
			{
				_counter = Counter.Instance.Value,
				_fingerprint = Context.IdentityFingerprint,
				_random = BinaryPrimitives.ReadInt64LittleEndian(Utils.GenerateRandom()),
				_timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
			};
#else
			CuidResult result = new CuidResult
			{
				_counter = Counter.Instance.Value,
				_fingerprint = Context.IdentityFingerprint,
				_random = BinaryPrimitives.ReadInt64LittleEndian(Utils.GenerateRandom()),
				_timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
			};
#endif

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
			Guard.IsNotNullOrWhiteSpace(input);
			return Parse(input.AsSpan());
		}

		/// <summary>
		///     Converts a read-only character span that represents a CUID to the equivalent <see cref="Cuid" /> structure.
		/// </summary>
		/// <param name="input">A read-only span containing the bytes representing a CUID.</param>
		/// <returns>A structure that contains the value that was parsed.</returns>
		[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
		public static Cuid Parse(ReadOnlySpan<char> input)
		{
#if NET8_0_OR_GREATER
			CuidResult result = new();
#else
			CuidResult result = new CuidResult();
#endif

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
#if NET8_0_OR_GREATER
			CuidResult parseResult = new();
#else
			CuidResult parseResult = new CuidResult();
#endif
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
#if NET8_0_OR_GREATER
		public static bool TryParse([NotNullWhen(true)] string? input, out Cuid result)
#else
		public static bool TryParse([AllowNull] string input, out Cuid result)
#endif
		{
			if ( !string.IsNullOrWhiteSpace(input) )
			{
				return TryParse(input.AsSpan(), out result);
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
			return HashCode.Combine(_counter, _fingerprint, _random, _timestamp);
		}

		/// <inheritdoc />
		public void ReadXml(XmlReader reader)
		{
			reader.Read();

#if NET8_0_OR_GREATER
			CuidResult result = new();
#else
			CuidResult result = new CuidResult();
#endif

			_ = TryParseCuid(reader.Value.AsSpan(), true, ref result);

			Unsafe.AsRef(in this) = result.ToCuid();
		}

		/// <summary>
		///     Returns a string representation of the value of this instance.
		/// </summary>
		/// <returns>The value of this <see cref="Cuid" />.</returns>
		public override string ToString()
		{
#if NET8_0_OR_GREATER
			return string.Create(25, ( _t: _timestamp, _c: _counter, _f: _fingerprint, _r: _random ),
								 (dest, buffer) =>
								 {
									 Prefix.WriteTo(ref dest);

									 Utils.Encode((ulong) buffer._t)
										  .WriteTo(ref dest);

									 Utils.Encode(buffer._c)
										  .TrimPad(BlockSize)
										  .WriteTo(ref dest);

									 buffer._f.WriteTo(ref dest);

									 Utils.Encode(buffer._r)
										  .TrimPad(BlockSize * 2)
										  .WriteTo(ref dest);
								 });
#else
			return string.Empty;
#endif
		}

		/// <inheritdoc />
		public void WriteXml(XmlWriter writer)
		{
			writer.WriteString(ToString());
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
			if ( cuidString.Length != ValueLength || !cuidString.StartsWith(Prefix.AsSpan()) || !IsAlphaNum(cuidString) )
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
			result._timestamp = Utils.Decode(timestamp);

			return true;
		}

		[ExcludeFromCodeCoverage]
		XmlSchema? IXmlSerializable.GetSchema()
		{
			return null;
		}

		[SuppressMessage("ReSharper", "InconsistentNaming")]
		[StructLayout(LayoutKind.Explicit)]
		private struct CuidResult
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public readonly Cuid ToCuid()
			{
#if NET8_0_OR_GREATER
				CuidResult result = this;
				return Unsafe.As<CuidResult, Cuid>(ref Unsafe.AsRef(ref result));
#else
				return Unsafe.As<CuidResult, Cuid>(ref Unsafe.AsRef(in this));
#endif
			}

			#pragma warning disable CA1822
			// ReSharper disable once MemberCanBeMadeStatic.Local
			internal readonly void SetFailure(string message)
			{
				throw new FormatException(message);
			}
			#pragma warning restore CA1822

			#pragma warning disable S4487
			[FieldOffset(8)]
			internal long _counter;

			[FieldOffset(0)]
			internal string _fingerprint;

			[FieldOffset(16)]
			internal long _random;

			[FieldOffset(24)]
			internal long _timestamp;
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
#if NET8_0_OR_GREATER
			private static readonly Lazy<Counter> _counter = new(() => new Counter());
#else
			private static readonly Lazy<Counter> _counter = new Lazy<Counter>(() => new Counter());
#endif

			private static readonly long DiscreteValues = (long) Math.Pow(36, 4);

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
}
