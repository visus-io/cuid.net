namespace Visus.Cuid.Tests;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using PublicApiGenerator;

[ExcludeFromCodeCoverage]
[UsesVerify]
public class ApiFacts
{
	[Fact]
	[MethodImpl(MethodImplOptions.NoInlining)]
	public async Task Cuid_NoBreakingChanges_Async()
	{
		var api = typeof(Cuid2).Assembly.GeneratePublicApi(new ApiGeneratorOptions
		{
			ExcludeAttributes = new[]
			{
				"System.Runtime.Versioning.TargetFrameworkAttribute", "System.Reflection.AssemblyMetadataAttribute"
			}
		});

		await Verify(api);
	}
}
