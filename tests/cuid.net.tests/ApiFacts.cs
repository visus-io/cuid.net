﻿namespace Visus.Cuid.Tests
{
	using System.Diagnostics.CodeAnalysis;
	using System.Runtime.CompilerServices;
	using System.Threading.Tasks;
	using PublicApiGenerator;
	using Xunit;
#if NET8_0_OR_GREATER
	using VerifyXunit;
#endif

	[ExcludeFromCodeCoverage]
	public class ApiFacts
	{
#if NET8_0_OR_GREATER
		[Fact]
		[MethodImpl(MethodImplOptions.NoInlining)]
		public async Task Cuid_NoBreakingChanges_Async()
		{
			var api = typeof(Cuid2).Assembly.GeneratePublicApi(new ApiGeneratorOptions
			{
				ExcludeAttributes = ["System.Runtime.Versioning.TargetFrameworkAttribute", "System.Reflection.AssemblyMetadataAttribute"]
			});

			await Verifier.Verify(api);
		}
#endif
	}
}
