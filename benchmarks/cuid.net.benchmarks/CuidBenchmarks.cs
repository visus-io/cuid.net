#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable VISLIB0001 // Public API usage is allowed in benchmarks

namespace Visus.Cuid.Benchmarks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class CuidBenchmarks
{
    private Guid _guid;

    private Cuid _instance;

    [Benchmark]
    [BenchmarkCategory("Construct")]
    public Cuid Construct()
    {
        return Cuid.NewCuid();
    }

    [GlobalSetup]
    public void Setup()
    {
        _instance = Cuid.NewCuid();
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

#pragma warning restore VISLIB0001
