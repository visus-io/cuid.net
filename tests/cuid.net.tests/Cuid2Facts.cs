namespace Visus.Cuid.Tests
{
	using System;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using Xunit;

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

			Debug.WriteLine(result);

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

		[Fact]
		public void Cuid2_Equality()
		{
			var c1 = new Cuid2(10);
			var c2 = new Cuid2(10);

			Assert.False(c1.Equals(c2));
			Assert.False(c1.Equals((object) c2));

			Assert.False(c1 == c2);
			Assert.True(c1 != c2);

			Assert.False(c1.GetHashCode() == c2.GetHashCode());
		}
	}
}
