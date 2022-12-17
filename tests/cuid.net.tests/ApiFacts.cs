namespace Xaevik.Cuid.Tests;

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
		var api = typeof(Cuid).Assembly.GeneratePublicApi();

		await Verify(api);
	}
}