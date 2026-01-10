# Security Policy

## Supported Versions

We actively support the following versions with security updates:

| Version | Supported          | Notes                               |
|---------|--------------------|-------------------------------------|
| 7.x     | :white_check_mark: | Current version, CUIDv2 recommended |
| 6.x     | :white_check_mark: | Security patches only               |
| < 6.0   | :x:                | No longer supported                 |

## Known Security Issues

### CUIDv1 (Cuid) - Deprecated for Security Reasons

**Status:** Deprecated (emits compiler warning `VISLIB0001`)

The original CUID implementation (`Cuid` type) has been deprecated due to security vulnerabilities:
- **Predictability**: The algorithm's deterministic nature makes IDs potentially guessable
- **Collision resistance**: Not cryptographically strong enough for security-sensitive use cases
- **Timing attacks**: Timestamp-based generation may leak information

**Migration:** All users should migrate to `Cuid2` for security-sensitive applications.

```csharp
// ❌ Deprecated - do not use for new code
var oldId = Cuid.NewCuid();

// ✅ Recommended - cryptographically strong
var newId = Cuid2.NewCuid2();
```

## Reporting a Vulnerability

We take security vulnerabilities seriously. If you discover a security issue, please follow these steps:

### 1. **Do Not** Open a Public Issue

Please do not report security vulnerabilities through public GitHub issues. Public disclosure may put users at risk.

### 2. Report Privately

**Preferred:** Use GitHub Security Advisories:
- Navigate to the Security tab in the repository
- Click "Report a vulnerability"
- Provide detailed information about the issue

**Alternative:** Email security@projects.visus.io

### 3. Include These Details

When reporting a security vulnerability, please include:

- **Type of vulnerability** (e.g., cryptographic weakness, ID predictability, timing attack)
- **Affected versions** (specific version numbers or ranges)
- **Steps to reproduce** (detailed reproduction steps with code samples if applicable)
- **Potential impact** (what an attacker could achieve)
- **Suggested fix** (if available)
- **Your contact information** for follow-up questions
- **CVE/CWE identifiers** (if applicable)

### 4. Response Timeline

We are committed to transparent and timely responses:

- **Initial Acknowledgment:** Within 48 hours of receipt
- **Assessment & Severity Classification:** Within 7 days
  - We will provide a severity classification (Critical, High, Medium, Low)
  - Regular status updates every 7-14 days
- **Resolution Timeline:** Varies based on severity
  - **Critical:** 7-14 days
  - **High:** 14-30 days
  - **Medium:** 30-60 days
  - **Low:** 60-90 days or next scheduled release

### 5. Disclosure Policy

- We request a **90-day disclosure window** before public revelation to allow users time to update
- We will credit reporters in security advisories (unless you prefer to remain anonymous)
- We will coordinate disclosure timing with you
- We will publish security advisories through GitHub Security Advisories and update affected NuGet packages

## Security Scope

### In-Scope Vulnerabilities

We consider the following issues to be in scope for security reports:

- **Cryptographic Weaknesses**: Flaws in SHA-3 usage, entropy issues, or hash function vulnerabilities
- **ID Predictability**: Ability to predict future or past IDs beyond theoretical probability
- **Collision Vulnerabilities**: Practical collision attacks beyond birthday paradox expectations
- **Memory Safety Issues**: Buffer overflows, memory leaks, or unsafe operations
- **Timing Attacks**: Information leakage through execution time variations
- **Dependency Vulnerabilities**: Security issues in BouncyCastle or other dependencies
- **Thread Safety Issues**: Race conditions or concurrency vulnerabilities in ID generation
- **Fingerprinting Attacks**: Exploitation of system fingerprinting for privacy violations
- **RNG Weaknesses**: Predictable or insufficient randomness in ID generation

### Out-of-Scope Issues

The following are generally **not considered security vulnerabilities**:

- **Theoretical Collision Probability**: Expected birthday paradox collision rates (2^128 for Cuid2)
- **Application Misuse**: Using CUIDs as cryptographic secrets, passwords, or session tokens
- **Resource Exhaustion DoS**: Memory or CPU exhaustion from generating large numbers of IDs
- **Non-Security Configuration**: Preference for different ID lengths or character sets
- **Performance Characteristics**: Speed of ID generation (unless enabling timing attacks)
- **Compatibility Issues**: Lack of support for unsupported .NET versions or platforms
- **Build System Issues**: Development/build problems not affecting runtime security

If you're unsure whether an issue is in scope, please report it and we'll make the determination.

## Recommended Uses

### ✅ Appropriate Use Cases

Cuid2 is well-suited for:

- **Public-facing URL identifiers** (e.g., `/posts/ck2k3j4h00000`)
- **Database primary keys** (distributed systems, horizontal scaling)
- **File and resource naming** (unique file identifiers)
- **Distributed system identifiers** (microservices, message queues)
- **Correlation IDs** (request tracking, distributed tracing)
- **Non-sequential record IDs** (preventing enumeration attacks)

### ❌ Not Recommended For

CUIDs should **not** be used for:

- **Cryptographic secrets** (encryption keys, signing keys)
- **Passwords or password resets** (use bcrypt, Argon2, or PBKDF2)
- **Session identifiers** (use cryptographically secure session tokens)
- **API keys or bearer tokens** (use dedicated token generation libraries)
- **Security-critical random values** (use `RandomNumberGenerator` directly)
- **HMAC keys or nonces** (use proper cryptographic libraries)

**Important:** While Cuid2 is cryptographically strong for uniqueness, it is designed as a collision-resistant identifier, not a cryptographic primitive.

## Security Best Practices

### When Using This Library

1. **Use Cuid2 for security-sensitive applications**
   - Cuid2 uses SHA-3 512-bit (NIST FIPS-202 compliant)
   - Variable length support (4-32 characters, default 24)
   - Not sortable by design (prevents information leakage)
   - Cryptographically strong random prefix

2. **Do not use CUIDs as secrets**
   - CUIDs are unique identifiers, not cryptographic keys
   - Use `System.Security.Cryptography.RandomNumberGenerator` for secrets
   - Use proper cryptographic libraries for authentication tokens

3. **Consider your threat model**
   - **For public-facing IDs in URLs:** Cuid2 is appropriate
   - **For session tokens or API keys:** Use dedicated cryptographic libraries
   - **For database primary keys:** Either version works (prefer Cuid2)
   - **For rate limiting or quota tracking:** Cuid2 is appropriate

4. **Keep dependencies updated**
   ```bash
   # Check for outdated packages
   dotnet list package --outdated

   # Update to latest version
   dotnet add package cuid.net

   # Check for known vulnerabilities
   dotnet list package --vulnerable
   ```

5. **Monitor for security advisories**
   - Watch this repository for security updates
   - Subscribe to GitHub Security Advisories
   - Check NuGet package advisories regularly
   - Monitor BouncyCastle security updates

6. **Use appropriate ID length**
   ```csharp
   // Default 24 characters (2^128 collision resistance)
   var id = Cuid2.NewCuid2();

   // Longer for extreme collision resistance
   var longId = Cuid2.NewCuid2(32);

   // Shorter only if collision resistance requirements are lower
   var shortId = Cuid2.NewCuid2(16);
   ```

7. **Thread safety considerations**
   - Both `Cuid` and `Cuid2` generation methods are thread-safe
   - Safe for concurrent use across multiple threads
   - Internal counter uses atomic operations for consistency

## Technical Security Details

### Cuid2 Cryptographic Implementation

- **Hash Algorithm**: SHA-3 512-bit (Keccak) via BouncyCastle (NIST FIPS-202 compliant)
- **Random Number Generator**: `System.Security.Cryptography.RandomNumberGenerator` (CSPRNG)
- **Entropy Sources**:
  - High-resolution timestamp (100-nanosecond ticks)
  - Cryptographically secure random data
  - Atomic counter (thread-safe)
  - System fingerprint (hostname, process ID, environment variables)
- **Collision Resistance**: 2^128 (default 24-character length)
- **Character Set**: Base-36 (a-z, 0-9, lowercase)
- **Random Prefix**: Single random letter (a-z) prevents dictionary attacks

### Cuid (Legacy/Deprecated)

- **Hash Algorithm**: None (Base-36 encoding only)
- **Random Number Generator**: Non-cryptographic (timestamp + counter + basic random)
- **Known Vulnerabilities**: Predictable, timing attacks, insufficient entropy
- **Status**: Deprecated, do not use for new applications

### Fingerprinting

The library incorporates system fingerprinting to reduce collision probability across distributed systems:

- **Version 1 (Legacy)**: Hostname + Process ID
- **Version 2 (Default)**: Hostname + Process ID + Environment Variables

Fingerprinting does not provide cryptographic security but aids in uniqueness across distributed deployments.

## Security Testing

### For Security Researchers

We welcome security research on this library. When testing, consider:

1. **Thread Safety Testing**
   - Generate IDs concurrently from multiple threads
   - Test for race conditions in counter management
   - Validate uniqueness under high concurrency

2. **Entropy Analysis**
   - Analyze randomness distribution
   - Test RNG quality and seeding
   - Validate SHA-3 hash output distribution

3. **Collision Testing**
   - Generate large batches of IDs (millions+)
   - Test for premature collisions
   - Validate birthday paradox expectations

4. **Timing Attack Analysis**
   - Measure ID generation timing variations
   - Test for information leakage via timing
   - Analyze constant-time properties

5. **Memory Safety**
   - Test for memory leaks in long-running scenarios
   - Validate buffer handling
   - Test with sanitizers if using interop

### Recommended Testing Tools

- **Static Analysis**: Roslyn analyzers, SonarQube, SecurityCodeScan
- **Dynamic Analysis**: dotMemory, perfview
- **Fuzzing**: SharpFuzz, libFuzzer
- **Concurrency Testing**: CHESS, Coyote

## Cryptographic Dependencies

This library depends on the following cryptographic components:

- **BouncyCastle.Cryptography** (≥2.5.0)
  - Provides SHA-3 512-bit (Keccak) implementation
  - NIST FIPS-202 compliant
  - Regularly updated and maintained

- **System.Security.Cryptography.RandomNumberGenerator** (.NET BCL)
  - Cryptographically secure random number generator
  - Platform-specific implementations (CNG on Windows, OpenSSL on Linux)

We actively monitor these dependencies for security updates and will release patches as needed.

### Dependency Security Monitoring

- BouncyCastle releases are monitored via GitHub Dependabot
- CVE databases are checked regularly
- NuGet package vulnerability scanning is enabled
- Critical dependency updates are released within 7 days

## Security Considerations by Target Framework

### .NET 8.0+ and .NET 10.0
- Full support for all security features
- Native `System.Security.Cryptography` implementations
- Optimized `Span<T>` operations for memory safety
- Trimming support (IsTrimmable=true) for reduced attack surface

### .NET Standard 2.1
- Uses BouncyCastle for SHA-3
- Full `Span<T>` support for safe memory operations
- Compatible with modern .NET implementations

### .NET Standard 2.0
- Uses BouncyCastle for SHA-3
- Additional dependencies: `Microsoft.Bcl.HashCode`, `System.Text.Json`
- Limited `Span<T>` support (polyfills used where needed)
- Compatible with .NET Framework 4.6.1+

### Platform Security Notes

- **Windows**: Uses CNG (Cryptography Next Generation) for RNG
- **Linux/macOS**: Uses OpenSSL/dev/urandom for RNG
- **All Platforms**: SHA-3 via BouncyCastle (consistent implementation)

## Past Security Advisories

No security advisories have been published for this library.

When advisories are published, they will be available at:
- GitHub Security Advisories: https://github.com/visus-io/cuid.net/security/advisories
- NuGet Package Advisories

## Security Questions & Contact

### For Security Vulnerabilities
- **GitHub Security Advisories** (preferred): Repository Security tab
- **Email**: security@projects.visus.io

### For General Security Questions
- **Email**: security@projects.visus.io
- **GitHub Discussions**: For non-sensitive security questions

### Security Team Response Times
- Email responses: Within 48 hours (business days)
- GitHub Security Advisory responses: Within 48 hours

---

**Last updated:** 2026-01-10
**Policy version:** 2.0
