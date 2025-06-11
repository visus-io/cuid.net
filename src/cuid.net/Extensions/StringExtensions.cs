namespace Visus.Cuid.Extensions;

#if NET8_0_OR_GREATER
using System;
#endif

internal static class StringExtensions
{
    internal static string TrimPad(this string source, int size)
    {
        return string.IsNullOrWhiteSpace(source)
                   ? string.Empty
                   : source.PadLeft(9, '0')[^size..];
    }

#if NET8_0_OR_GREATER
    internal static void WriteTo(this string source, ref Span<char> destination)
    {
        source.AsSpan().WriteToInternal(ref destination);
    }

    private static void WriteToInternal(this ReadOnlySpan<char> source, ref Span<char> destination)
    {
        source.CopyTo(destination);
        destination = destination[source.Length..];
    }
#endif
}
