namespace Xaevik.Cuid;

using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;

public readonly struct Cuid
{
	private const int Base = 36;

	private const int BlockSize = 4;

	private static ulong _synchronizedCounter;

	private readonly ulong _counter;

	private readonly string _fingerprint;

	private readonly string _random;

	private readonly long _timestamp;

	public Cuid()
	{
		_counter = SafeCounter();
		_fingerprint = Context.Fingerprint;
		_random = SecureRandom();
		_timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
	}

	public override string ToString()
	{
		string[] result = new string[5];

		result[0] = "c";
		result[1] = Encode((ulong) _timestamp);
		result[2] = Pad(Encode(_counter), BlockSize);
		result[3] = _fingerprint;
		result[4] = _random;

		return string.Join(string.Empty, result);
	}

	private static ulong Decode(string value)
	{
		return value
			.Select(s => s is >= '0' and <= '9' ? s - '0' : 10 + s - 'a')
			.Aggregate((ulong) 0, (i, c) => ( i * Base ) + (uint) c);
	}

	private static string Encode(ulong value)
	{
		if ( value is 0 )
		{
			return "0";
		}

		string result = string.Empty;

		while ( value > 0 )
		{
			ulong c = value % Base;
			result = (char) ( c is >= 0 and <= 9 ? c + 48 : c + 'a' - 10 ) + result;
			value /= Base;
		}

		return result;
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

	private static string SecureRandom()
	{
		Span<byte> bytes = new byte[BlockSize * 2];
		RandomNumberGenerator.Fill(bytes);

		if ( BitConverter.IsLittleEndian )
		{
			bytes.Reverse();
		}

		ulong item = BitConverter.ToUInt64(bytes);
		item *= Context.DiscreteValues;

		return Pad(Encode(item), BlockSize * 2);
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