# AGENTS.md — cuid.net

Developer and AI-agent guide for working in this repository.

---

## Project Overview

**cuid.net** is a .NET library providing collision-resistant unique identifiers (CUIDs) for horizontal scalability and security in distributed environments. It ships two identifier types:

| Type    | Status                      | Description                                                                                       |
|---------|-----------------------------|---------------------------------------------------------------------------------------------------|
| `Cuid2` | **Recommended**             | Cryptographically strong, variable length (4–32 chars, default 24), SHA-3 512-bit hashing, opaque |
| `Cuid`  | **Deprecated** (VISLIB0001) | Sortable 25-char identifier with timestamp leakage; kept for backward compatibility               |

NuGet package: [`cuid.net`](https://www.nuget.org/packages/cuid.net)
GitHub repository: `https://github.com/visus-io/cuid.net`
License: MIT

---

## Repository Layout

```
cuid.net/
├── src/
│   └── cuid.net/                   # Main library (C# 14, ~1 100 LOC)
│       ├── Cuid.cs                 # Deprecated CUIDv1 implementation
│       ├── Cuid2.cs                # Recommended CUIDv2 implementation
│       ├── Fingerprint.cs          # Host-identity generation (hostname + PID + env)
│       ├── Utils.cs                # Base-36 encode/decode, RNG helpers
│       ├── Obsoletions.cs          # Diagnostic ID constant: VISLIB0001
│       ├── Abstractions/
│       │   └── FingerprintVersion.cs
│       ├── Extensions/
│       │   └── StringExtensions.cs
│       └── Serialization/
│           └── Json/Converters/
│               └── CuidConverter.cs
├── tests/
│   ├── cuid.net.tests/             # TUnit test suite (net48, net8.0, net10.0)
│   │   ├── CuidTests.cs            # CUIDv1 tests
│   │   ├── Cuid2Tests.cs           # CUIDv2 tests
│   │   ├── ApiTests.cs             # Public API surface / breaking-change detection
│   │   └── ModuleInitializer.cs    # Test fixture setup
│   └── cuid.net.tests.net48/       # .NET Framework 4.8 test target
├── .github/workflows/
│   ├── ci.yml                      # CI: build + test + SonarCloud
│   ├── release.yml                 # Release: pack + publish to nuget.org
│   └── lint_pullrequest.yml        # PR title semantic validation
├── Directory.Build.props           # Global build properties (analysis, trimming, docs)
├── Directory.Packages.props        # Centralized NuGet version management
├── global.json                     # Pins .NET SDK to 10.0.202
├── nuget.config                    # Single NuGet source: nuget.org
├── cuid.net.slnx                   # Solution file
├── README.md                       # User-facing documentation
├── CONTRIBUTING.md                 # Contribution guide
└── SECURITY.md                     # Security policy
```

---

## Prerequisites

- **.NET SDK 10.0+** — version is pinned in `global.json`; `dotnet --version` must match
- **Windows** recommended — CI runs on Windows to cover the `net48` target; most work builds fine on macOS/Linux for `net8.0`/`net10.0`

---

## Essential Commands

```bash
# Restore (locked mode; required after any package change)
dotnet restore

# Build (Release)
dotnet build -c Release --no-restore

# Build (Debug, fast iteration)
dotnet build

# Run all tests
dotnet test -c Release --no-build --no-restore

# Run tests for a specific framework
dotnet test --framework net8.0
dotnet test --framework net10.0

# Run tests with coverage (CI format)
dotnet test -c Release --no-build --no-restore \
  -- --coverage --coverage-output-format xml --report-trx

# Pack NuGet package
dotnet pack -c release --no-restore --no-build
```

---

## Architecture

### Cuid2 (recommended)

`src/cuid.net/Cuid2.cs` — immutable `readonly struct` implementing `IEquatable<Cuid2>`.

Construction pipeline:
1. Capture `DateTimeOffset.UtcNow` ticks as `_timestamp`
2. Increment a process-local atomic counter (`Counter`, lazy singleton via `Interlocked.Increment`)
3. Fetch process fingerprint (`Fingerprint.Generate()` — hostname + PID + env vars, cached in `Context.IdentityFingerprint`)
4. Generate a random alphabetic prefix via `Utils.GenerateCharacterPrefix()`
5. Generate random bytes via `Utils.GenerateRandom(maxLength)`
6. Hash timestamp + counter + fingerprint + random bytes with **SHA-3 512-bit** (BouncyCastle)
7. Encode result with base-36 (`Utils.Encode`), prepend prefix, truncate to `maxLength`

```csharp
Cuid2 id = new();            // default length 24
Cuid2 id = new(32);          // custom length 4–32
string s  = id.ToString();
```

### Cuid (deprecated)

`src/cuid.net/Cuid.cs` — `readonly struct` implementing `IComparable<Cuid>`, `IEquatable<Cuid>`, JSON/XML serialization. Emits compiler diagnostic `VISLIB0001` on any usage. Do not use for new code; support only for migration.

### Supporting types

| File                      | Role                                                                                                             |
|---------------------------|------------------------------------------------------------------------------------------------------------------|
| `Fingerprint.cs`          | Generates v1/v2 host fingerprints; v2 uses SHA-3 over hostname + PID + env                                       |
| `Utils.cs`                | `Encode(byte[])` — BigInteger base-36; `GenerateRandom()` — `RandomNumberGenerator`; `GenerateCharacterPrefix()` |
| `Obsoletions.cs`          | Defines `DiagnosticId = "VISLIB0001"` and the associated message constant                                        |
| `StringExtensions.cs`     | `TrimPad` / `WriteTo` — zero-allocation helpers                                                                  |
| `FingerprintVersion` enum | `None = 0`, `One = 1`, `Two = 2`                                                                                 |
| `CuidConverter.cs`        | `System.Text.Json` converter for `Cuid` (v1 only)                                                                |

---

## Code Conventions

Follow `.editorconfig` exactly. Key rules:

- **Language version**: C# 14
- **Indentation**: 4 spaces (2 for `.json`, `.props` files)
- **Line endings**: LF
- **No `var`** — explicit types required everywhere
- **Private fields**: `_camelCase`; public members: `PascalCase`
- **`readonly` preferred** for fields
- **Access modifiers required** on all non-interface members
- **Null-coalescing** and **collection initializers** preferred
- **XML doc comments** on every public API member
- **`[MethodImpl(AggressiveInlining)]`** on hot-path internal methods
- **`stackalloc` / `Span<T>`** for temporary buffers — avoid heap allocations in hot paths
- **`#if NETSTANDARD`** guards for APIs missing in netstandard2.0/2.1 (e.g., `DateTimeOffset.UnixEpoch`)
- **`CommunityToolkit.Diagnostics.Guard`** for all parameter validation (no manual `if`/`throw`)
- Use `readonly struct` for value types; implement `IEquatable<T>` and override `GetHashCode`

---

## Testing Conventions

Framework: **TUnit** + **AwesomeAssertions** + **Verify** (snapshot)

- Tests target `net48`, `net8.0`, `net10.0` — all must pass
- `[Property("Category", "…")]` groups tests (e.g., `"Comparison"`, `"Construction"`)
- `[Arguments(…)]` for parameterized cases
- Collision-resistance tests run **10 000 iterations** concurrently
- `ApiTests.cs` uses **PublicApiGenerator** to snapshot the public API surface — if you make intentional API changes, regenerate the snapshot with `dotnet test` after updating the `.verified.txt` files

When adding a new public API:
1. Implement with full XML doc comments
2. Add unit tests covering construction, equality, edge cases
3. Run `dotnet test` — `ApiTests` will fail; accept the new snapshot

---

## Dependency Management

- Versions are **centralized** in `Directory.Packages.props` — never set a `Version` attribute in individual `.csproj` files
- `packages.lock.json` is enforced — after any package change run `dotnet restore` to update the lock file and commit it
- CI runs with `RestoreLockedMode=true`; builds will fail if the lock file is stale
- Dependency updates are automated via **Renovate** (`renovate.json`)
- `CentralPackageTransitivePinningEnabled=true` — transitive versions are pinned

Key runtime dependencies:

| Package                        | Purpose                                                  |
|--------------------------------|----------------------------------------------------------|
| `BouncyCastle.Cryptography`    | SHA-3 512-bit hashing (Cuid2)                            |
| `CommunityToolkit.Diagnostics` | Guard clauses / parameter validation                     |
| `System.Text.Json`             | JSON serialization (netstandard targets only)            |
| `PolySharp`                    | C# language backport for netstandard (compile-time only) |

---

## Commit and PR Conventions

Enforced by `.github/workflows/lint_pullrequest.yml`.

Format: **Conventional Commits**
```
<type>: <subject in lowercase>
```

Allowed types: `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `build`, `ci`, `chore`, `revert`

Examples:
```
feat: add ISpanFormattable support to Cuid2
fix: correct base-36 encoding for zero-padded values
test: add edge case for minimum length Cuid2
chore: update BouncyCastle to 2.7.0
```

PR title must match the single commit message format. Subject must **not** start with an uppercase letter.

---

## CI/CD

### ci.yml (continuous integration)
- Triggers on push to `main` and all PRs (excludes markdown, `renovate.json`, issue templates)
- Runs on **Windows** (required for `net48`)
- Steps: restore → build → test with coverage → SonarCloud upload → publish test results
- SonarCloud project: `visus:cuid.net` (skipped for bot PRs)

### release.yml
- Triggered by tag push or manual workflow dispatch
- Requires `production` environment approval
- Steps: restore → build with MinVer version → pack → push to nuget.org

### lint_pullrequest.yml
- Validates PR title matches Conventional Commits format
- Also validates that the single commit on the PR matches the PR title

---

## Multi-Targeting Guidelines

The library targets `netstandard2.0`, `netstandard2.1`, `net8.0`, `net10.0`. When adding code:

- Wrap APIs unavailable on netstandard in `#if NETSTANDARD` / `#if NET8_0_OR_GREATER`
- `DateTimeOffset.UnixEpoch` — not available in netstandard2.0 (see `Cuid2.cs` pragma pattern)
- `HashCode` — provided via `Microsoft.Bcl.HashCode` on netstandard targets
- Prefer APIs from `System.Runtime.InteropServices`, `System.Buffers`, and `System.Security.Cryptography` which have good cross-framework coverage
- Run tests on all frameworks before submitting: `dotnet test --framework net48 && dotnet test --framework net10.0`

---

## Versioning

- **MinVer** derives version from Git tags (format: `v1.2.3`)
- Follows Semantic Versioning 2.0:
  - `MAJOR` — breaking API changes (requires `PublicApiGenerator` snapshot updates)
  - `MINOR` — new backward-compatible features
  - `PATCH` — bug fixes
- Only maintainers push release tags; do not create tags manually
