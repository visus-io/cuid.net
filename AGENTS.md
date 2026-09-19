# Agent Instructions

Developer and AI-agent guide for this repository.
This document follows the ASD-STE100 (Simplified Technical English) style: short sentences, one instruction per sentence, active voice.

<!-- architecture -->
@ARCHITECTURE.md
<!-- architecture -->

## Language and Runtime

- Language version is C# 14 (`LangVersion` in `Directory.Build.props`).
- The library targets `netstandard2.0`, `netstandard2.1`, `net8.0`, and `net10.0`.
- The test project targets `net48`, `net8.0`, and `net10.0`. All three must pass.

## Code Style

Follow `.editorconfig` exactly. Key rules:

- **No `var`**. Use explicit types everywhere.
- **Allman braces** — opening `{` on its own line (`csharp_new_line_before_open_brace = all`).
- **Indentation**: 4 spaces (2 for `.json`, `.props` files). **Line endings**: LF.
- **Private fields**: `_camelCase`. Public members: `PascalCase`.
- **Explicit accessibility modifiers** on every non-interface member.
- Prefer `readonly` for fields.
- **Trailing comma** on multiline object/collection initializers.
- Prefer null-coalescing operators and collection initializers.
- Add XML doc comments on every public API member (`GenerateDocumentationFile` is `true`).
- Add `[MethodImpl(AggressiveInlining)]` on hot-path internal methods.
- Use `stackalloc` and `Span<T>` for temporary buffers. Avoid heap allocations in hot paths.
- Guard APIs missing from netstandard2.0/2.1 with `#if NETSTANDARD` (for example, `DateTimeOffset.UnixEpoch`).
- Use `CommunityToolkit.Diagnostics.Guard` for all parameter validation. Do not write manual `if`/`throw` checks.
- Use `readonly struct` for value types. Implement `IEquatable<T>` and override `GetHashCode`.

## Project Structure Rules

- New supporting types go in `src/cuid.net/`, mirroring the existing flat layout (`Abstractions/`, `Extensions/`, `Serialization/Json/Converters/`).
- `Cuid2` and `Cuid` must not depend on each other. They share only `Fingerprint.cs` and `Utils.cs`.
- Never add a `Version` attribute to a `<PackageReference>` — all versions are managed centrally in `Directory.Packages.props`.
- After any package change, run `dotnet restore` to update `packages.lock.json`, then commit it. CI runs with `RestoreLockedMode=true`; a stale lock file fails the build.

## Patterns to Follow

- **Cache expensive per-process work.** Reuse the existing caching patterns — `Context.IdentityFingerprint`, the `Lazy<byte[]>` environment-variable snapshot, and the `[ThreadStatic] Sha3Digest` instance — instead of recomputing fingerprint or hashing state per instance.
- **Never dispose the cached `[ThreadStatic] IncrementalHash`.** `Cuid2.cs` caches one native `IncrementalHash` per thread for the SHA3-512 path, the same way it caches `[ThreadStatic] Sha3Digest` for the BouncyCastle fallback. `IncrementalHash` wraps its native digest context in a `SafeHandle` (`SafeEvpMdCtxHandle` on Linux, `SafeDigestCtxHandle` on macOS, `SafeBCryptHashHandle` on Windows). A `SafeHandle` finalizes its native handle on its own once the thread-static reference is unreachable, so this does not leak. Wrapping the cached instance in `using` per call reintroduces a per-`Cuid2` allocation on the construction hot path. See `ARCHITECTURE.md`'s Performance Details section for the full explanation.
- **Centralize obsoletion messages.** Route every new deprecation through the `Obsoletions.cs` constant pattern (`DiagnosticId`, message) instead of inlining a new `[Obsolete]` id at the call site.

## Testing Requirements

Framework: **TUnit** + **AwesomeAssertions** + **Verify** (snapshot)

- Group tests with `[Property("Category", "…")]` (for example, `"Comparison"`, `"Construction"`).
- Use `[Arguments(…)]` for parameterized cases.
- Run collision-resistance tests for **10 000 iterations**, concurrently.
- `ApiTests.cs` uses **PublicApiGenerator** to snapshot the public API surface. After any intentional API change, run `dotnet test`, then accept the new `.verified.txt` snapshot.

When you add a new public API:
1. Implement it with full XML doc comments.
2. Add unit tests. Cover construction, equality, and edge cases.
3. Run `dotnet test`. `ApiTests` fails on the first run. Accept the new snapshot.

## Multi-Targeting Guidelines

- Wrap APIs unavailable on netstandard in `#if NETSTANDARD` or `#if NET8_0_OR_GREATER`.
- `Microsoft.Bcl.HashCode` and `PolySharp` backfill APIs on the netstandard targets only. Do not reference them from `net8.0`/`net10.0` code paths.
- Prefer APIs from `System.Runtime.InteropServices`, `System.Buffers`, and `System.Security.Cryptography`. These have good cross-framework coverage.
- Run tests on all frameworks before you submit: `dotnet test --framework net48 && dotnet test --framework net10.0`.

## YAML and Workflow Files

- Before finishing any change that touches a `.yml`/`.yaml` file, check whether `yamllint` is available (`command -v yamllint`) and, if so, run it against the changed file(s) (config lives at `.yamllint.yml`). Skip silently only if the tool is not installed.
- If the changed `.yml` file is a GitHub Actions workflow under `.github/workflows/`, additionally check whether `actionlint` is available (`command -v actionlint`) and, if so, run it against the changed file(s). Skip silently only if the tool is not installed.

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

## Documentation

- Any change to the public API surface, the construction pipeline, or a supporting type's responsibility must update `ARCHITECTURE.md` in the same change.
- Any change to user-facing behavior (a new option, a changed default, a new supported type) must also update `README.md`.

## What NOT to Do

- Do not use `var` anywhere.
- Do not write manual `if`/`throw` parameter checks — use `CommunityToolkit.Diagnostics.Guard`.
- Do not add a `Version` attribute to any `<PackageReference>` — versioning is centralized in `Directory.Packages.props`.
- Do not push release tags. **MinVer** derives the version from Git tags (`v1.2.3`). Only maintainers push them.
- Do not skip the `ApiTests` snapshot update after a public API change.
- Do not inline a new `[Obsolete]` diagnostic id — extend `Obsoletions.cs` instead.
- Do not dispose or re-create the cached `[ThreadStatic] IncrementalHash` per call. Its `SafeHandle` reclaims native resources on its own. Disposing per call reintroduces the allocation this cache exists to avoid.
- Do not introduce a dependency between `Cuid2` and `Cuid`.
