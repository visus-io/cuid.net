namespace Xaevik.Cuid.Tests;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class Cuid2Facts
{
	[Fact]
	public void Cuid2_Constructor()
	{
		var cuid = new Cuid2();

		var cuidString = cuid.ToString();

		var result = cuidString.Length == 24
		             && cuidString.All(char.IsLetterOrDigit);

		Assert.True(result);
	}
}