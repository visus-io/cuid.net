namespace Xaevik.Cuid.Tests;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class CuidFacts
{
	//   _t       _c   _f   _r
	// c lbqylg5v 0001 08mn 7kmn0t1e
	private const string CuidString = "clbqylg5v000108mn7kmn0t1e";

	private const string InvalidCuidString = "xSQcDXq7N6YTJZ7i1zNXCA==";
	
	[Fact]
	public void Cuid_Construct()
	{
		var cuid = new Cuid();

		var cuidString = cuid.ToString();

		var result = cuidString.Length == 25
		             && cuidString.All(char.IsLetterOrDigit)
		             && cuid != Cuid.Empty;

		Assert.True(result);
	}

	[Fact]
	public void Cuid_ConstructFromString()
	{
		var cuid = new Cuid(CuidString);

		Assert.Equal(CuidString, cuid.ToString());
	}

	[Fact]
	public void Cuid_ConstructFromString_ThrowsFormatException()
	{
		Assert.Throws<FormatException>(() => new Cuid(InvalidCuidString));
	}

	[Fact]
	public void Cuid_Equality()
	{
		var c1 = new Cuid(CuidString);
		var c2 = new Cuid(CuidString);

		Assert.True(c1 == c2);
	}

	[Fact]
	public void Cuid_Equals()
	{
		var c1 = new Cuid(CuidString);
		var c2 = new Cuid(CuidString);

		Assert.True(c1.Equals(c2));
		Assert.True(c1.Equals((object) c2));

		Assert.True(c1.CompareTo(null) > 0);
		Assert.True(c1.CompareTo(c2) == 0);
		Assert.True(c1.CompareTo((object) c2) == 0);
		
		Assert.True(c1 >= c2);
		Assert.True(c1 <= c2);

		Assert.True(c1.GetHashCode() == c2.GetHashCode());
	}

	[Fact]
	public void Cuid_GreaterThan()
	{
		var c1 = new Cuid("clbqylg5v000108mn7kmn0t1e");
		var c2 = new Cuid("clbqylg5v000208mn7kmn0t1e");

		Assert.True(c2 > c1);
	}

	[Fact]
	public void Cuid_Inequality()
	{
		var c1 = new Cuid();
		var c2 = new Cuid(CuidString);

		Assert.False(c1 == c2);
	}

	[Fact]
	public void Cuid_LessThan()
	{
		var c1 = new Cuid("clbqylg5v000108mn7kmn0t1e");
		var c2 = new Cuid("clbqylg5v000208mn7kmn0t1e");

		Assert.True(c1 < c2);
	}

	[Fact]
	public void Cuid_NotEquals()
	{
		var c1 = new Cuid();
		var c2 = new Cuid(CuidString);

		Assert.False(c1.Equals(c2));
		Assert.False(c1.Equals((object) c2));

		Assert.False(c1.CompareTo(c2) == 0);
		Assert.False(c1.CompareTo((object) c2) == 0);

		Assert.False(c1.GetHashCode() == c2.GetHashCode());
	}

	[Fact]
	public void Cuid_Parse()
	{
		var cuid = Cuid.Parse(CuidString);

		Assert.Equal(CuidString, cuid.ToString());
	}

	[Fact]
	public void Cuid_Parse_ThrowsFormatException()
	{
		Assert.Throws<FormatException>(() => Cuid.Parse(InvalidCuidString));
	}

	[Fact]
	public void Cuid_TryParse_ReturnsFalse()
	{
		var result = Cuid.TryParse(InvalidCuidString, out var cuid);

		Assert.False(result);
		Assert.Equal(cuid, Cuid.Empty);
	}

	[Fact]
	public void Cuid_TryParse_ReturnsTrue()
	{
		var result = Cuid.TryParse(CuidString, out var cuid);

		Assert.True(result);
		Assert.Equal(CuidString, cuid.ToString());
	}
}