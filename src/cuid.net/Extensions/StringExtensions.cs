namespace Visus.Cuid.Extensions;

internal static class StringExtensions
{
    internal static string TrimPad(this string source, int size)
    {
        return string.IsNullOrWhiteSpace(source)
                   ? new string('0', size)
                   : source.PadLeft(size, '0')[^size..];
    }

    internal static void WriteTo(this string source, ref Span<char> destination)
    {
        source.AsSpan().WriteToInternal(ref destination);
    }

    private static void WriteToInternal(this ReadOnlySpan<char> source, ref Span<char> destination)
    {
        source.CopyTo(destination);
        destination = destination[source.Length..];
    }
}
