namespace Visus.Cuid
{
	using System;
	using System.Buffers.Binary;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using Abstractions;
	using Extensions;

	internal static class Fingerprint
	{
		public static byte[] Generate(FingerprintVersion version = FingerprintVersion.Two)
		{
			return version == FingerprintVersion.One
					   ? GenerateLegacyIdentity()
					   : GenerateIdentity();
		}

		private static byte[] GenerateIdentity()
		{
			byte[] identity = Encoding.UTF8.GetBytes(RetrieveSystemName());

			Span<byte> buffer = stackalloc byte[identity.Length + 40];

			identity.CopyTo(buffer[..identity.Length]);

			BinaryPrimitives.WriteInt32LittleEndian(
													buffer.Slice(identity.Length + 1, 4),
													Environment.ProcessId
												   );

			BinaryPrimitives.WriteInt32LittleEndian(
													buffer.Slice(identity.Length + 6, 4),
													Environment.CurrentManagedThreadId
												   );

			Utils.GenerateRandom(32).CopyTo(buffer[^32..]);

			return buffer.ToArray();
		}

		private static byte[] GenerateLegacyIdentity()
		{
			string machineName = RetrieveSystemName();

			int machineIdentifier = machineName.Length + 36;
			machineIdentifier = machineName.Aggregate(machineIdentifier, (i, c) => i + c);

			string result = string.Create(4, machineIdentifier, (dest, _) =>
																{
																	Environment.ProcessId
																			   .ToString(CultureInfo.InvariantCulture)
																			   .TrimPad(2).WriteTo(ref dest);
																	machineIdentifier.ToString(CultureInfo.InvariantCulture)
																					 .TrimPad(2).WriteTo(ref dest);
																});

			return Encoding.UTF8.GetBytes(result);
		}

		private static string GenerateSystemName()
		{
			byte[] bytes = Utils.GenerateRandom(32);
			string hostname = Convert.ToHexString(bytes).ToUpperInvariant();

			return OperatingSystem.IsWindows()
					   ? hostname[..15] // windows hostnames are limited to 15 characters 
					   : hostname;
		}

		private static string RetrieveSystemName()
		{
			string machineName;
			try
			{
				machineName = !string.IsNullOrWhiteSpace(Environment.MachineName)
								  ? Environment.MachineName
								  : GenerateSystemName();
			}
			catch ( InvalidOperationException )
			{
				machineName = GenerateSystemName();
			}

			return machineName;
		}
	}
}
