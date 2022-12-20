# cuid.net

![GitHub](https://img.shields.io/github/license/xaevik/cuid.net?logo=github&style=flat) ![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/xaevik/cuid.net/ci.yaml?branch=main&logo=github&style=flat) [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=cuid.net&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=cuid.net) [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=cuid.net&metric=coverage)](https://sonarcloud.io/summary/new_code?id=cuid.net) 

A fast modern .NET implementation of collision-resistant ids. You can read more about CUIDs from the [official project website](https://usecuid.org/).

## Installation

As of right now, the library is only available as a `prerelease` package until such time that the implementation is deemed stable. To install simply run:

`dotnet package add cuid.net --prerelease` 

## Usage

cuid.net provides the CUID implementation as a `readonly struct` similar to `guid`. 

### Example usage

**Standard Instantiation**

```csharp
using Xaevik.Cuid;

Cuid cuid = new();

Console.WriteLine(cuid); // clbvi4441000007ld63liebkf
```

**Instantiation from string**

```csharp
using Xaevik.Cuid;

Cuid cuid = new("clbvi4441000007ld63liebkf");
```

**String parsing**

```csharp
using Xaevik.Cuid;

// explicit parsing
Cuid cuid = Cuid.Parse("clbvi4441000007ld63liebkf");

// implicit parsing
bool success = Cuid.TryParse("clbvi4441000007ld63liebkf", out Cuid cuid);
```

## Limitations

As of right now (2022-12-19), `Cuid` does not currently implement the `[Serializable]` so serialization is not guaranteed. This will eventually be resolved in a future release.



