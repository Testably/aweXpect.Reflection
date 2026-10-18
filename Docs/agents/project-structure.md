# Project Structure

## Repository layout

| Path | Contents |
|---|---|
| `Source/aweXpect.Reflection/` | the library, the only shipped package |
| `Tests/aweXpect.Reflection.Tests/` | TUnit tests of the public behaviour |
| `Tests/aweXpect.Reflection.Internal.Tests/` | TUnit tests of internal helpers and formatters (strong-named for `InternalsVisibleTo`) |
| `Tests/aweXpect.Reflection.Api.Tests/` | TUnit public API snapshot tests |
| `Tests/Aot/aweXpect.Reflection.Aot/` | console application published with Native AOT as a smoke test |
| `Benchmarks/aweXpect.Reflection.Benchmarks/` | BenchmarkDotNet benchmarks |
| `Pipeline/` | Fallout build targets (`Build.<Area>.cs`), run through `build.ps1`, `build.sh` or `build.cmd` |
| `Docs/pages/` | user documentation published to the docs site |
| `Docs/agents/` | guides for contributors and agents |
| `.github/workflows/` | CI workflows; `dependabot.yml` and `mergify.yml` sit next to them |

## How an expectation is built

User code follows four steps: pick a source, navigate to a member kind, filter, expect.

```csharp
await Expect.That(In.AssemblyContaining<MyClass>().Methods().WhichAreAsync())
    .HaveName("Async").AsSuffix();
```

- **Sources**: `In.cs` selects from concrete objects (`In.AllLoadedAssemblies()`, `In.AssemblyContaining<T>()`,
  `In.Type<T>()`, `In.Methods(...)`, ...). `Types.cs` selects types by criteria; its `In*` methods mirror the
  assembly sources of `In` and are mirrored again on `Filtered.Types.InNamespaceResult`, so a new assembly source
  has to be added to all three.
- **Filtered collections**: `Collections/Filtered.*.cs` define `Filtered.Assemblies`, `Filtered.Types`,
  `Filtered.Constructors`, `Filtered.Methods`, `Filtered.Properties`, `Filtered.Fields` and `Filtered.Events` on top
  of `Filtered<T, TFiltered>`. They are immutable: every filter returns a new instance. From `net8.0` on they are
  `IAsyncEnumerable<T>`, on `netstandard2.0` `IEnumerable<T>`. Navigation (`.Types()`, `.Methods()`, ...) and
  member scopes (`MemberScope`, `AccessModifiers`) live in the same folder. `Filter.cs` builds filters together with
  their description, which becomes part of the failure message.
- **Filters**: `Filters/<Kind>Filters.<Name>.cs` add extension methods on the filtered collections, e.g.
  `TypeFilters.WhichAreSealed`. Most filters come with a negated twin in the same file (`WhichAreNotSealed`).
- **Expectations**: `That<Kind>.<Name>.cs` extend `IThat<T?>` for a single subject (`ThatType`, `ThatMethod`, ...)
  and `That<Kinds>.<Name>.cs` extend `IThat<IEnumerable<T?>?>` for collections (`ThatTypes`, `ThatMethods`, ...).
  Both are partial static classes; the constraint is a private nested class in the same file, deriving from a
  `ConstraintResult` base of `aweXpect.Core`, and the negated expectation usually inverts the same constraint.
  `ThatMember` and `ThatMembers` hold expectations shared by all member kinds.
- **Results and options**: `Results/` holds result types that add chained options (e.g. `HasAttributeResult`,
  `TypeDependencyResult`); `Options/` holds the option objects they configure.

## Cross-cutting parts

- `Customization/`: `Customize.aweXpect.Reflection()` extensions for `IncludedCompilerGeneratedMembers()`,
  `IncludedSpecialNameMembers()`, `DependencyResolver()` and `ExcludedAttributeTypes()`. `ExcludedAssemblyPrefixes`
  comes from `aweXpect.Core`. User documentation: `Docs/pages/04-configuration.md`.
- `Formatting/`: members are formatted through the package-owned `FormattableMember` wrapper, whose formatter is
  registered by a module initializer (`FormatterRegistration`), so no formatter is registered for the reflection
  types themselves and the registration survives trimming.
- `Helpers/`: reflection helpers per kind (`TypeHelpers`, `MethodInfoHelpers`, `NullabilityHelpers`, ...), the
  dependency graph and violation renderers, `ThrowHelper` and `ReflectionFallbackHelpers`, which creates the failure
  when reflection is switched off after trimming.
- `Polyfills/` and `ModuleInitializerAttribute.cs`: attributes that `netstandard2.0` lacks.
- `Usings.cs`: global usings for `aweXpect.Formatting`.

## Tests

- Test files mirror the source files: `ThatType.IsSealed.cs` is tested in `ThatType.IsSealed.Tests.cs` (or
  `ThatType.IsNotSealed.Tests.cs` for the negation), filters in `Filters/`, collections in `Collections/`. Test
  classes nest like `ThatType.IsSealed.Tests`. Expectation tests are usually named `When<Condition>_Should<Outcome>`,
  filter tests `Should<Behaviour>`.
- Tests use TUnit (`[Test]`, `[Arguments(...)]`, `[MethodDataSource(...)]`). A failed expectation throws
  `FailException`, an inconclusive one TUnit's `InconclusiveTestException`. In files that declare their own nested
  `TestAttribute`, the test methods use `[TUnit.Core.Test]`, because `[Test]` would bind to the nested attribute.
- Failure messages are pinned with raw string literals in `.WithMessage(...)`.
- Sample types live in `TestHelpers/Types/`, dependency layers for the architecture-rule tests in
  `TestHelpers/Dependencies/`. `Examples/` contains usage examples (two of them skipped) and a tested Mono.Cecil dependency resolver.
- `net48` needs polyfills for newer language features, e.g. `TestHelpers/IsExternalInit.cs` for records and `init`.
