namespace Xaevik.Cuid.Benchmarks;

using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

[SimpleJob(RuntimeMoniker.Net60)]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class IdentifierPerformance
{
	[Benchmark(Baseline = true)]
	public void NewGuid()
	{
		for ( int i = 0; i < 1000000; i++ )
		{
			_ = Guid.NewGuid();
		}
	}

	[Benchmark]
	public void NewCuid()
	{
		for ( int i = 0; i < 1000000; i++ )
		{
			_ = new Cuid();
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