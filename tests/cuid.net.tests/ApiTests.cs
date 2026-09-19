namespace Visus.Cuid.Tests;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using PublicApiGenerator;

[ExcludeFromCodeCoverage]
internal sealed class ApiTests
{
    // Skipped on net48: the net48 host resolves cuid.net's netstandard2.0 build, and Mono.Cecil
    // (used by PublicApiGenerator) cannot resolve the netstandard facade assembly when hosted by
    // .NET Framework. See https://github.com/jbevain/cecil/issues/901. net8.0/net10.0 already
    // exercise the library's native builds, so API-surface regressions are still caught.
#if !NET48
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
#endif
}
