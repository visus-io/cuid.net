# cuid.net

[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/visus-io/cuid.net/ci.yml?style=for-the-badge&logo=github)](https://github.com/visus-io/cuid.net/actions/workflows/ci.yml)
[![Sonar Quality Gate](https://img.shields.io/sonar/quality_gate/visus%3Acuid.net?server=https%3A%2F%2Fsonarcloud.io&style=for-the-badge&logo=sonar&logoColor=red)](https://sonarcloud.io/summary/overall?id=visus%3Acuid.net)
[![Sonar Coverage](https://img.shields.io/sonar/coverage/visus%3Acuid.net?server=https%3A%2F%2Fsonarcloud.io&style=for-the-badge&logo=sonar&logoColor=red)](https://sonarcloud.io/summary/overall?id=visus%3Acuid.net)

[![Nuget](https://img.shields.io/nuget/v/cuid.net?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/cuid.net)
![Downloads](https://img.shields.io/nuget/dt/cuid.net?style=for-the-badge&logo=nuget)
![Static Badge](https://img.shields.io/badge/license-mit-green?style=for-the-badge)

cuid.net is a .NET library. It generates collision-resistant unique identifiers (CUIDs). Use CUIDs in distributed systems. CUIDs are an alternative to GUIDs. CUIDs are more readable than GUIDs. Some CUIDs are sortable. CUIDs have better security characteristics than GUIDs.

For more information about CUIDs, go to the official projects: [CUID](https://github.com/paralleldrive/cuid) and [CUID2](https://github.com/paralleldrive/cuid2).

A command-line tool, [cuidgen](https://github.com/visus-io/cuidgen/), is also available. Use cuidgen to generate CUIDs in scripts.

<details>
<summary>Table of Contents</summary>

- [Features](#features)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [CUIDv2 (Recommended)](#cuidv2-recommended)
  - [Features](#cuidv2-features)
  - [Structure](#cuidv2-structure)
  - [Usage](#cuidv2-usage)
  - [Validation](#cuidv2-validation)
- [CUIDv1 (Deprecated)](#cuidv1-deprecated)
  - [Security Considerations](#security-considerations)
  - [Structure](#cuidv1-structure)
  - [Usage](#cuidv1-usage)
  - [Serialization](#cuidv1-serialization)
- [Framework Support](#framework-support)
  - [Platform-Specific Features](#platform-specific-features)
- [Performance Considerations](#performance-considerations)
  - [CUIDv2 Performance](#cuidv2-performance)
  - [CUIDv1 Performance](#cuidv1-performance)

</details>

## Features

- **Two implementations**: CUIDv1 (deprecated) and CUIDv2 (recommended).
- **Collision resistance**: The library generates cryptographically strong identifiers. Collisions almost never happen.
- **Horizontal scalability**: Generate identifiers on many machines at the same time. The machines do not need to coordinate.
- **URL-safe format**: The library uses base-36 encoding (0-9, a-z). The identifiers are clean and readable.
- **Configurable length**: CUIDv2 supports lengths from 4 to 32 characters. The default length is 24 characters.
- **Type safety**: The identifiers are immutable structures. They have full type safety and equality support.
- **Framework support**: The library targets .NET Standard 2.0, .NET Standard 2.1, .NET 8.0, and .NET 10.0.
- **Serialization**: The library has built-in JSON and XML serialization support for CUIDv1.
- **Trimming support**: The library supports trimming on .NET 8 and later.
- **Compiler warnings**: Use of CUIDv1 emits diagnostic `VISLIB0001`. This warning tells you to migrate to CUIDv2.

## Installation

Install cuid.net with the NuGet Package Manager:

```shell
dotnet add package cuid.net
```

Or install cuid.net with the Package Manager Console:

```shell
Install-Package cuid.net
```

### Requirements

**Supported platforms:**
- .NET 8.0 and later
- .NET Core 2.0 and later
- .NET Framework 4.6.1 and later
- Mono 5.4 and later
- Xamarin.iOS 10.14 and later
- Xamarin.Mac 3.8 and later
- Xamarin.Android 8.0 and later
- Universal Windows Platform 10.0.16299 and later

**Dependencies:**

NuGet installs the following runtime dependencies with the library.

*All platforms:*
- **BouncyCastle.Cryptography** — provides SHA-3 hashing for CUIDv2.
- **CommunityToolkit.Diagnostics** — provides guard clauses and validation.

*.NET Standard 2.0/2.1 only:*
- **Microsoft.Bcl.HashCode** — provides `HashCode` support for older frameworks.
- **PolySharp** — provides compile-time language polyfills for older frameworks.
- **System.Text.Json** — provides JSON serialization support for CUIDv1.

## Quick Start

```csharp
using Visus.Cuid;

// CUIDv2 (Recommended)
Cuid2 id = new Cuid2();
Console.WriteLine(id); // o2tm13zgjtaur83duiakvgiq

// CUIDv2 with custom length
Cuid2 shortId = new Cuid2(10);
Console.WriteLine(shortId); // rolaz6ek3u

// CUIDv1 (Deprecated - emits compiler warning VISLIB0001)
Cuid legacyId = Cuid.NewCuid();
Console.WriteLine(legacyId); // cmjj07yka00016337xrs9mj24
```

## CUIDv2 (Recommended)

> [!NOTE]
> Use `Cuid2` for all new projects. `Cuid2` generates cryptographically strong identifiers. Use these identifiers in security-sensitive contexts.

`Cuid2` is an immutable structure. It generates collision-resistant identifiers with SHA-3 hashing. `Cuid2` puts security first. CUIDv1 does not. `Cuid2` does not reveal the generation time or location of an identifier.

### CUIDv2 Features

- **Cryptographically strong**: `Cuid2` uses SHA-3 512-bit hashing through BouncyCastle.
- **No information disclosure**: You cannot derive when or where the library created the identifier.
- **Variable length**: `Cuid2` supports identifiers from 4 to 32 characters. The default length is 24 characters.
- **Not sortable**: `Cuid2` does not implement `IComparable`. This design improves security.
- **Equality support**: `Cuid2` implements `IEquatable<Cuid2>` for comparisons.
- **No built-in serialization**: Use `.ToString()` to get the string representation.

### CUIDv2 Structure

A CUIDv2 value has a variable-length structure. The structure has no fixed pattern. The library generates a CUIDv2 value with this process:

1. **Input components**:
   - **Prefix**: One random character (a-z).
   - **Timestamp**: The Unix timestamp, in ticks.
   - **Counter**: A session counter. The library initializes the counter with a cryptographic RNG, then increments it.
   - **Fingerprint**: Host-specific data. This data includes the hostname, the process ID, and environment variables.
   - **Random data**: Cryptographically strong random bytes. The length matches the requested identifier length.

2. **Hash computation**: The library hashes all components except the prefix with SHA-3 512-bit.

3. **Encoding**: The library encodes the hash in base-36. It truncates the result to the requested length minus 1. It prepends the random prefix to the result.

**Example:**
```
o2tm13zgjtaur83duiakvgiq
```

### CUIDv2 Usage

#### Basic Generation

```csharp
using Visus.Cuid;

// Default length (24 characters)
Cuid2 id = new Cuid2();
Console.WriteLine(id); // o2tm13zgjtaur83duiakvgiq

// Custom length (4-32 characters)
Cuid2 shortId = new Cuid2(10);
Console.WriteLine(shortId); // v1888wvo9i

Cuid2 longId = new Cuid2(32);
Console.WriteLine(longId); // zkx5dng1v8r0dg36id29uoqt1dsndmvb
```

#### String Conversion

```csharp
using Visus.Cuid;

Cuid2 id = new Cuid2();

// Explicit conversion
string idString = id.ToString();

// Implicit conversion
string implicit = id;
```

#### Equality Comparison

```csharp
using Visus.Cuid;

Cuid2 id1 = new Cuid2();
Cuid2 id2 = new Cuid2();
Cuid2 id3 = id1;

// Equality operators
bool areEqual = id1 == id3;     // true
bool notEqual = id1 != id2;     // true

// Equals method
bool equals = id1.Equals(id3);  // true

// GetHashCode support for collections
HashSet<Cuid2> uniqueIds = new HashSet<Cuid2> { id1, id2, id3 };
Console.WriteLine(uniqueIds.Count); // 2
```

#### Empty/Default Values

```csharp
using Visus.Cuid;

// Default value
Cuid2 defaultId = default;
Cuid2 emptyId = new Cuid2(0); // Creates empty instance

// Check for empty
bool isEmpty = string.IsNullOrEmpty(defaultId.ToString());
```

> [!IMPORTANT]
> **Technical details:**
> - The fingerprint size changes with the hostname length and the environment variables.
> - The random data size matches the requested identifier length.
> - The timestamp precision is in ticks (100-nanosecond intervals). It is not in milliseconds.
> - SHA-3 is the NIST-standardized algorithm (FIPS 202). It is not the original Keccak submission.

### CUIDv2 Validation

`Cuid2` validates the length during construction:

```csharp
using Visus.Cuid;

try
{
    // Invalid: length must be between 4 and 32
    Cuid2 tooShort = new Cuid2(3);  // throws ArgumentOutOfRangeException
    Cuid2 tooLong = new Cuid2(33);  // throws ArgumentOutOfRangeException
}
catch (ArgumentOutOfRangeException ex)
{
    Console.WriteLine($"Invalid length: {ex.Message}");
}

// Valid lengths
Cuid2 valid1 = new Cuid2(4);   // Minimum
Cuid2 valid2 = new Cuid2(24);  // Default
Cuid2 valid3 = new Cuid2(32);  // Maximum
```

## CUIDv1 (Deprecated)

> [!CAUTION]
> CUIDv1 is deprecated for security reasons. Migrate to `Cuid2` for all new projects and for security-sensitive applications.

> [!WARNING]
> An observer can often work out when and where the library created a CUIDv1 value. Do not use CUIDv1 in security-sensitive contexts.

> [!NOTE]
> Use of CUIDv1 emits the compiler warning `VISLIB0001`. This warning tells you to migrate to CUIDv2.

`Cuid` is an immutable structure. `Cuid` provides a sortable, string-safe alternative to `Guid`. Use `Cuid` for horizontal scaling and binary search. Use `Cuid` when you need chronological order. Use `Cuid` only in contexts where security is not a concern.

### Security Considerations

Do not use CUIDv1 in these cases:
- Security or privacy is a concern.
- You must hide the generation time or location.
- The application exposes identifiers in URLs or public APIs.
- Compliance rules require non-predictable identifiers.

You may use CUIDv1 in these cases:
- The identifiers are internal, in a controlled environment.
- You need compatibility with a legacy system.
- Sortability matters more than security.

### CUIDv1 Structure

A CUIDv1 value has several data points. The library base-36 encodes the value to a fixed length of 25 characters.

**Example:**
```
cmjj07yka00016337xrs9mj24
```

| Segment    | Length | Source                                     |
|------------|--------|--------------------------------------------|
| `c`        | 1      | CUIDv1 identifier prefix                   |
| `mjj07yka` | 8      | Unix timestamp in milliseconds (base-36)   |
| `0001`     | 4      | Session counter (base-36)                  |
| `6337`     | 4      | Client fingerprint (process ID + hostname) |
| `xrs9mj24` | 8      | Random data (base-36)                      |

**Total length:** 25 characters.

### CUIDv1 Usage

#### Generation

```csharp
using Visus.Cuid;

// Static factory method (recommended)
Cuid id = Cuid.NewCuid();
Console.WriteLine(id); // cmjj07yka00016337xrs9mj24

// Empty/default value
Cuid empty = Cuid.Empty;
```

#### Parsing

```csharp
using Visus.Cuid;

// Constructor parsing
Cuid id1 = new Cuid("cmjj07yka00016337xrs9mj24");

// Explicit parsing
Cuid id2 = Cuid.Parse("cmjj07yka00016337xrs9mj24");

// Try-parse pattern
if (Cuid.TryParse("cmjj07yka00016337xrs9mj24", out Cuid id3))
{
    Console.WriteLine($"Parsed: {id3}");
}
else
{
    Console.WriteLine("Invalid CUID format");
}
```

#### Comparison and Sorting

`Cuid` implements `IComparable`, `IComparable<Cuid>`, and `IEquatable<Cuid>`:

```csharp
using Visus.Cuid;

Cuid id1 = Cuid.NewCuid();
Thread.Sleep(10); // Ensure different timestamp
Cuid id2 = Cuid.NewCuid();

// Comparison operators
bool isLess = id1 < id2;        // true (earlier timestamp)
bool isGreater = id2 > id1;     // true
bool areEqual = id1 == id1;     // true

// CompareTo method
int comparison = id1.CompareTo(id2); // -1 (id1 is earlier)

// Sorting
List<Cuid> ids = new List<Cuid> { id2, id1 };
ids.Sort(); // Chronological order: [id1, id2]

// Empty comparison
bool isEmpty = id1 == Cuid.Empty; // false
```

#### Equality

```csharp
using Visus.Cuid;

Cuid id1 = Cuid.Parse("cmjj07yka00016337xrs9mj24");
Cuid id2 = Cuid.Parse("cmjj07yka00016337xrs9mj24");
Cuid id3 = Cuid.NewCuid();

// Equality operators
bool equal = id1 == id2;        // true
bool notEqual = id1 != id3;     // true

// Equals method
bool equals = id1.Equals(id2);  // true

// Hash code support
Dictionary<Cuid, string> lookup = new Dictionary<Cuid, string>
{
    { id1, "First" },
    { id3, "Second" }
};
```

### CUIDv1 Serialization

CUIDv1 has built-in serialization support for JSON and XML.

#### JSON Serialization

```csharp
using System.Text.Json;
using Visus.Cuid;

// Serialize
Cuid id = Cuid.NewCuid();
string json = JsonSerializer.Serialize(id);
Console.WriteLine(json); // "cmjj07yka00016337xrs9mj24"

// Deserialize
Cuid deserialized = JsonSerializer.Deserialize<Cuid>("\"cmjj07yka00016337xrs9mj24\"");

// In objects
public class Document
{
    public Cuid Id { get; set; }
    public string Content { get; set; }
}

Document doc = new Document
{
    Id = Cuid.NewCuid(),
    Content = "Example"
};
string docJson = JsonSerializer.Serialize(doc);
// {"Id":"cmjj07yka00016337xrs9mj24","Content":"Example"}
```

#### XML Serialization

```csharp
using System.Xml;
using System.Xml.Serialization;
using Visus.Cuid;

// Serialize
Cuid id = Cuid.NewCuid();
XmlSerializer serializer = new XmlSerializer(typeof(Cuid));
XmlWriterSettings settings = new XmlWriterSettings { Indent = false };

using (StringWriter sw = new StringWriter())
using (XmlWriter xw = XmlWriter.Create(sw, settings))
{
    serializer.Serialize(xw, id);
    Console.WriteLine(sw.ToString());
    // <?xml version="1.0" encoding="utf-16"?><cuid>cmjj07yka00016337xrs9mj24</cuid>
}

// Deserialize
string xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><cuid>cmjj07yka00016337xrs9mj24</cuid>";
using (StringReader sr = new StringReader(xml))
using (XmlReader xr = XmlReader.Create(sr))
{
    Cuid deserialized = (Cuid)serializer.Deserialize(xr);
}
```

## Framework Support

cuid.net targets multiple frameworks for broad compatibility:

| Target Framework | Version                          |
|-------------------|-----------------------------------|
| .NET Standard      | 2.0, 2.1                          |
| .NET                | 8.0, 10.0                         |
| .NET Framework      | 4.6.1 and later (through .NET Standard 2.0) |

> [!NOTE]
> .NET Framework 4.6.1 is the minimum supported version. For best .NET Standard 2.0 compatibility, use .NET Framework 4.7.2 or later.

The test suite targets .NET Framework 4.8, .NET 8.0, and .NET 10.0. All three targets run in CI.

### Platform-Specific Features

**C# language features:**
- The library uses C# 14 language features.
- The library uses PolySharp to support these features on older frameworks.
- The library uses conditional compilation for framework-specific APIs.

**Trimming support:**
- The library sets `IsTrimmable` to `true` on the .NET 8.0 and .NET 10.0 targets.
- Trimming reduces the deployment size of self-contained applications.

See [Installation → Dependencies](#installation) for the full dependency list.

## Performance Considerations

The library measures `Cuid2` and `Cuid` performance with BenchmarkDotNet. The `benchmarks/cuid.net.benchmarks` project holds the benchmark code. Run the benchmarks with this command:

```shell
dotnet run -c Release --project benchmarks/cuid.net.benchmarks/cuid.net.benchmarks.csproj -- --filter '*'
```

The tables below come from this environment: BenchmarkDotNet v0.15.8, macOS Tahoe 26.6.2, Apple M2 Pro, .NET SDK 10.0.401, .NET 10.0.12 (Arm64 RyuJIT). Each `Guid` row is the baseline for its Ratio column. Your numbers will vary by platform and .NET version.

### CUIDv2 Performance

| Method                        |          Mean | Ratio | Allocated |
|--------------------------------|--------------:|------:|----------:|
| `new Cuid2()` (default length) |      21.63 μs | 91.54 |     416 B |
| `new Cuid2(32)` (max length)   |      21.55 μs | 91.20 |     456 B |
| `Guid.NewGuid()` (baseline)     |     236.31 ns |  1.00 |         — |
| `Guid.ToString()` (baseline)    |       5.98 ns |  1.00 |      96 B |
| `Cuid2.ToString()`              |       0.01 ns |  0.00 |         — |

Cuid2 construction costs more than `Guid.NewGuid()`. The SHA-3 512-bit hash causes most of this cost. `Cuid2.ToString()` returns a cached string. It costs close to nothing.

**Optimization tips:**
- Cache an identifier value instead of creating a new one for the same entity.
- Use a shorter length (4 to 10 characters) outside security-sensitive contexts.

### CUIDv1 Performance

| Method                     |          Mean | Ratio | Allocated |
|-----------------------------|--------------:|------:|----------:|
| `Cuid.NewCuid()`             |     176.71 ns |  0.74 |     250 B |
| `Guid.NewGuid()` (baseline)  |     237.81 ns |  1.00 |         — |
| `Guid.ToString()` (baseline) |       5.94 ns |  1.00 |      96 B |
| `Cuid.ToString()`            |       0.01 ns |  0.00 |         — |

CUIDv1 construction costs less than `Guid.NewGuid()`. CUIDv1 skips the SHA-3 hash that CUIDv2 uses. Use `Cuid2` in new code. Use `Cuid` only when you have a specific reason to.
