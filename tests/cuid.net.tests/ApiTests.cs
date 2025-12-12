namespace Visus.Cuid.Tests;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using PublicApiGenerator;

[ExcludeFromCodeCoverage]
internal sealed class ApiTests
{
    [Test]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public async Task PublicApi_HasNoBreakingChanges_Async()
    {
        string api = typeof(Cuid2).Assembly.GeneratePublicApi(new ApiGeneratorOptions
        {
            ExcludeAttributes =
            [
                "System.ObsoleteAttribute",
                "System.Reflection.AssemblyMetadataAttribute",
                "System.Runtime.Versioning.TargetFrameworkAttribute",
            ],
        });

        await Verify(api).ConfigureAwait(false);
    }
}
