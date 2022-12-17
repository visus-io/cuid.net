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
public class IdentifierPerformance
{
	[Benchmark]
	[BenchmarkCategory("Constructor")]
	public void Cuid_New()
	{
		for ( var i = 0; i < 1000000; i++ )
		{
			_ = new Cuid();
		}
	}

	[Benchmark]
	[BenchmarkCategory("ToString()")]
	public void Cuid_ToString()
	{
		for ( var i = 0; i < 1000000; i++ )
		{
			_ = new Cuid().ToString();
		}
	}

	[Benchmark(Baseline = true)]
	[BenchmarkCategory("Constructor")]
	public void Guid_New()
	{
		for ( var i = 0; i < 1000000; i++ )
		{
			_ = Guid.NewGuid();
		}
	}

	[Benchmark(Baseline = true)]
	[BenchmarkCategory("ToString()")]
	public void Guid_ToString()
	{
		for ( var i = 0; i < 1000000; i++ )
		{
			_ = Guid.NewGuid().ToString();
		}
	}
}

public static class Program
{
	public static void Main()
	{
		BenchmarkRunner.Run<IdentifierPerformance>();
	}
}