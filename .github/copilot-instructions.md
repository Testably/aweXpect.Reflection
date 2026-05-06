# aweXpect.Reflection

**ALWAYS follow these instructions first and fallback to additional search and context gathering only if the information in these instructions is incomplete or found to be in error.**

aweXpect.Reflection is a .NET library providing reflection expectations for the [aweXpect](https://github.com/aweXpect/aweXpect) testing framework. It enables assertions on reflection types like `Assembly`, `Type`, `MethodInfo`, `PropertyInfo`, etc.

## Working Effectively

### Bootstrap and Build
Run these commands in order to set up the repository:

```bash
# CRITICAL: Unshallow repository for GitVersion (required for NUKE build)
git fetch --unshallow

# Cross-platform build commands:
# Linux/macOS:
./build.sh Compile
# Windows:
build.cmd Compile
# NEVER CANCEL: Initial SDK download takes 70+ seconds. Set timeout to 180+ seconds.
# NEVER CANCEL: Build takes 7+ seconds. Set timeout to 120+ seconds.
```

**Alternative direct build (bypasses GitVersion issues):**
```bash
# Use NUKE-installed SDK directly to avoid version conflicts
# Linux/macOS:
./.nuke/temp/dotnet-unix/dotnet build aweXpect.Reflection.slnx
# Windows:
.\.nuke\temp\dotnet-win\dotnet.exe build aweXpect.Reflection.slnx
# Takes ~7 seconds. Set timeout to 120+ seconds.
```

### Run Tests
```bash
# Run all tests using NUKE  
# Linux/macOS:
./build.sh UnitTests
# Windows:
build.cmd UnitTests
# NEVER CANCEL: Takes 5+ seconds. Set timeout to 60+ seconds.

# Alternative: Direct test execution (faster, .NET 8 only)
# Linux/macOS:
./.nuke/temp/dotnet-unix/dotnet test aweXpect.Reflection.slnx --framework net8.0 --no-build
# Windows:
.\.nuke\temp\dotnet-win\dotnet.exe test aweXpect.Reflection.slnx --framework net8.0 --no-build
# Takes ~4 seconds for 1067+ tests. Set timeout to 60+ seconds.
```

### Additional Build Targets
```bash
# API compatibility checks
./build.sh ApiChecks    # Linux/macOS
build.cmd ApiChecks     # Windows
# Takes ~2 seconds. Set timeout to 60+ seconds.

# Run benchmarks (performance testing)
./build.sh Benchmarks   # Linux/macOS
build.cmd Benchmarks    # Windows
# NEVER CANCEL: Takes 60+ seconds. Set timeout to 300+ seconds.

# Code coverage analysis
./build.sh CodeCoverage # Linux/macOS
build.cmd CodeCoverage  # Windows
# Takes ~15 seconds. Set timeout to 120+ seconds.

# Static code analysis (requires SONAR_TOKEN environment variable)
./build.sh CodeAnalysis # Linux/macOS
build.cmd CodeAnalysis  # Windows
# NEVER CANCEL: Takes 120+ seconds. Set timeout to 300+ seconds.

# Mutation testing (requires STRYKER_DASHBOARD_API_KEY environment variable)
./build.sh MutationTests # Linux/macOS
build.cmd MutationTests  # Windows
# NEVER CANCEL: Takes 300+ seconds. Set timeout to 600+ seconds.
```

## Validation Scenarios

**ALWAYS test these scenarios after making changes:**

1. **Build validation**: Run `./build.sh Compile` and verify successful completion
2. **Test validation**: Run `./build.sh UnitTests` and ensure all 1067+ tests pass
3. **API compatibility**: Run `./build.sh ApiChecks` to verify no breaking API changes
4. **Code quality**: Always run these before committing:
   - Build must pass without warnings
   - All unit tests must pass
   - API tests must pass for compatibility verification

### Quick Validation Workflow
```bash
# Complete validation sequence (Linux/macOS)
git fetch --unshallow && \
./build.sh Compile && \
./build.sh UnitTests && \
./build.sh ApiChecks

# Windows equivalent
git fetch --unshallow && build.cmd Compile && build.cmd UnitTests && build.cmd ApiChecks
```

### Individual Test Project Validation
```bash
# Test main library (1008 tests)
./.nuke/temp/dotnet-unix/dotnet test Tests/aweXpect.Reflection.Tests/aweXpect.Reflection.Tests.csproj --framework net8.0 --no-build

# Test internal APIs (57 tests) 
./.nuke/temp/dotnet-unix/dotnet test Tests/aweXpect.Reflection.Internal.Tests/aweXpect.Reflection.Internal.Tests.csproj --framework net8.0 --no-build

# Test API compatibility (2 tests)
./.nuke/temp/dotnet-unix/dotnet test Tests/aweXpect.Reflection.Api.Tests/aweXpect.Reflection.Api.Tests.csproj --framework net8.0 --no-build
```

## Common Issues and Troubleshooting

### GitVersion Failures
**Symptom**: `Could not inject value for Build.GitVersion` error  
**Solutions**:
1. Run `git fetch --unshallow` to get full git history (ignore "does not make sense" error if already unshallowed)
2. Use direct dotnet build: `./.nuke/temp/dotnet-unix/dotnet build aweXpect.Reflection.slnx`
3. Ensure you're on a valid branch (not detached HEAD)

### .NET SDK Version Issues  
**Symptom**: `A compatible .NET SDK was not found. Requested SDK version: 10.0.201`  
**Solution**: Use NUKE-installed SDK:
- Linux/macOS: `./.nuke/temp/dotnet-unix/dotnet` instead of system `dotnet`
- Windows: `.\.nuke\temp\dotnet-win\dotnet.exe` instead of system `dotnet`

### .NET Framework Test Failures on Linux
**Symptom**: `Could not find 'mono' host` when running net48 tests  
**Solution**: Expected behavior on Linux. Use `--framework net8.0` to run only .NET 8 tests.

### Build Timeouts
**Critical**: NEVER cancel builds or tests that appear to hang:
- Initial setup downloads .NET SDK (70+ seconds)
- Full builds take 35+ seconds
- Benchmarks take 60+ seconds  
- Mutation tests take 300+ seconds
- Always use appropriate timeout values (see command sections above)

## Project Structure

### Key Directories
- `Source/aweXpect.Reflection/` - Main library source code
- `Tests/aweXpect.Reflection.Tests/` - Primary unit tests (1000+ tests)
- `Tests/aweXpect.Reflection.Internal.Tests/` - Internal API tests
- `Tests/aweXpect.Reflection.Api.Tests/` - API compatibility tests  
- `Pipeline/` - NUKE build automation scripts
- `Benchmarks/aweXpect.Reflection.Benchmarks/` - Performance benchmarks
- `.github/workflows/` - CI/CD pipeline definitions

### Important Files
- `global.json` - Specifies required .NET SDK version (10.0.201)
- `aweXpect.Reflection.slnx` - Main solution file
- `build.sh` / `build.ps1` - Cross-platform build scripts
- `Pipeline/Build.cs` - NUKE build configuration

## Development Workflow

### Making Changes
1. **Always** run `git fetch --unshallow` in fresh clones
2. **Always** build and test before committing: 
   - Linux/macOS: `./build.sh Compile && ./build.sh UnitTests`
   - Windows: `build.cmd Compile && build.cmd UnitTests`
3. **Always** run API checks for library changes: `./build.sh ApiChecks` (or `build.cmd ApiChecks`)
4. For performance-sensitive changes, run benchmarks: `./build.sh Benchmarks` (or `build.cmd Benchmarks`)
5. **Never** update files in the root directory (e.g., `.gitignore`, `global.json`) unless specifically required for the issue being addressed

### Pull Request Title
To communicate intent to the consumers of your library, the title of the pull requests is prefixed with one of the following elements:
- `fix:`: patches a bug
- `feat:`: introduces a new feature
- `refactor:`: improves internal structure without changing the observable behavior
- `docs`: updates documentation or XML comments
- `chore`: updates to dependencies, build pipelines, ...

### Target Frameworks  
The library targets:
- .NET Standard 2.0 (for broad compatibility)
- .NET 8.0
- .NET 10.0 (for latest features)
- Tests also target .NET Framework 4.8 (Windows only)

### Test Categories
- **Unit Tests**: Core functionality testing (1000+ tests)
- **Internal Tests**: Implementation detail testing  
- **API Tests**: Public API surface validation
- **Benchmarks**: Performance regression testing

## CI/CD Pipeline
The repository uses GitHub Actions with multiple jobs:
- Unit tests on Ubuntu, Windows, macOS
- API compatibility checks
- Mutation testing for code quality
- Benchmarks for performance tracking
- Static code analysis via SonarCloud
- Automatic NuGet package publishing on tags

**Quality Gates**: 
- Code coverage > 90% required
- All SonarCloud issues must be resolved
- All tests must pass across all platforms
