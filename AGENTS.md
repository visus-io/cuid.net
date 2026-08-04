# AGENTS.md — cuid.net

Developer and AI-agent guide for this repository.
This document follows the ASD-STE100 (Simplified Technical English) style: short sentences, one instruction per sentence, active voice.

@ARCHITECTURE.md

---

## Project Overview

**cuid.net** is a .NET library. It generates collision-resistant unique identifiers (CUIDs) for distributed systems.
The library ships two identifier types, `Cuid2` (recommended) and `Cuid` (deprecated).

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
├── ARCHITECTURE.md                 # Internal design and construction pipeline
├── CONTRIBUTING.md                 # Contribution guide
└── SECURITY.md                     # Security policy
```

---

## Prerequisites

- **.NET SDK 10.0+**. The version is pinned in `global.json`. Run `dotnet --version` to check the match.
- **Windows** is the recommended platform. CI runs on Windows to cover the `net48` target. Most work also builds on macOS and Linux for `net8.0` and `net10.0`.

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

## Code Conventions

Follow `.editorconfig` exactly. Key rules:

- **Language version**: C# 14
- **Indentation**: 4 spaces (2 for `.json`, `.props` files)
- **Line endings**: LF
- **No `var`**. Use explicit types everywhere.
- **Private fields**: `_camelCase`. Public members: `PascalCase`.
- Prefer `readonly` for fields.
- Add access modifiers on all non-interface members.
- Prefer null-coalescing operators and collection initializers.
- Add XML doc comments on every public API member.
- Add `[MethodImpl(AggressiveInlining)]` on hot-path internal methods.
- Use `stackalloc` and `Span<T>` for temporary buffers. Avoid heap allocations in hot paths.
- Guard APIs missing from netstandard2.0/2.1 with `#if NETSTANDARD` (for example, `DateTimeOffset.UnixEpoch`).
- Use `CommunityToolkit.Diagnostics.Guard` for all parameter validation. Do not write manual `if`/`throw` checks.
- Use `readonly struct` for value types. Implement `IEquatable<T>` and override `GetHashCode`.

---

## Testing Conventions

Framework: **TUnit** + **AwesomeAssertions** + **Verify** (snapshot)

- Tests target `net48`, `net8.0`, and `net10.0`. All three must pass.
- Group tests with `[Property("Category", "…")]` (for example, `"Comparison"`, `"Construction"`).
- Use `[Arguments(…)]` for parameterized cases.
- Run collision-resistance tests for **10 000 iterations**, concurrently.
- `ApiTests.cs` uses **PublicApiGenerator** to snapshot the public API surface. After an intentional API change, regenerate the snapshot: run `dotnet test`, then update the `.verified.txt` files.

When you add a new public API:
1. Implement it with full XML doc comments.
2. Add unit tests. Cover construction, equality, and edge cases.
3. Run `dotnet test`. `ApiTests` fails on the first run. Accept the new snapshot.

---

## Dependency Management

- Versions are **centralized** in `Directory.Packages.props`. Do not set a `Version` attribute in individual `.csproj` files.
- `packages.lock.json` is enforced. After any package change, run `dotnet restore` to update the lock file, then commit it.
- CI runs with `RestoreLockedMode=true`. A stale lock file fails the build.
- **Renovate** (`renovate.json`) automates dependency updates.
- `CentralPackageTransitivePinningEnabled=true` pins transitive versions.

Key runtime dependencies:

| Package                        | Purpose                                                  |
|---------------------------------|-----------------------------------------------------------|
| `BouncyCastle.Cryptography`    | SHA-3 512-bit hashing (Cuid2)                            |
| `CommunityToolkit.Diagnostics` | Guard clauses / parameter validation                     |
| `System.Text.Json`             | JSON serialization (netstandard targets only)            |
| `PolySharp`                    | C# language backport for netstandard (compile-time only) |

---

## Commit and PR Conventions

The workflow `.github/workflows/lint_pullrequest.yml` enforces this format.

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

The PR title must match the single commit message format. The subject must **not** start with an uppercase letter.

---

## CI/CD

### ci.yml (continuous integration)
- Triggers on push to `main` and on all PRs. Excludes markdown files, `renovate.json`, and issue templates.
- Runs on **Windows** (required to cover `net48`).
- Steps: restore, build, test with coverage, upload to SonarCloud, publish test results.
- SonarCloud project: `visus:cuid.net`. Skipped for bot PRs.

### release.yml
- Triggers on a tag push or a manual workflow dispatch.
- Requires `production` environment approval.
- Steps: restore, build with MinVer version, pack, push to nuget.org.

### lint_pullrequest.yml
- Validates that the PR title matches the Conventional Commits format.
- Validates that the single commit on the PR matches the PR title.

---

## Multi-Targeting Guidelines

The library targets `netstandard2.0`, `netstandard2.1`, `net8.0`, and `net10.0`. When you add code:

- Wrap APIs unavailable on netstandard in `#if NETSTANDARD` or `#if NET8_0_OR_GREATER`.
- `Microsoft.Bcl.HashCode` provides `HashCode` on netstandard targets.
- Prefer APIs from `System.Runtime.InteropServices`, `System.Buffers`, and `System.Security.Cryptography`. These have good cross-framework coverage.
- Run tests on all frameworks before you submit: `dotnet test --framework net48 && dotnet test --framework net10.0`.

---

## Versioning

- **MinVer** derives the version from Git tags (format: `v1.2.3`).
- The project follows Semantic Versioning 2.0:
  - `MAJOR` — breaking API changes. Requires `PublicApiGenerator` snapshot updates.
  - `MINOR` — new backward-compatible features.
  - `PATCH` — bug fixes.
- Only maintainers push release tags. Do not create tags manually.
