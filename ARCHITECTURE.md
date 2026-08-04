# ARCHITECTURE.md — cuid.net

This document describes the internal design of the cuid.net library.

---

## Type comparison

The library ships two identifier types:

| Type    | Status                      | Description                                                                                                   |
|---------|------------------------------|------------------------------------------------------------------------------------------------------------------|
| `Cuid2` | **Recommended**              | Cryptographically strong. Variable length (4–32 characters, default 24). Uses SHA-3 512-bit hashing. Opaque.    |
| `Cuid`  | **Deprecated** (`VISLIB0001`) | Sortable, 25 characters. Leaks the creation timestamp. Kept only for backward compatibility.                    |

---

## Cuid2 (recommended)

`Cuid2` is an immutable `readonly struct`. It implements `IEquatable<Cuid2>`.
The implementation is in `src/cuid.net/Cuid2.cs`.

### Construction pipeline

The constructor builds an identifier in six steps:

1. Capture `DateTimeOffset.UtcNow` ticks. Store the value in `_timestamp`.
2. Increment a process-local atomic counter. The counter is a lazy singleton. It uses `Interlocked.Increment`.
3. Fetch the process fingerprint from `Fingerprint.Generate()`. The fingerprint combines the hostname, the process ID, and environment variables. The result is cached in `Context.IdentityFingerprint`.
4. Generate a random alphabetic prefix with `Utils.GenerateCharacterPrefix()`.
5. Generate random bytes with `Utils.GenerateRandom(maxLength)`.
6. Hash the timestamp, the counter, the fingerprint, and the random bytes together. Use SHA-3 512-bit (BouncyCastle). Encode the hash in base-36 with `Utils.Encode`. Prepend the prefix. Truncate the result to `maxLength`.

`DateTimeOffset.UnixEpoch` is not available in netstandard2.0. `Cuid2.cs` guards this API with a pragma pattern (`#if NETSTANDARD`).

### Usage

```csharp
Cuid2 id = new();            // default length 24
Cuid2 id = new(32);          // custom length 4–32
string s  = id.ToString();
```

---

## Cuid (deprecated)

`Cuid` is a `readonly struct`. It implements `IComparable<Cuid>` and `IEquatable<Cuid>`. It supports JSON and XML serialization.
The implementation is in `src/cuid.net/Cuid.cs`.

`Cuid` emits compiler diagnostic `VISLIB0001` on every use.
Do not use `Cuid` in new code. The type exists only to support migration from earlier versions.

---

## Supporting types

| File                      | Role                                                                                                              |
|---------------------------|---------------------------------------------------------------------------------------------------------------------|
| `Fingerprint.cs`          | Generates v1 and v2 host fingerprints. Version 2 hashes the hostname, the process ID, and environment variables with SHA-3. |
| `Utils.cs`                | `Encode(byte[])` encodes a `BigInteger` in base-36. `GenerateRandom()` produces random bytes with `RandomNumberGenerator`. `GenerateCharacterPrefix()` produces the random alphabetic prefix. |
| `Obsoletions.cs`          | Defines the `DiagnosticId` constant `"VISLIB0001"` and the associated message.                                     |
| `StringExtensions.cs`     | Defines `TrimPad` and `WriteTo`. Both are zero-allocation helpers.                                                 |
| `FingerprintVersion` enum | Defines three values: `None = 0`, `One = 1`, `Two = 2`.                                                            |
| `CuidConverter.cs`        | A `System.Text.Json` converter for `Cuid` (version 1 only).                                                        |
