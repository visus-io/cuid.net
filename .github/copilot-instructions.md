# GitHub Copilot Instructions

This file provides context and guidelines for working with the cuid.net codebase.

## Project Overview

cuid.net is a .NET implementation of collision-resistant unique identifiers (CUIDs). The library provides two implementations:
- **CUIDv1** (`Cuid`) - Deprecated for security reasons, emits compiler warning `VISLIB0001`
- **CUIDv2** (`Cuid2`) - Recommended cryptographically strong identifier

**Target Frameworks**: `netstandard2.0`, `netstandard2.1`, `net8.0`, `net10.0`

## Key Architecture Details

### Cuid (CUIDv1) - `src/cuid.net/Cuid.cs`
- Readonly struct implementing `IComparable`, `IComparable<Cuid>`, `IEquatable<Cuid>`, `IXmlSerializable`
- Marked obsolete with diagnostic ID `VISLIB0001`
- Structure: prefix (c) + timestamp + counter + fingerprint + random data
- Base-36 encoded, 25 characters total
- Supports JSON serialization via `CuidConverter` and XML serialization
- Timestamp uses 10-microsecond precision (ticks / 10000)

### Cuid2 (CUIDv2) - `src/cuid.net/Cuid2.cs`
- Readonly struct implementing `IEquatable<Cuid2>`
- Does NOT implement `IComparable` (not sortable by design for security)
- Does NOT support JSON or XML serialization (use `.ToString()` for string representation)
- Uses SHA-3 512-bit (NIST FIPS 202) via BouncyCastle for cryptographic strength
- Variable length (4-32 characters, default 24)
- Structure: random prefix (a-z) + SHA-3 hash of (timestamp in ticks + counter + fingerprint + random data)
- Timestamp stored in ticks (100-nanosecond intervals), not milliseconds

### Fingerprint - `src/cuid.net/Fingerprint.cs`
- Internal static class generating host-specific fingerprints
- `FingerprintVersion.One` (legacy): hostname + process ID
- `FingerprintVersion.Two` (default): hostname + process ID + environment variables

## Multi-Targeting Strategy

The project uses C# 14 language features across all target frameworks:
- Conditional compilation (`#if NETSTANDARD`, `#if NETSTANDARD2_0`, etc.) for framework-specific implementations
- Key differences handled:
  - Diagnostic IDs on obsolete attributes (NetStandard doesn't support `DiagnosticId` parameter)
  - BigInteger constructors (byte[] vs ReadOnlySpan<byte>)
  - Span operations and string constructors
  - `DateTimeOffset.UnixEpoch` availability
  - `Environment.ProcessId` vs `Process.GetCurrentProcess().Id`
  - `Convert.ToHexString` vs manual hex conversion
- NetStandard targets require additional packages: `Microsoft.Bcl.HashCode`, `PolySharp`, `System.Text.Json`

## Code Style Guidelines

### Obsoletions
- Use centralized obsolete message definitions from `src/cuid.net/Obsoletions.cs`
- Apply conditional compilation for `DiagnosticId` parameter:
  ```csharp
  #if NETSTANDARD
  [Obsolete(Obsoletions.CuidMessage)]
  #else
  [Obsolete(Obsoletions.CuidMessage, DiagnosticId = Obsoletions.CuidDiagId)]
  #endif
  ```

### Performance Patterns
- Use `stackalloc` for temporary buffers where possible
- Prefer `string.Create` over manual char array allocation (except in `NETSTANDARD2_0`)
- Use `Span<T>` and `ReadOnlySpan<T>` for efficient memory operations
- Cache expensive operations (fingerprints, environment variables) using `Lazy<T>`

### Serialization
- `Cuid` supports JSON via `CuidConverter` (System.Text.Json) and XML via `IXmlSerializable`
- `Cuid2` intentionally does NOT support serialization - only `.ToString()`
- Custom converters are in `src/cuid.net/Serialization/Json/Converters/`

## Testing

### Framework and Tools
- **TUnit** (v1.9.42) - test framework
- **Verify.TUnit** - snapshot testing for API surface validation
- **PublicApiGenerator** - detecting breaking changes
- **AwesomeAssertions** - assertions
- Target frameworks: `net48`, `net8.0`, `net10.0`

### API Surface Tests
- `ApiTests.cs` uses Verify snapshots to ensure no breaking changes
- Regenerate snapshots only when API changes are intentional
- Snapshots validate the public API surface using PublicApiGenerator

## Build Commands

```bash
# Build
dotnet build

# Run tests
dotnet test
dotnet test --framework net8.0
dotnet test --framework net10.0

# Run tests with coverage (CI format)
dotnet test -c release \
  --logger:trx \
  --results-directory ./TestResults \
  -p:CollectCoverage=true \
  -p:CoverletOutputFormat=opencover \
  -p:CoverletOutput=./CodeCoverage/coverage.opencover.xml

# Pack NuGet package
dotnet pack -c Release
```

## Package Management

- Central Package Management enabled via `Directory.Packages.props`
- Test projects automatically receive test packages via `IsTestProject` condition
- Key dependencies:
  - **BouncyCastle.Cryptography** - SHA-3/Keccak support
  - **CommunityToolkit.Diagnostics** - Guard clauses
  - **MinVer** - semantic versioning from git tags

## Versioning

- Uses **MinVer** for semantic versioning
- Version determined from git tags
- Build metadata includes GitHub run number in CI builds
- See `.github/workflows/release.yml` for release process

## CI/CD Requirements

- CI runs on Windows (required for multi-targeting)
- Integrates with SonarCloud for code quality and coverage
- Tests must pass for all target frameworks
- Library supports trimming for .NET 8+ (`IsTrimmable=true`)

## Important Constraints

1. **Cuid** is deprecated but maintained for backward compatibility - emits `VISLIB0001` warning
2. **Cuid2** cannot be sorted (no `IComparable`) - this is intentional for security
3. **Cuid2** does not support serialization - use `.ToString()` for persistence
4. Multi-targeting requires careful use of conditional compilation
5. All changes must work across `netstandard2.0`, `netstandard2.1`, `net8.0`, and `net10.0`
6. API changes require snapshot regeneration and careful consideration of breaking changes

## Common Patterns in This Codebase

### Base-36 Encoding
- Utility methods in `Utils.cs` handle encoding/decoding
- Used extensively in `Cuid` (v1) for compact representation

### Counter Pattern
- Thread-safe counter implementations in both `Cuid` and `Cuid2`
- Uses `Interlocked.Increment` for atomic operations
- `Cuid2` counter initialized with random value for better security

### Fingerprint Caching
- Static `Context` nested class caches fingerprints at startup
- Avoids repeated computation of expensive operations
- Different versions for `Cuid` (v1) and `Cuid2`

### Span-Based String Building
- Custom `WriteTo(ref Span<char> dest)` extension methods
- Enables efficient string construction without allocations
- Falls back to char arrays in `NETSTANDARD2_0`
