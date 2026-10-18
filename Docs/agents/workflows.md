# Workflows

Commands run from the repository root. Examples use PowerShell; on Linux and macOS replace `.\build.ps1` with
`./build.sh`.

## Build

```powershell
dotnet build aweXpect.Reflection.slnx
```

The solution contains the library, the test projects, the AOT smoke test and the benchmarks. `Pipeline/Build.csproj`
is listed but not built with the solution. Building the library also packs it (`GeneratePackageOnBuild`).

## Test

```powershell
# One test project, one framework
dotnet test Tests/aweXpect.Reflection.Tests/aweXpect.Reflection.Tests.csproj --framework net10.0

# Filter to a class or method (xunit v2 on VSTest, so --filter, not --treenode-filter)
dotnet test Tests/aweXpect.Reflection.Tests/aweXpect.Reflection.Tests.csproj --framework net10.0 `
    --filter "FullyQualifiedName~aweXpect.Reflection.Tests.ThatAssemblies+AreStrongNamed"

# Tests of internal helpers (the library grants InternalsVisibleTo to this project)
dotnet test Tests/aweXpect.Reflection.Internal.Tests/aweXpect.Reflection.Internal.Tests.csproj --framework net10.0
```

- `net48` runs only on Windows. The pipeline skips it elsewhere.
- Tests nest the expectation in classes: `ThatAssemblies.AreStrongNamed.Tests`, so the fully qualified name uses `+`
  between nested classes.

## Public API

```powershell
dotnet test Tests/aweXpect.Reflection.Api.Tests/aweXpect.Reflection.Api.Tests.csproj
```

The project (NUnit, `net8.0`) builds every target framework of the library and compares its public API with
`ApiApprovalTests.aweXpect%2EReflection.<framework>.awexpect-snapshot.txt` using `aweXpect.Snapshots.PublicApi`.
A mismatch writes a `*.awexpect-snapshot.pending.txt` file next to the snapshot. To accept an intentional change:

- rename the pending file to replace the snapshot, or
- run the explicit `AcceptApiChanges` test, or
- run the tests with the environment variable `AWEXPECT_ACCEPT_SNAPSHOTS` set to `all`.

The tests read the build output of the same configuration (`Debug` or `Release`) as their own.

## AOT smoke tests

```powershell
.\build.ps1 AotSmokeTests

# Only the compiled output, without publishing
dotnet run --project Tests/Aot/aweXpect.Reflection.Aot/aweXpect.Reflection.Aot.csproj
```

`Tests/Aot/aweXpect.Reflection.Aot` is a console application, not a test project. Each check in `Checks.cs` states
the result under the JIT; the published executable must reproduce it or fail with a message naming the switch
`aweXpect.ReflectionFallback.IsSupported`. The pipeline target runs the compiled output, publishes with Native AOT
for the current runtime into `Artifacts/Aot/` and runs the executable; a non-zero exit code fails the target.
Publishing needs the platform's native toolchain for Native AOT.

## Benchmarks

```powershell
dotnet run -c Release --project Benchmarks/aweXpect.Reflection.Benchmarks -- --filter *
```

The project uses `BenchmarkSwitcher`, so `--filter` selects benchmarks. `.\build.ps1 Benchmarks` runs all of them and
prints the report of `HappyCaseBenchmarks`. Compare before and after a change on the same machine.

## Mutation tests

`.\build.ps1 MutationTests` installs `dotnet-stryker` into `TestResults/dotnet-stryker`, mutates
`Source/aweXpect.Reflection` against `aweXpect.Reflection.Tests` and `aweXpect.Reflection.Internal.Tests` on `net10.0`
with mutation level `Advanced`, and writes reports to `Artifacts/Stryker`. Off `main` it only mutates code changed
since `main`. It takes long; run it only when asked.

## Documentation

- `README.md` is the package readme and is included in `Docs/pages/00-index.md` through `{README}`. The `Pack` target
  writes a copy with release badges to `Artifacts/README.md`, which is packed instead.
- `Docs/pages/` is published to the docs site at https://docs.testably.org/Extensions/aweXpect.Reflection: after a
  successful `Build` workflow on `main`, `notify-docs-site.yml` triggers a rebuild of `Testably/Testably.Site`.
  Update these pages together with behaviour changes. Do not put contributor or agent notes there.
- `Docs/agents/` holds these guides; they are not published.

## Pipeline targets

`.\build.ps1 <Target>` builds `Pipeline/Build.csproj` and runs the target. `.\build.ps1 --help` lists them. Without a
target, `ApiChecks`, `Benchmarks` and `CodeAnalysis` run. The configuration defaults to `Debug` locally and `Release`
on a server (`--configuration`).

| Target | Depends on | Does |
|---|---|---|
| `Compile` | `Restore` (cleans `Artifacts/` and `TestResults/`), `CalculateNugetVersion` | builds the solution with the GitVersion version |
| `UnitTests` | `Compile` | runs `aweXpect.Reflection.Tests` and `aweXpect.Reflection.Internal.Tests` for each framework with coverage and `.trx` logs in `TestResults/` |
| `CodeCoverage` | `UnitTests` | merges the coverage into `TestResults/reports/OpenCover.xml` |
| `ApiChecks` | `Compile` | runs `aweXpect.Reflection.Api.Tests` |
| `AotSmokeTests` | `Compile` | runs and publishes the AOT smoke test |
| `CodeAnalysis` | `Compile`, `CodeCoverage` | SonarCloud analysis; needs `SonarToken` |
| `Benchmarks` | `BenchmarkDotNet`, `BenchmarkResult` | runs the benchmarks into `Artifacts/Benchmarks` |
| `BenchmarkComment` | | CI only: posts the benchmark report on the pull request |
| `MutationTests` | `MutationTestExecution`, `MutationComment` | runs Stryker and prepares the pull request comment |
| `MutationTestDashboard` | | CI only: uploads the report to the Stryker dashboard and comments on the pull request |
| `Pack` | `UpdateReadme`, `Compile` | moves the `.nupkg` and `.snupkg` files to `Artifacts/Packages` |

## CI workflows

| Workflow | Trigger | Jobs |
|---|---|---|
| `ci.yml` (CI) | pull requests to `main` | unit tests with coverage on Ubuntu, Windows and macOS; AOT smoke tests on all three; API tests; mutation tests; benchmarks; SonarCloud (not for Dependabot or forks); test result publishing |
| `ci-analysis.yml` | after CI completes | uploads mutation results and posts mutation and benchmark comments with the artifacts of the CI run |
| `build.yml` (Build) | pushes to `main`, `v*` tags | the CI jobs plus `Pack`; on a tag, pushes the packages to nuget.org, creates the GitHub release for stable versions and comments on the released issues |
| `notify-docs-site.yml` | successful Build on `main` | triggers the docs site rebuild |

Mergify squash-merges Dependabot pull requests once the unit tests on all three systems and the API tests pass.
