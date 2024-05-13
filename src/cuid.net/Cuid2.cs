namespace Visus.Cuid
{
#if NET6_0_OR_GREATER
	using NSec.Cryptography;
#endif

	using System;
	using System.Buffers.Binary;
	using System.Collections;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Threading;
	using CommunityToolkit.Diagnostics;
#if NETSTANDARD2_0
	using Org.BouncyCastle.Crypto.Digests;
#endif

	/// <summary>
	///     Represents a collision resistant unique identifier (CUID).
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public readonly struct Cuid2 : IEquatable<Cuid2>
	{
		/// <summary>
		///     A read-only instance of <see cref="Cuid2" /> structure whose values are all zeros.
		/// </summary>
		public static readonly Cuid2 Empty;

		private const int DefaultLength = 24;

		private readonly long _counter;

		private readonly byte[] _fingerprint;

		private readonly int _maxLength;

		private readonly char _prefix;

		private readonly byte[] _random;

		private readonly long _timestamp;

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

			_counter = Counter.Instance.Value;
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
		[SuppressMessage("ReSharper", "SimplifyConditionalTernaryExpression")]
		public bool Equals(Cuid2 other)
		{
			return ( _counter == other._counter &&
					 _fingerprint == null ) || ( _fingerprint.SequenceEqual(other._fingerprint) &&
												 _prefix == other._prefix &&
												 _random == null ) || ( _random.SequenceEqual(other._random) &&
																		_timestamp == other._timestamp );
		}

		/// <inheritdoc />
#if NET6_0_OR_GREATER
		public override bool Equals(object? obj)
#else
		public override bool Equals(object obj)
#endif
		{
			return obj is Cuid2 other && Equals(other);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return HashCode.Combine(_counter, StructuralComparisons.StructuralEqualityComparer.GetHashCode(_fingerprint), _prefix, _random, _timestamp);
		}

		/// <summary>
		///     Returns a string representation of the value of this instance.
		/// </summary>
		/// <returns>The value of this <see cref="Cuid2" />.</returns>
		public override string ToString()
		{
			if ( _counter == 0 ||
				 _fingerprint == null ||
				 _maxLength == 0 ||
				 _prefix == char.MinValue ||
				 _random == null ||
				 _timestamp == 0 )
			{
				return new string('0', DefaultLength);
			}

			Span<byte> buffer = stackalloc byte[16];

			BinaryPrimitives.WriteInt64LittleEndian(buffer[..8], _timestamp);
			BinaryPrimitives.WriteInt64LittleEndian(buffer[^8..], _counter);

#if NET6_0_OR_GREATER
			IncrementalHash.Initialize(HashAlgorithm.Sha512, out IncrementalHash state);

			IncrementalHash.Update(ref state, buffer);
			IncrementalHash.Update(ref state, _fingerprint);
			IncrementalHash.Update(ref state, _random);

			byte[] hash = IncrementalHash.Finalize(ref state);
#else
			Sha3Digest digest = new(512);

			digest.BlockUpdate(buffer.ToArray(), 0, buffer.Length);
			digest.BlockUpdate(_fingerprint, 0, _fingerprint.Length);
			digest.BlockUpdate(_random, 0, _random.Length);

			byte[] hash = new byte[digest.GetByteLength()];
			digest.DoFinal(hash, 0);
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
			private static readonly Lazy<Counter> _counter = new(() => new Counter());

			private long _value;

			private Counter()
			{
				_value = BinaryPrimitives.ReadInt64LittleEndian(Utils.GenerateRandom()) * 476782367;
			}

			public static Counter Instance => _counter.Value;

			public long Value => Interlocked.Increment(ref _value);
		}
	}
}
