namespace Xaevik.Cuid.Tests;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class Cuid2Facts
{
	[Fact]
	public void Cuid2_Constructor()
	{
		var cuid = new Cuid2();
		var cuid2 = new Cuid2();

		var cuidString = cuid.ToString();

		var result = cuidString.Length == 24
		             && cuidString.All(char.IsLetterOrDigit);

		Assert.True(result);
	}

	[Fact]
	public void Cuid2_Constructor_DefinedLength()
	{
		var cuid = new Cuid2(10);

		var cuidString = cuid.ToString();

		var result = cuidString.Length == 10
		             && cuidString.All(char.IsLetterOrDigit);

		Assert.True(result);
	}

	[Fact]
	public void Cuid2_Constructor_ThrowsArgumentOutOfRangeException()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => new Cuid2(64));
	}
}