namespace Visus.Cuid;

using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Abstractions;
using Extensions;
#if NETSTANDARD2_0 || NET472
using System.Diagnostics;
using System.Runtime.InteropServices;
#endif

internal static class Fingerprint
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] Generate(FingerprintVersion version = FingerprintVersion.Two)
    {
        return version == FingerprintVersion.One
                   ? GenerateLegacyIdentity()
                   : GenerateIdentity();
    }

    private static byte[] GenerateIdentity()
    {
        ReadOnlySpan<byte> identity = Encoding.UTF8.GetBytes(GetSystemName());
        ReadOnlySpan<byte> process = GetProcessIdentifier();
        ReadOnlySpan<byte> environment = GetEnvironmentVariables();

        Span<byte> buffer = stackalloc byte[identity.Length + sizeof(int) + environment.Length];

        identity.CopyTo(buffer[..identity.Length]);
        process.CopyTo(buffer[identity.Length..]);
        environment.CopyTo(buffer[( identity.Length + sizeof(int) )..]);

        Utils.GenerateRandom(32).CopyTo(buffer[^32..]);

        return buffer.ToArray();
    }

    private static byte[] GenerateLegacyIdentity()
    {
        string machineName = GetSystemName();

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
        List<string> items =
        [
            Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture).TrimPad(2),
            machineIdentifier.ToString(CultureInfo.InvariantCulture).TrimPad(2)
        ];

        string result = string.Join(string.Empty, items);
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

    private static ReadOnlySpan<byte> GetEnvironmentVariables()
    {
        IEnumerable<string> data = from DictionaryEntry item in Environment.GetEnvironmentVariables()
                                   select $"{item.Key}={item.Value}";

        return Encoding.UTF8.GetBytes(string.Join(string.Empty, data));
    }

    private static ReadOnlySpan<byte> GetProcessIdentifier()
    {
        Span<byte> result = stackalloc byte[sizeof(int)];

#if NET8_0_OR_GREATER
        int processId = Environment.ProcessId;
#else
        int processId = Process.GetCurrentProcess().Id;
#endif

        BinaryPrimitives.WriteInt32LittleEndian(result, processId);

        return result.ToArray();
    }

    private static string GetSystemName()
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
