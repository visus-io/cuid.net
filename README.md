# cuid.net

[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/visus-io/cuid.net/ci.yml?style=for-the-badge&logo=github)](https://github.com/visus-io/cuid.net/actions/workflows/ci.yaml)
[![Sonar Quality Gate](https://img.shields.io/sonar/quality_gate/visus%3Acuid.net?server=https%3A%2F%2Fsonarcloud.io&style=for-the-badge&logo=sonarcloud&logoColor=white)](https://sonarcloud.io/summary/overall?id=visus%3Acuid.net)
[![Sonar Coverage](https://img.shields.io/sonar/coverage/visus%3Acuid.net?server=https%3A%2F%2Fsonarcloud.io&style=for-the-badge&logo=sonarcloud&logoColor=white)](https://sonarcloud.io/summary/overall?id=visus%3Acuid.net)

[![Nuget](https://img.shields.io/nuget/v/cuid.net?style=for-the-badge&logo=nuget&label=stable)](https://www.nuget.org/packages/cuid.net)
[![Nuget](https://img.shields.io/nuget/vpre/cuid.net?style=for-the-badge&logo=nuget&label=dev)](https://www.nuget.org/packages/cuid.net)
![Downloads](https://img.shields.io/nuget/dt/cuid.net?style=for-the-badge&logo=nuget)
![GitHub](https://img.shields.io/github/license/visus-io/cuid.net?style=for-the-badge)

A .NET implementation of collision-resistant unique identifiers (CUIDs) designed for horizontal scalability and security in distributed environments. This library provides robust alternatives to traditional GUIDs with improved readability, sortability, and security characteristics.

For more information about CUIDs, visit the official projects: [CUID](https://github.com/paralleldrive/cuid) and [CUID2](https://github.com/paralleldrive/cuid2).

A command-line utility, [cuidgen](https://github.com/visus-io/cuidgen/), is also available for leveraging CUIDs in scripting environments.

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

- **Two Implementations**: CUIDv1 (deprecated) and CUIDv2 (recommended)
- **Collision Resistance**: Cryptographically strong identifiers with negligible collision probability
- **Horizontal Scalability**: Safe for distributed generation across multiple machines without coordination
- **URL-Safe Format**: Base-36 encoding (0-9, a-z) for clean, readable identifiers
- **Configurable Length**: CUIDv2 supports 4-32 character lengths (default: 24)
- **Type Safety**: Immutable structures with full type safety and equality support
- **Framework Support**: Targets .NET Standard 2.0, .NET Standard 2.1, .NET 8.0, and .NET 10.0
- **Serialization**: Built-in JSON and XML serialization support for CUIDv1
- **Trimming Support**: Optimized for .NET trimming in .NET 8+
- **Compiler Warnings**: CUIDv1 usage emits diagnostic `VISLIB0001` to encourage migration

## Installation

Install cuid.net via NuGet Package Manager:

```shell
dotnet add package cuid.net
```

Or via the Package Manager Console:

```shell
Install-Package cuid.net
```

### Requirements

**Supported Platforms:**
- .NET 8.0+
- .NET Core 2.0+
- .NET Framework 4.6.1+
- Mono 5.4+
- Xamarin.iOS 10.14+
- Xamarin.Mac 3.8+
- Xamarin.Android 8.0+
- Universal Windows Platform 10.0.16299+

**Dependencies:**

The library automatically includes the following runtime dependencies via NuGet:

*All platforms:*
- **BouncyCastle.Cryptography** - SHA-3 hashing for CUIDv2
- **CommunityToolkit.Diagnostics** - Guard clauses and validation

*.NET Standard 2.0/2.1 only:*
- **Microsoft.Bcl.HashCode** - HashCode support for older frameworks
- **System.Text.Json** - JSON serialization support for CUIDv1

> [!NOTE]
> While .NET Framework 4.6.1 is the minimum supported version, .NET Framework 4.7.2+ is recommended for optimal compatibility.

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
> `Cuid2` is the recommended implementation for all new projects. It provides cryptographically strong identifiers suitable for security-sensitive contexts.

`Cuid2` is an immutable structure that generates collision-resistant identifiers using SHA-3 hashing. Unlike CUIDv1, it is designed with security as a primary concern and does not leak information about generation time or location.

### CUIDv2 Features

- **Cryptographically Strong**: Uses SHA-3 512-bit hashing via BouncyCastle
- **No Information Leakage**: Cannot derive when or where the identifier was created
- **Variable Length**: Supports 4-32 character identifiers (default: 24)
- **Not Sortable**: Intentionally does not implement `IComparable` for security
- **Equality Support**: Implements `IEquatable<Cuid2>` for comparisons
- **No Built-in Serialization**: Use `.ToString()` for string representation

### CUIDv2 Structure

CUIDv2 values use a variable-length structure with no predefined pattern. The generation process:

1. **Input Components**:
   - **Prefix**: Single random character (a-z)
   - **Timestamp**: Unix timestamp in ticks
   - **Counter**: Session counter (initialized with cryptographic RNG, then incremented)
   - **Fingerprint**: Host-specific data (hostname + process ID + environment variables)
   - **Random Data**: Cryptographically strong random bytes (length matches requested identifier length)

2. **Hash Computation**: All components except the prefix are hashed using SHA-3 512-bit

3. **Encoding**: Hash is base-36 encoded, then truncated to requested length minus 1, with the random prefix prepended

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
> **Technical Details:**
> - The fingerprint size is variable (depends on hostname length and environment variables)
> - The random data size matches the requested identifier length
> - The timestamp precision is in ticks (100-nanosecond intervals), not milliseconds
> - SHA-3 is the NIST-standardized algorithm (FIPS 202), not the original Keccak submission

### CUIDv2 Validation

CUIDv2 validates length during construction:

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
> CUIDv1 has been deprecated for security reasons. Migrate to `Cuid2` for all new projects and security-sensitive applications.

> [!WARNING]
> It is possible to derive with a degree of certainty when and where a CUIDv1 was created, making it unsuitable for security-sensitive contexts.

> [!NOTE]
> Usage of CUIDv1 will emit the compiler warning `VISLIB0001` to encourage migration to CUIDv2.

`Cuid` is an immutable structure designed for horizontal scaling and binary searches. It provides a sortable, "string-safe" alternative to `Guid` for scenarios where chronological ordering is needed and security is not a primary concern.

### Security Considerations

CUIDv1 should be avoided when:
- Security or privacy is a concern
- Hiding generation time/location is important
- Identifiers are exposed in URLs or public APIs
- Compliance requires non-predictable identifiers

CUIDv1 may be acceptable for:
- Internal identifiers in controlled environments
- Legacy system compatibility
- Applications where sortability is critical and security is not

### CUIDv1 Structure

CUIDv1 values are composed of several data points, base-36 encoded to a fixed 25-character length:

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

**Total Length:** 25 characters

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

CUIDv1 provides built-in serialization support for JSON and XML.

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

| Target Framework | Version                        |
|------------------|--------------------------------|
| .NET Standard    | 2.0, 2.1                       |
| .NET             | 8.0, 10.0                      |
| .NET Framework   | 4.6.1+ (via .NET Standard 2.0) |

> [!NOTE]
> While .NET Framework 4.6.1 is the minimum supported version, .NET Framework 4.7.2 or later is recommended for optimal .NET Standard 2.0 compatibility.

### Platform-Specific Features

**C# Language Features:**
- Uses C# 14 language features with PolySharp for older frameworks
- Conditional compilation for framework-specific APIs

**Trimming Support:**
- Optimized for .NET 8+ trimming (`IsTrimmable=true`)
- Reduced deployment size for self-contained applications

**Dependencies:**

*All platforms:*
- BouncyCastle.Cryptography
- CommunityToolkit.Diagnostics

*.NET Standard 2.0/2.1 additional packages:*
- Microsoft.Bcl.HashCode
- PolySharp (compile-time only)
- System.Text.Json

## Performance Considerations

### CUIDv2 Performance

CUIDv2 uses cryptographic operations (SHA-3 512-bit) for security, which has performance implications:

**Generation Speed:**
- ~100,000-500,000 identifiers per second (single-threaded)
- Performance varies by platform and .NET version
- Suitable for most application scenarios

**Memory Allocation:**
- Minimal heap allocations (struct type)
- Stackalloc usage on .NET 8+ for better performance
- Suitable for high-throughput scenarios

**Optimization Tips:**
- Generate identifiers asynchronously in batches if needed
- Consider caching if generating many identifiers rapidly
- Use shorter lengths (4-10 chars) for non-security contexts

### CUIDv1 Performance

CUIDv1 is faster than CUIDv2 due to simpler generation:

**Generation Speed:**
- ~1,000,000+ identifiers per second (single-threaded)
- Minimal CPU overhead

**Trade-offs:**
- Faster generation vs. security (CUIDv2 is recommended)
