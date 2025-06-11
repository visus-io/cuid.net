namespace Visus.Cuid.Tests
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using PublicApiGenerator;
    using VerifyXunit;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class ApiFacts
    {
        [Fact]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task Cuid_NoBreakingChanges_Async()
        {
            var api = typeof(Cuid2).Assembly.GeneratePublicApi();

            await Verifier.Verify(api).UniqueForTargetFrameworkAndVersion();
        }
    }
}
