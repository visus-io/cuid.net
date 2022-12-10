namespace Xaevik.Cuid.Tests;

using System.Runtime.CompilerServices;
using PublicApiGenerator;

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