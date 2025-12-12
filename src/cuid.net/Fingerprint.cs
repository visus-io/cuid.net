namespace Visus.Cuid;

using System.Buffers.Binary;
using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Abstractions;
using Extensions;
#if NETSTANDARD
using System.Diagnostics;
using System.Runtime.InteropServices;
#endif

internal static class Fingerprint
{
    private static readonly Lazy<byte[]> CachedEnvironmentVariables = new(ComputeEnvironmentVariables);

#if NETSTANDARD
    private static readonly int CachedProcessId = Process.GetCurrentProcess().Id;
#else
    private static readonly int CachedProcessId = Environment.ProcessId;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] Generate(FingerprintVersion version = FingerprintVersion.Two)
    {
        return version == FingerprintVersion.One
                   ? GenerateLegacyIdentity()
                   : GenerateIdentity();
    }

    private static byte[] ComputeEnvironmentVariables()
    {
        IEnumerable<string> data = from DictionaryEntry item in Environment.GetEnvironmentVariables()
                                   orderby item.Key
                                   select $"{item.Key}={item.Value}";

        return Encoding.UTF8.GetBytes(string.Join(string.Empty, data));
    }

    private static byte[] GenerateIdentity()
    {
        string systemName = GetSystemName();
        byte[] environment = CachedEnvironmentVariables.Value;

#if NETSTANDARD2_0
        byte[] identity = Encoding.UTF8.GetBytes(systemName);
        Span<byte> buffer = stackalloc byte[identity.Length + sizeof(int) + environment.Length];

        identity.CopyTo(buffer[..identity.Length]);
        BinaryPrimitives.WriteInt32LittleEndian(buffer[identity.Length..(identity.Length + sizeof(int))], CachedProcessId);
        environment.CopyTo(buffer[(identity.Length + sizeof(int))..]);
#else
        int systemNameByteCount = Encoding.UTF8.GetByteCount(systemName);
        Span<byte> buffer = stackalloc byte[systemNameByteCount + sizeof(int) + environment.Length];

        Encoding.UTF8.GetBytes(systemName, buffer[..systemNameByteCount]);
        BinaryPrimitives.WriteInt32LittleEndian(buffer[systemNameByteCount..( systemNameByteCount + sizeof(int) )], CachedProcessId);
        environment.CopyTo(buffer[( systemNameByteCount + sizeof(int) )..]);
#endif

        return buffer.ToArray();
    }

    private static byte[] GenerateLegacyIdentity()
    {
        string machineName = GetSystemName();

        int machineIdentifier = machineName.Length + 36;
        machineIdentifier = machineName.Aggregate(machineIdentifier, (i, c) => i + c);

#if NETSTANDARD2_0
        char[] buffer = new char[4];
        Span<char> dest = buffer;

        CachedProcessId.ToString(CultureInfo.InvariantCulture)
                       .TrimPad(2).WriteTo(ref dest);

        machineIdentifier.ToString(CultureInfo.InvariantCulture)
                         .TrimPad(2).WriteTo(ref dest);

        string result = new(buffer);
#else
        string result = string.Create(4, machineIdentifier, (dest, _) =>
        {
            CachedProcessId.ToString(CultureInfo.InvariantCulture)
                           .TrimPad(2).WriteTo(ref dest);

            machineIdentifier.ToString(CultureInfo.InvariantCulture)
                             .TrimPad(2).WriteTo(ref dest);
        });
#endif

        return Encoding.UTF8.GetBytes(result);
    }

    private static string GenerateSystemName()
    {
        byte[] bytes = Utils.GenerateRandom(32);

#if NETSTANDARD
        bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        string hostname = string.Concat(Array.ConvertAll(bytes,
            b => b.ToString("X2", CultureInfo.InvariantCulture)));
#else
        bool isWindows = OperatingSystem.IsWindows();
        string hostname = Convert.ToHexString(bytes);
#endif

        return isWindows
                   ? hostname[..15] // windows hostnames are limited to 15 characters 
                   : hostname;
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
