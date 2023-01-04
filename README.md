# cuid.net

![GitHub](https://img.shields.io/github/license/xaevik/cuid.net?logo=github&style=flat) [![Continuous Integration](https://github.com/xaevik/cuid.net/actions/workflows/ci.yaml/badge.svg)](https://github.com/xaevik/cuid.net/actions/workflows/ci.yaml) [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=cuid.net&metric=alert_status)](https://sonarcloud.io/summary/overall?id=cuid.net) [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=cuid.net&metric=coverage)](https://sonarcloud.io/summary/overall?id=cuid.net) [![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=cuid.net&metric=security_rating)](https://sonarcloud.io/summary/overall?id=cuid.net) 

[![Nuget](https://img.shields.io/nuget/v/cuid.net)](https://www.nuget.org/packages/cuid.net/) ![Nuget](https://img.shields.io/nuget/dt/cuid.net)

A .NET implementation of collision-resistant ids. You can read more about CUIDs from the [official project website](https://github.com/paralleldrive/cuid2).

## Table of Contents
- [Getting Started](#getting-started)
- [Implementations](#implementations)
  - [CUIDv1](#CUIDv1)
  - [CUIDv2](#CUIDv2)
- [Performance](#performance)

### Getting Started

You can install cuid.net as a [nuget package](https://www.nuget.org/packages/cuid.net):

```shell
dotnet add package cuid.net 
```

### Implementations

cuid.net supports the construction and use of both CUIDv1 (deprecated) and CUIDv2 instances. 

---

#### CUIDv1
> :exclamation:
> CUIDv1 has been deprecated for security reasons.

Designed and optimized for horizontal scaling and binary searches, `Cuid` is an immutable structure that can be a potential alternative to `Guid` for situations where a clean "string safe" unique and sortable identifier is needed and where security is not of the upmost concern.

> :warning:
> It is fully possible to derive when and where a CUIDv1 has been created. 
> 
> As such, they should not be used in a security sensitive context such as device or user identities. If you need a cryptographically secure identifier, [generate a CUIDv2](#CUIDv2) instead via `Cuid2`.

**Structure**

CUIDv1 values are composed of several data points which are base 36 encoded.

> clbvi4441000007ld63liebkf

| segment  | source                                                         |
|----------|----------------------------------------------------------------|
| c        | CUIDv1 identifier                                              |
| lbvi4441 | unix timestamp (in microseconds)                               |
| 0000     | counter                                                        |
| 07ld     | client fingerprint (host process identifier + system hostname) |
| 63liebkf | random data                                                    |

**Instantiation**

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

**Serialization**

`Cuid` has built-in support for serialization either with `System.Text.Json` or `XmlSerializer`. 

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

----

#### CUIDv2

`Cuid2` is an immutable structure that generates a cryptographically strong identity. `Cuid2` and is recommended for use over `Cuid` where security context is important. The length of the value can also be adjusted to be anywhere from 4 characters to 32 characters in length, the default is 24.

> :memo: `Cuid2` does not implement `IComparable`, `IComparable<T>`, and `IEquatable<T>` nor does it support instantiation from string or serialization.

**Structure**

CUIDv2 values follow a different variable structure length than that of their predecessor. As such, there is no predefined pattern of how they will look once generated. However, with that said, they do use the following data sources:

- An alphabetic (a-z) letter randomly chosen as the prefix
- Unix Timestamp
- 32 bytes of random data from a cryptographically strong source
- Session counter value from a weak random number generator with a unique seed fingerprint
- Host fingerprint composed of 32 bytes of data containing non-confidential information and padded with cryptographically strong random data

The information is then combined and a SHA-3 (SHA512) salted hash is computed and then encoded into a base 36 encoded string.

**Instantiation**

```csharp
using Xaevik.Cuid;

// new (default length of 24)
Cuid2 cuid = new Cuid2();
Console.WriteLine(cuid); // x8kvch3q341xr1wa5ida3ns0

// new (with custom length)
Cuid2 cuid = new Cuid2(10);
Console.WriteLine(cuid); // rolaz6ek3u
```

### Performance

cuid.net was written to be as fast as possible (where possible) with the performance of `Cuid` measured against that of `Guid`. The performance of `Cuid2` will be considerably slower than that of `Cuid` due to the cryptographically strong signatures it generates. The benchmarks presented below are based on the creation of one million objects.

**CUIDv1 (As Compared Against `Guid`)**

``` ini
BenchmarkDotNet=v0.13.2, OS=Windows 11 (10.0.22621.963)
Intel Core i7-10870H CPU 2.20GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.101
  [Host]   : .NET 6.0.12 (6.0.1222.56807), X64 RyuJIT AVX2
  .NET 6.0 : .NET 6.0.12 (6.0.1222.56807), X64 RyuJIT AVX2
  .NET 7.0 : .NET 7.0.1 (7.0.122.56804), X64 RyuJIT AVX2
```
|        Method |      Job |  Runtime |       Categories |      Mean |    Error |   StdDev | Ratio | RatioSD |
|-------------- |--------- |--------- |----------------- |----------:|---------:|---------:|------:|--------:|
|  Cuid_NewCuid | .NET 6.0 | .NET 6.0 |            New() |  56.01 ms | 0.413 ms | 0.387 ms |  0.85 |    0.01 |
|  Guid_NewGuid | .NET 6.0 | .NET 6.0 |            New() |  65.79 ms | 0.932 ms | 0.872 ms |  1.00 |    0.00 |
|  Cuid_NewCuid | .NET 7.0 | .NET 7.0 |            New() |  55.07 ms | 0.571 ms | 0.534 ms |  0.84 |    0.01 |
|  Guid_NewGuid | .NET 7.0 | .NET 7.0 |            New() |  63.63 ms | 0.639 ms | 0.597 ms |  0.97 |    0.01 |
|               |          |          |                  |           |          |          |       |         |
| Cuid_ToString | .NET 6.0 | .NET 6.0 | New()+ToString() | 236.56 ms | 1.086 ms | 0.848 ms |  1.29 |    0.01 |
| Guid_ToString | .NET 6.0 | .NET 6.0 | New()+ToString() | 182.90 ms | 1.432 ms | 1.340 ms |  1.00 |    0.00 |
| Cuid_ToString | .NET 7.0 | .NET 7.0 | New()+ToString() | 236.59 ms | 1.943 ms | 1.817 ms |  1.29 |    0.02 |
| Guid_ToString | .NET 7.0 | .NET 7.0 | New()+ToString() | 174.92 ms | 2.460 ms | 2.301 ms |  0.96 |    0.02 |

**CUIDv2 (As Compared Against `Cuid`)**

``` ini
BenchmarkDotNet=v0.13.2, OS=Windows 11 (10.0.22621.963)
Intel Core i7-10870H CPU 2.20GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.101
  [Host]   : .NET 6.0.12 (6.0.1222.56807), X64 RyuJIT AVX2
  .NET 6.0 : .NET 6.0.12 (6.0.1222.56807), X64 RyuJIT AVX2
  .NET 7.0 : .NET 7.0.1 (7.0.122.56804), X64 RyuJIT AVX2

```
|         Method |      Job |  Runtime |       Categories |         Mean |     Error |    StdDev | Ratio | RatioSD |
|--------------- |--------- |--------- |----------------- |-------------:|----------:|----------:|------:|--------:|
|   Cuid_NewCuid | .NET 6.0 | .NET 6.0 |            New() |     60.49 ms |  1.055 ms |  0.987 ms |  1.00 |    0.00 |
|   Cuid_NewCuid | .NET 7.0 | .NET 7.0 |            New() |     55.23 ms |  0.387 ms |  0.362 ms |  0.91 |    0.02 |
|                |          |          |                  |              |           |           |       |         |
|  Cuid2_NewCuid | .NET 6.0 | .NET 6.0 |            New() |    296.88 ms |  3.033 ms |  2.837 ms |  1.00 |    0.00 |
|  Cuid2_NewCuid | .NET 7.0 | .NET 7.0 |            New() |    289.22 ms |  2.623 ms |  2.453 ms |  0.97 |    0.01 |
|                |          |          |                  |              |           |           |       |         |
|  Cuid_ToString | .NET 6.0 | .NET 6.0 | New()+ToString() |    240.90 ms |  2.213 ms |  1.962 ms |  1.00 |    0.00 |
|  Cuid_ToString | .NET 7.0 | .NET 7.0 | New()+ToString() |    231.53 ms |  2.563 ms |  2.397 ms |  0.96 |    0.01 |
|                |          |          |                  |              |           |           |       |         |
| Cuid2_ToString | .NET 6.0 | .NET 6.0 | New()+ToString() | 11,165.24 ms | 11.778 ms |  9.835 ms |  1.00 |    0.00 |
| Cuid2_ToString | .NET 7.0 | .NET 7.0 | New()+ToString() | 11,684.33 ms | 22.521 ms | 21.066 ms |  1.05 |    0.00 |

