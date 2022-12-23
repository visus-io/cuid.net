namespace Xaevik.Cuid.Benchmarks;

using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

[SimpleJob(RuntimeMoniker.Net60)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
[ExcludeFromCodeCoverage]
public class IdentifierPerformance
{
	[Benchmark]
	[BenchmarkCategory("New()")]
	public void Cuid_NewCuid()
	{
		for ( var i = 0; i < 1000000; i++ )
		{
			_ = Cuid.NewCuid();
		}
	}

	[Benchmark]
	[BenchmarkCategory("New()+ToString()")]
	public void Cuid_ToString()
	{
		for ( var i = 0; i < 1000000; i++ )
		{
			_ = Cuid.NewCuid().ToString();
		}
	}

	[Benchmark(Baseline = true)]
	[BenchmarkCategory("New()")]
	public void Guid_NewGuid()
	{
		for ( var i = 0; i < 1000000; i++ )
		{
			_ = Guid.NewGuid();
		}
	}

	[Benchmark(Baseline = true)]
	[BenchmarkCategory("New()+ToString()")]
	public void Guid_ToString()
	{
		for ( var i = 0; i < 1000000; i++ )
		{
			_ = Guid.NewGuid().ToString();
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