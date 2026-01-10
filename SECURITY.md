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

Please do not report security vulnerabilities through public GitHub issues.

### 2. Report Privately

**Preferred:** Use GitHub Security Advisories:
- Navigate to the Security tab in the repository
- Click "Report a vulnerability"
- Provide detailed information about the issue

**Alternative:** Email the maintainers directly at security@projects.visus.io

### 3. Include These Details

- Type of vulnerability
- Affected versions
- Steps to reproduce
- Potential impact
- Suggested fix (if available)
- Your contact information for follow-up

### 4. Response Timeline

- **Initial Response:** Within 48 hours
- **Status Update:** Within 7 days
- **Fix Timeline:** Varies based on severity
  - **Critical:** 7-14 days
  - **High:** 30 days
  - **Medium/Low:** Next scheduled release

### 5. Disclosure Policy

- We request 90 days before public disclosure
- We will credit reporters in security advisories (unless you prefer to remain anonymous)
- We will coordinate disclosure timing with you

## Security Best Practices

### When Using This Library

1. **Use Cuid2 for security-sensitive applications**
   - Cuid2 uses SHA-3 (NIST FIPS-202 compliant)
   - Variable length support (4-32 characters)
   - Not sortable by design (prevents information leakage)

2. **Do not use CUIDs as secrets**
   - CUIDs are unique identifiers, not cryptographic keys
   - Use proper cryptographic libraries for secrets/tokens

3. **Consider your threat model**
   - For public-facing IDs in URLs: Cuid2 is appropriate
   - For session tokens or API keys: Use dedicated cryptographic libraries
   - For database primary keys: Either version works (prefer Cuid2)

4. **Keep dependencies updated**
   ```bash
   dotnet list package --outdated
   dotnet add package cuid.net
   ```

5. **Monitor for security advisories**
   - Watch this repository for security updates
   - Subscribe to GitHub Security Advisories
   - Check NuGet package advisories

## Cryptographic Dependencies

This library depends on:
- **BouncyCastle.Cryptography** for SHA-3 implementation (NIST FIPS-202 compliant)

We monitor these dependencies for security updates and will release patches as needed.

## Security Considerations by Target Framework

- **.NET 8.0+**: Full support for all security features
- **.NET Standard 2.0/2.1**: Uses BouncyCastle for cryptographic primitives
- **Trimming**: Library is trim-compatible (IsTrimmable=true) for .NET 8+

## Past Security Advisories

No security advisories have been published for this library.

## Contact

For security-related questions that are not vulnerabilities, you can:
- Email security@projects.visus.io

---

Last updated: 2025-01-10
