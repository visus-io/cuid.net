# cuid.net

[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/visus-io/cuid.net/ci.yml?style=for-the-badge&logo=github)](https://github.com/visus-io/cuid.net/actions/workflows/ci.yaml)
[![Code Quality](https://img.shields.io/codacy/grade/d20b5dbb3a7a4837ae83f2908c85451c?style=for-the-badge&logo=codacy)](https://app.codacy.com/gh/visus-io/cuid.net/dashboard)
[![Coverage](https://img.shields.io/codacy/coverage/d20b5dbb3a7a4837ae83f2908c85451c?style=for-the-badge&logo=codacy)](https://app.codacy.com/gh/visus-io/cuid.net/coverage/dashboard)

[![Nuget](https://img.shields.io/nuget/v/cuid.net?style=for-the-badge&logo=nuget&label=stable)](https://www.nuget.org/packages/cuid.net)
[![Nuget](https://img.shields.io/nuget/vpre/cuid.net?style=for-the-badge&logo=nuget&label=dev)](https://www.nuget.org/packages/cuid.net)
![Downloads](https://img.shields.io/nuget/dt/cuid.net?style=for-the-badge&logo=nuget)
![GitHub](https://img.shields.io/github/license/visus-io/cuid.net?style=for-the-badge)

A .NET implementation of collision-resistant ids. You can read more about CUIDs from
the [official project website](https://github.com/paralleldrive/cuid2).

A command-line utility, [cuidgen](https://github.com/visus-io/cuidgen/), is also available that implements the cuid.net
library for those wishing to leverage CUIDs in scripting environments.

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

> :exclamation: CUIDv1 has been deprecated for security reasons. Efforts should be made towards migrating to `Cuid2`.
>
> :warning: It is possible to derive with a degree of certainty when and where a CUIDv1 has been created.
>
> :memo: Usage of CUIDv1 will emit the compiler warning `VISLIB0001`.

Designed and optimized for horizontal scaling and binary searches, `Cuid` is an immutable structure that can be a
potential alternative to `Guid` for situations where a clean "string safe" unique and sortable identifier is needed and
where security is not of the upmost concern.

##### Structure

CUIDv1 values are composed of several data points which are base 36 encoded.

> clbvi4441000007ld63liebkf

| Segment  | Source                                                         |
|----------|----------------------------------------------------------------|
| c        | CUIDv1 identifier                                              |
| lbvi4441 | Unix timestamp (in milliseconds)                               |
| 0000     | Session counter                                                |
| 07ld     | Client fingerprint (host process identifier + system hostname) |
| 63liebkf | Random data                                                    |

##### Instantiation

```csharp
using Visus.Cuid;

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

`Cuid` fully implements `IComparable`, `IComparable<T>`, and `IEquatable<T>` along with including `Cuid.Empty` for
comparing against empty or uninitialized `Cuid` objects.

##### Serialization

`Cuid` has built-in support for serialization either with `System.Text.Json` or `XmlSerializer`.

##### JSON

```csharp
using Visus.Cuid;

// serialize
Cuid cuid = Cuid.NewCuid();
string json = JsonSerializer.Serialize(cuid);

Console.WriteLine(json); // "clbvi4441000007ld63liebkf"

// deserialize
Cuid cuid = JsonSerializer.Deserialize<Cuid>("\"clbvi4441000007ld63liebkf\"");
```

##### XML

```csharp
using Visus.Cuid;

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

// deserialize

string xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><cuid>clbvi4441000007ld63liebkf</cuid>";
using (TextReader sr = new StringReader(xml))
{
    using XmlReader xr = XmlReader.Create(sr);
    
    Cuid cuid = (Cuid) serializer.Deserialize(xr);
}
```

---

#### CUIDv2

> :memo: `Cuid2` implements `IEquatable<T>` but does not implement `IComparable` or `IComparable<T>`.

`Cuid2` is an immutable structure that generates a cryptographically strong identity. `Cuid2` and is recommended for use
over `Cuid` where security context is important. The length of the value can also be adjusted to be anywhere from 4
characters to 32 characters in length, the default is 24.

##### Structure

CUIDv2 values follow a different variable structure length than that of their predecessor. As such, there is no
predefined pattern of how they will look once generated. However, with that said, they do use the following data
sources:

| Segments                                                                                                      |
|---------------------------------------------------------------------------------------------------------------|
| Single character (a-z) randomly chosen as the prefix                                                          |
| Unix timestamp (in milliseconds)                                                                              |
| 32-byte array containing cryptographically strong random data                                                 |
| Session counter value from a cryptographically weak random number generator                                   |
| 32-byte array containing non-sensitive host information and padding with cryptographically strong random data |

The information is then combined and a SHA-512 (SHA-3 Keccak) salted hash is computed and then encoded into a base 36
string.

##### Instantiation

```csharp
using Visus.Cuid;

// new (default length of 24)
Cuid2 cuid = new Cuid2();
Console.WriteLine(cuid); // x8kvch3q341xr1wa5ida3ns0

// new (with custom length)
Cuid2 cuid = new Cuid2(10);
Console.WriteLine(cuid); // rolaz6ek3u
```
