namespace Xaevik.Cuid.Extensions;

using System.Runtime.CompilerServices;

internal static class StringExtensions
{
	internal static string TrimPad(this string source, int size)
	{
		return string.IsNullOrWhiteSpace(source)
			? string.Empty
			: source.PadLeft(9, '0')[^size..];
	}

	internal static void WriteTo(this ReadOnlySpan<char> source, ref Span<char> destination)
	{
		source.CopyTo(destination);
		destination = destination[source.Length..];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void WriteTo(this string source, ref Span<char> destination)
	{
		source.AsSpan().WriteTo(ref destination);
	}
}