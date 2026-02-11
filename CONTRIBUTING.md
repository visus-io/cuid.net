# Contributing to cuid.net

Thank you for your interest in contributing to cuid.net! We welcome contributions from the community and are grateful for your support.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Setting Up Your Development Environment](#setting-up-your-development-environment)
- [How to Contribute](#how-to-contribute)
  - [Reporting Bugs](#reporting-bugs)
  - [Suggesting Features](#suggesting-features)
  - [Improving Documentation](#improving-documentation)
  - [Submitting Code Changes](#submitting-code-changes)
- [Development Workflow](#development-workflow)
  - [Building the Project](#building-the-project)
  - [Running Tests](#running-tests)
  - [Code Quality Checks](#code-quality-checks)
- [Coding Standards](#coding-standards)
- [Pull Request Process](#pull-request-process)
- [Versioning and Releases](#versioning-and-releases)
- [Need Help?](#need-help)

## Code of Conduct

This project adheres to the [Contributor Covenant Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Please report unacceptable behavior to [admin@projects.visus.io](mailto:admin@projects.visus.io).

## Getting Started

### Prerequisites

To contribute to cuid.net, you'll need:

- **.NET SDK 10.0 or later** (specified in `global.json`)
- **Windows OS** (recommended for multi-targeting support)
  - While development is possible on macOS/Linux, CI runs on Windows to support all target frameworks
- **Git** for version control
- A code editor or IDE:
  - [Visual Studio 2022](https://visualstudio.microsoft.com/) (recommended for Windows)
  - [JetBrains Rider](https://www.jetbrains.com/rider/)
  - [Visual Studio Code](https://code.visualstudio.com/) with C# extension

### Setting Up Your Development Environment

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/YOUR-USERNAME/cuid.net.git
   cd cuid.net
   ```
3. **Add the upstream remote**:
   ```bash
   git remote add upstream https://github.com/visus-io/cuid.net.git
   ```
4. **Restore dependencies**:
   ```bash
   dotnet restore
   ```
5. **Build the solution**:
   ```bash
   dotnet build
   ```
6. **Run tests** to verify your setup:
   ```bash
   dotnet test
   ```

If all tests pass, you're ready to start contributing!

## How to Contribute

### Reporting Bugs

If you find a bug, please [create a bug report](https://github.com/visus-io/cuid.net/issues/new?template=bug_report.yml) with:

- A clear, descriptive title
- Steps to reproduce the issue
- Expected behavior vs actual behavior
- Your environment (.NET version, OS, framework target)
- Any relevant code samples or error messages

### Suggesting Features

We welcome feature suggestions! Please [create a feature request](https://github.com/visus-io/cuid.net/issues/new?template=feature_request.yml) with:

- A clear description of the feature
- The problem it solves or use case it addresses
- Any alternative solutions you've considered
- Whether you'd be willing to implement it

### Improving Documentation

Documentation improvements are always welcome! This includes:

- Fixing typos or clarifying existing documentation
- Adding code examples
- Improving API documentation (XML comments)
- Updating the README.md or CLAUDE.md

For documentation-only changes, you can submit a pull request directly.

### Submitting Code Changes

1. **Check existing issues** to see if your change is already being discussed
2. **Create or comment on an issue** before starting significant work
3. **Follow the development workflow** outlined below
4. **Submit a pull request** following our PR guidelines

## Development Workflow

### Building the Project

```bash
# Clean build
dotnet clean
dotnet build

# Release build
dotnet build -c Release

# Build for specific framework
dotnet build --framework net8.0
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests for specific framework
dotnet test --framework net8.0
dotnet test --framework net10.0

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run tests with coverage (CI format)
dotnet test -c Release \
  --logger:trx \
  --results-directory ./TestResults \
  -- --coverage --coverage-output-format xml --report-trx
```

**Important notes about tests:**

- Tests use **TUnit** as the test framework
- API surface tests use **Verify.TUnit** for snapshot testing
- If you make intentional API changes, you may need to regenerate snapshots
- Tests must pass for all target frameworks: `net48`, `net8.0`, `net10.0`

### Code Quality Checks

Before submitting your changes:

1. **Ensure the solution builds without errors:**
   ```bash
   dotnet build -c Release
   ```

2. **Run all tests:**
   ```bash
   dotnet test -c Release
   ```

3. **Check for compiler warnings:**
   - Your changes should not introduce new warnings
   - Use `#pragma warning disable` sparingly and with justification

4. **Verify API surface changes** (if applicable):
   - API surface tests will detect breaking changes
   - Breaking changes require strong justification and major version bump

## Coding Standards

### Code Style

- Follow the `.editorconfig` settings in the repository
- Use **C# 14** language features where appropriate
- Write **clear, self-documenting code** with meaningful names
- Add **XML documentation comments** for all public APIs
- Keep methods focused and concise (Single Responsibility Principle)

### Multi-Targeting Considerations

This project targets multiple frameworks:
- `netstandard2.0`
- `netstandard2.1`
- `net8.0`
- `net10.0`

When contributing:

- Use **conditional compilation** (`#if NETSTANDARD`) when necessary
- Test on multiple frameworks when possible
- Be mindful of API availability across frameworks
- Avoid dependencies not available on all target frameworks

### Structural Guidelines

- **Use readonly structs** for performance-critical types
- **Implement appropriate interfaces** (`IEquatable<T>`, `IComparable<T>`, etc.)
- **Keep dependencies minimal** and aligned with project goals
- **Add guards** using `CommunityToolkit.Diagnostics.Guard` for parameter validation
- **Centralize messages** for consistency (see `Obsoletions.cs` pattern)

### Testing Guidelines

- Write **unit tests** for all new functionality
- Maintain or **improve code coverage**
- Use **descriptive test names** that explain what is being tested
- Follow the **Arrange-Act-Assert** pattern
- Test **edge cases** and **error conditions**
- Use **snapshot tests** (Verify) for API surface validation

## Pull Request Process

1. **Create a feature branch** from `main`:
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes** following the coding standards

3. **Write or update tests** for your changes

4. **Commit your changes** with clear, descriptive commit messages:
   ```bash
   git commit -m "feat: add support for custom length validation"
   ```

   Follow [Conventional Commits](https://www.conventionalcommits.org/) format:
   - `feat:` for new features
   - `fix:` for bug fixes
   - `docs:` for documentation changes
   - `test:` for test additions/changes
   - `refactor:` for code refactoring
   - `perf:` for performance improvements
   - `chore:` for maintenance tasks

5. **Push to your fork**:
   ```bash
   git push origin feature/your-feature-name
   ```

6. **Create a Pull Request** on GitHub:
   - Use the provided PR template
   - Fill in all relevant sections
   - Link related issues
   - Ensure all CI checks pass
   - Request review from maintainers

7. **Respond to feedback**:
   - Address reviewer comments promptly
   - Push additional commits if changes are requested
   - Keep the discussion focused and professional

8. **After approval**:
   - A maintainer will merge your PR
   - Your contribution will be included in the next release

### Pull Request Checklist

Before submitting, ensure:

- [ ] Code builds without errors
- [ ] All tests pass on all target frameworks
- [ ] No new compiler warnings
- [ ] Added/updated unit tests
- [ ] Code coverage maintained or improved
- [ ] Updated documentation (README.md, XML comments, CLAUDE.md if needed)
- [ ] API surface changes validated with snapshot tests (if applicable)
- [ ] Breaking changes documented and justified (if applicable)
- [ ] Followed coding standards and style guidelines
- [ ] Commit messages are clear and follow conventions

## Versioning and Releases

- This project uses **MinVer** for semantic versioning based on Git tags
- Versions follow [Semantic Versioning 2.0.0](https://semver.org/):
  - **MAJOR**: Breaking changes
  - **MINOR**: New features (backward compatible)
  - **PATCH**: Bug fixes (backward compatible)
- Releases are automated via GitHub Actions
- Only maintainers can create releases by pushing tags

## Need Help?

If you have questions or need assistance:

1. **Check existing issues** and discussions
2. **Review the documentation**:
   - [README.md](README.md) - Project overview and usage
   - [CLAUDE.md](CLAUDE.md) - Detailed architecture and development guide
   - [SECURITY.md](SECURITY.md) - Security policy and reporting
3. **Ask questions** by [opening an issue](https://github.com/visus-io/cuid.net/issues/new?template=config.yml)
4. **Reach out** to maintainers via [GitHub Discussions](https://github.com/visus-io/cuid.net/discussions) (if enabled)

## Recognition

All contributors will be recognized in the project. Thank you for helping make cuid.net better!

---

**By contributing to cuid.net, you agree that your contributions will be licensed under the same license as the project ([MIT License](LICENSE)).**
