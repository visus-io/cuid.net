namespace Visus.Cuid
{
	using System;
	using System.Buffers.Binary;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using Abstractions;
	using Extensions;
#if NETSTANDARD2_0
	using System.Diagnostics;
	using System.Runtime.InteropServices;
#endif

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

#if NET8_0_OR_GREATER
			BinaryPrimitives.WriteInt32LittleEndian(
													buffer.Slice(identity.Length + 1, 4),
													Environment.ProcessId
												   );
#else
			BinaryPrimitives.WriteInt32LittleEndian(
													buffer.Slice(identity.Length + 1, 4),
													Process.GetCurrentProcess().Id
												   );
#endif

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

#if NET8_0_OR_GREATER
			string result = string.Create(4, machineIdentifier, (dest, _) =>
																{
																	Environment.ProcessId
																			   .ToString(CultureInfo.InvariantCulture)
																			   .TrimPad(2).WriteTo(ref dest);
																	machineIdentifier.ToString(CultureInfo.InvariantCulture)
																					 .TrimPad(2).WriteTo(ref dest);
																});
#else
			StringBuilder sb = new StringBuilder();
			
			sb.Append(Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture).TrimPad(2));
			sb.Append(machineIdentifier.ToString(CultureInfo.InvariantCulture).TrimPad(2));

			string result = sb.ToString();
#endif

			return Encoding.UTF8.GetBytes(result);
		}

		private static string GenerateSystemName()
		{
			byte[] bytes = Utils.GenerateRandom(32);

#if NET8_0_OR_GREATER
			string hostname = Convert.ToHexString(bytes).ToUpperInvariant();
			return OperatingSystem.IsWindows()
					   ? hostname[..15] // windows hostnames are limited to 15 characters 
					   : hostname;
#else
			string hostname = BitConverter.ToString(bytes);
			return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
					   ? hostname[..15]
					   : hostname;
#endif
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
