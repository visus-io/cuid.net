namespace Visus.Cuid.Benchmarks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class Cuid2Benchmarks
{
    private Guid _guid;

    private Cuid2 _instance;

    [Benchmark]
    [BenchmarkCategory("Construct")]
    public Cuid2 Construct_DefaultLength()
    {
        return new Cuid2();
    }

    [Benchmark]
    [BenchmarkCategory("Construct")]
    public Cuid2 Construct_MaxLength()
    {
        return new Cuid2(32);
    }

    [GlobalSetup]
    public void Setup()
    {
        _instance = new Cuid2();
        _guid = Guid.NewGuid();
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Construct")]
    public Guid Guid_NewGuid()
    {
        return Guid.NewGuid();
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("ToString")]
    public string Guid_ToString()
    {
        return _guid.ToString();
    }

    [Benchmark]
    [BenchmarkCategory("ToString")]
    public string ToString_Default()
    {
        return _instance.ToString();
    }
}
