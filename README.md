# cuid.net

![GitHub](https://img.shields.io/github/license/xaevik/cuid.net?logo=github&style=flat) [![Continuous Integration](https://github.com/xaevik/cuid.net/actions/workflows/ci.yaml/badge.svg)](https://github.com/xaevik/cuid.net/actions/workflows/ci.yaml) [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=cuid.net&metric=alert_status)](https://sonarcloud.io/summary/overall?id=cuid.net) [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=cuid.net&metric=coverage)](https://sonarcloud.io/summary/overall?id=cuid.net) 

[![Nuget](https://img.shields.io/nuget/v/cuid.net)](https://www.nuget.org/packages/cuid.net/) ![Nuget](https://img.shields.io/nuget/dt/cuid.net)

A fast lightweight .NET implementation of collision-resistant ids. You can read more about CUIDs from the [official project website](https://usecuid.org/).

## Usage

cuid.net supports several methods of creating a `Cuid` structure both as a new value or from an existing CUID: 

```csharp
using Xaevik.Cuid;

// new
Cuid cuid = Cuid.NewCuid();
Console.WriteLine(cuid); // clbvi4441000007ld63liebkf

// constructor
Cuid cuid = new("clbvi4441000007ld63liebkf");

// explicit parsing
Cuid cuid = Cuid.Parse("clbvi4441000007ld63liebkf");

// implicit parsing
bool success = Cuid.TryParse("clbvi4441000007ld63liebkf", out Cuid cuid);
```

`Cuid` fully implements `IComparable`, `IComparable<T>`, and `IEquatable<T>` along with including `Cuid.Empty` for comparing against empty or uninitialized `Cuid` objects.

### Serialization

Built-in support is also available for serializing `Cuid` either with `System.Text.Json` or `XmlSerializer`.  

**JSON**
```csharp
using Xaevik.Cuid;

// serialize
Cuid cuid = Cuid.NewCuid();
string json = JsonSerializer.Serialize(cuid);

Console.WriteLine(json); // "clbvi4441000007ld63liebkf"

// deserialize
Cuid cuid = JsonSerializer.Deserialize<Cuid>("\"clbvi4441000007ld63liebkf\"");
```

**XML**
```csharp
using Xaevik.Cuid;

// serialize
Cuid cuid = Cuid.NewCuid();

XmlSerializer serializer = new(typeof(Cuid));
XmlWriterSettings settings = new() { Indent = false };

using (StringWriter sw = new())
{
    using XmlWriter xw = XmlWriter.Create(stringWriter, settings);

    serializer.Serialize(xw, cuid);

    Console.WriteLine(sw); // <?xml version="1.0" encoding="utf-16"?><cuid>clbvi4441000007ld63liebkf</cuid>
}

// ====================================================================================================

// deserialize

string xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><cuid>clbvi4441000007ld63liebkf</cuid>";
using (TextReader sr = new StringReader(xml))
{
    using XmlReader xr = XmlReader.Create(sr);
    
    Cuid cuid = (Cuid) serializer.Deserialize(xr);
}
```
## Performance

cuid.net was written to be as fast as possible (though there is still room for improvement). The baseline is measured against that of `Guid`.

``` ini
BenchmarkDotNet=v0.13.2, OS=Windows 11 (10.0.22621.963)
Intel Core i7-8700 CPU 3.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK=6.0.404
  [Host]   : .NET 6.0.12 (6.0.1222.56807), X64 RyuJIT AVX2
  .NET 6.0 : .NET 6.0.12 (6.0.1222.56807), X64 RyuJIT AVX2

Job=.NET 6.0  Runtime=.NET 6.0  
```
| Method        | Categories       |      Mean |    Error |   StdDev | Ratio | RatioSD |
|---------------|------------------|----------:|---------:|---------:|------:|--------:|
| Cuid_NewCuid  | New()            | 122.95 ms | 0.461 ms | 0.408 ms |  1.90 |    0.02 |
| Guid_NewGuid  | New()            |  64.61 ms | 0.575 ms | 0.510 ms |  1.00 |    0.00 |
|               |                  |           |          |          |       |         |
| Cuid_ToString | New()+ToString() | 513.62 ms | 4.347 ms | 4.066 ms |  2.69 |    0.03 |
| Guid_ToString | New()+ToString() | 190.67 ms | 1.326 ms | 1.240 ms |  1.00 |    0.00 |

**Note:** Results are based on the creation of 1 million objects.

