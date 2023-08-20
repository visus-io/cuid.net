namespace Visus.Cuid.Tests;

using System.Diagnostics.CodeAnalysis;
using Abstractions;
using Moq;

[ExcludeFromCodeCoverage]
public class FingerprintFacts
{
	[Fact]
	public void Fingerprint_Generate_Version_One()
	{
		var fingerprint = new Fingerprint();

		var value = fingerprint.Generate(FingerprintVersion.One);
		
		Assert.NotEmpty(value);
	}
	
	[Fact]
	public void Fingerprint_Generate_Version_Two()
	{
		var fingerprint = new Fingerprint();

		var value = fingerprint.Generate();
		
		Assert.NotEmpty(value);
	}

	[Fact]
	public void Fingerprint_Generate_Version_One_Fallback()
	{
		var environment = new Mock<IEnvironment>();
		environment.Setup(s => s.MachineName)
			.Returns(string.Empty);
		
		var fingerprint = new Fingerprint(environment.Object);

		var value = fingerprint.Generate(FingerprintVersion.One);
		
		Assert.NotEmpty(value);
	}
	
	[Fact]
	public void Fingerprint_Generate_Version_Two_Fallback()
	{
		var environment = new Mock<IEnvironment>();
		environment.Setup(s => s.MachineName)
			.Returns(string.Empty);
		
		var fingerprint = new Fingerprint(environment.Object);

		var value = fingerprint.Generate();
		
		Assert.NotEmpty(value);
	}
}