# aweXpect.Reflection Agent Guide

aweXpect.Reflection adds expectations and filters for reflection types (`Assembly`, `Type`, `ConstructorInfo`,
`MethodInfo`, `PropertyInfo`, `FieldInfo`, `EventInfo`) to [aweXpect](https://github.com/Testably/aweXpect).

## Setup

- Use the SDK pinned in `global.json` (`rollForward: latestPatch`). The tests also need the .NET 8 runtime.
- Shared settings live in `Directory.Build.props` (strong-name signing), `Source/Directory.Build.props` (target
  frameworks, nullable, warnings as errors, packing, trimming) and `Tests/Directory.Build.props`. Package versions
  are managed centrally in `Directory.Packages.props`.
- The build pipeline (`build.ps1`, `build.sh`, `build.cmd`) runs the Fallout targets in `Pipeline/`. It uses GitVersion, so it needs the full git history; a shallow clone fails.

## Design Goals

Correctness is a must. The other goals are strong defaults and win in this order when they conflict: stable and compatible, trimming friendly by failing loudly, a consistent and pleasant API, a fast happy path, extensible, errors at compile time, documented. [Docs/agents/design-goals.md](Docs/agents/design-goals.md) explains each goal, the code rules, the checks before a commit and the goals of this repository.

## Project Constraints

- The library targets `netstandard2.0`, `net8.0` and `net10.0`; the unit tests target `net8.0`, `net10.0` and
  `net48`.
  Guard newer APIs with `#if NET8_0_OR_GREATER` and keep the `netstandard2.0` build working.
- The only runtime dependency is `aweXpect.Core`. Do not add package dependencies to the library, and do not raise
  the `aweXpect.Core` version in `Directory.Packages.props` without need: it becomes the minimum version for every
  consumer. Test-only dependencies (e.g. `Mono.Cecil`) stay in the test projects.
- Every public type and member needs XML documentation (a missing one is warning CS1591). Changes to the public API are breaking unless additive and
  must show up in the API snapshots (see Validation).
- Keep trimming and Native AOT compatible. The library is `IsTrimmable` and `IsAotCompatible` from `net8.0` on.
  Every method that reflects over the members, interfaces, types or assembly references of a subject must check
  `ReflectionFallback.IsSupported` (from `aweXpect.Core`) itself, because the trim analyzer does not follow the
  check into a called method or a lambda, and throw `ReflectionFallbackHelpers.NotSupported(...)` when it is off.
  The analyzers only run for `net10.0`, as those of .NET 8 predate feature guards.
- Expectations funnel through the `aweXpect.Core` extensibility APIs (`ExpectationBuilder`, `ConstraintResult`);
  failure messages follow the aweXpect conventions ("Expected that subject ..., but ..."). Expectations fail for a
  null subject and carry `[GuaranteesNotNull]`; follow a neighbouring expectation.
- `.cs` files are UTF-8 with BOM (`.editorconfig`). PowerShell rewrites strip the BOM, so prefer editing tools and
  check `git diff` for a removed U+FEFF.
- `.cs` files use native line endings (`.gitattributes`): tests compare raw string literals against messages built
  with `Environment.NewLine`. Do not force `eol=crlf` and do not rewrite files with tools that change line endings.
- No em-dashes in new documentation or XML docs.

## Validation

- Before pushing, build without warnings and run the affected tests in `Tests/aweXpect.Reflection.Tests` and
  `Tests/aweXpect.Reflection.Internal.Tests`. `net48` only runs on Windows; use `--framework net10.0` for a quick run.
- Public API changes: run `Tests/aweXpect.Reflection.Api.Tests`. It compares the public API of every library target
  framework with the `ApiApprovalTests.*.awexpect-snapshot.txt` files. Accept an intentional change with the
  explicit `AcceptApiChanges` test, by renaming the `*.awexpect-snapshot.pending.txt` file, or by running the tests
  with `AWEXPECT_ACCEPT_SNAPSHOTS=all`. Commit the updated snapshots; never commit `*.pending.*` files.
- Reflection or trimming changes: run the AOT smoke tests (`./build.ps1 AotSmokeTests`). They run
  `Tests/Aot/aweXpect.Reflection.Aot` from the compiled output and again published with Native AOT, where trimmer
  and AOT warnings are errors.
- Always pass a target to `build.ps1`/`build.sh`: without one it runs `ApiChecks`, `Benchmarks` and `CodeAnalysis`.
- Mutation tests (Stryker) and benchmarks run in CI on every pull request; only run them locally when asked.

## Git Workflow

- Work on a feature branch and open a pull request against `main`. Do not commit to `main` directly.
- Pull request titles follow [conventional commits](https://www.conventionalcommits.org/): `feat:`, `fix:`,
  `refactor:`, `docs:`, `test:`, `ci:`, `chore:`; add `!` for a breaking change (e.g. `refactor!:`).
- Squash-merged commit messages: the subject is the pull request title, the body explains why the change is needed
  (the diff shows what changed). Write each paragraph on a single line. No co-author or attribution trailers.
- Do not touch root files (`global.json`, `.gitignore`, `.editorconfig`, ...) unless the change requires it.
- Dependabot pull requests are squash-merged automatically by Mergify once unit and API tests pass.

## References

Read these only when the task needs them:

- [Docs/agents/design-goals.md](Docs/agents/design-goals.md): the goals and code rules for every change, the checks before a commit and the goals of this repository.
- [Docs/agents/workflows.md](Docs/agents/workflows.md): build, test, benchmark, mutation and documentation
  commands, pipeline targets and CI workflows.
- [Docs/agents/project-structure.md](Docs/agents/project-structure.md): architecture and source locations.
- [CONTRIBUTING.md](CONTRIBUTING.md): quality gates for contributions.
