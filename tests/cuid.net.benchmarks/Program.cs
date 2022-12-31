namespace Xaevik.Cuid.Benchmarks;

using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

[SimpleJob(RuntimeMoniker.Net60)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
[ExcludeFromCodeCoverage]
public class IdentifierPerformance
{
	// [Benchmark]
	// public void Cuid_Constructor()
	// {
	// 	for ( var i = 0; i < 10000; i++ )
	// 	{
	// 		_ = new Cuid();
	// 	}
	// }

	[Benchmark]
	public void Cuid2_ToString()
	{
		for ( var i = 0; i < 500000; i++ )
		{
			_ = new Cuid2().ToString();
		}
	}
}

[ExcludeFromCodeCoverage]
public static class Program
{
	public static void Main()
	{
		BenchmarkRunner.Run<IdentifierPerformance>();
	}
}