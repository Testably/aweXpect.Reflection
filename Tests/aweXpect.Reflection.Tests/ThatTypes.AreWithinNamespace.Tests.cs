using aweXpect.Reflection.Collections;
using Xunit.Sdk;

namespace aweXpect.Reflection.Tests;

public sealed partial class ThatTypes
{
	public sealed class AreWithinNamespace
	{
		public sealed class Tests
		{
			[Fact]
			public async Task WhenTypesAreWithinNamespace_ShouldSucceed()
			{
				Filtered.Types subject = In.AssemblyContaining<Tests>()
					.Types().WithinNamespace("aweXpect.Reflection.Tests.TestHelpers.Types.NamespaceScope");

				async Task Act()
				{
					await That(subject)
						.AreWithinNamespace("aweXpect.Reflection.Tests.TestHelpers.Types.NamespaceScope");
				}

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenTypesContainTypeInOtherNamespace_ShouldFail()
			{
				Filtered.Types subject = In.AssemblyContaining<Tests>()
					.Types().WithNamespace("aweXpect.Reflection.Tests.TestHelpers.Types.NamespaceScope")
					.AsPrefix();

				async Task Act()
				{
					await That(subject)
						.AreWithinNamespace("aweXpect.Reflection.Tests.TestHelpers.Types.NamespaceScope");
				}

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that types with namespace starting with "aweXpect.Reflection.Tests.Test…" in assembly containing type ThatTypes.AreWithinNamespace.Tests
					             all are within namespace "aweXpect.Reflection.Tests.TestHelpers.Types.NamespaceScope",
					             but it contained not matching types [
					               *ClassInSiblingNamespaceScope*
					             ]
					             """).AsWildcard();
			}
		}

		public sealed class NegatedTests
		{
			[Fact]
			public async Task WhenTypesAreWithinNamespace_ShouldFail()
			{
				Filtered.Types subject = In.AssemblyContaining<Tests>()
					.Types().WithinNamespace("aweXpect.Reflection.Tests.TestHelpers.Types.NamespaceScope");

				async Task Act()
				{
					await That(subject).DoesNotComplyWith(they
						=> they.AreWithinNamespace("aweXpect.Reflection.Tests.TestHelpers.Types.NamespaceScope"));
				}

				await That(Act).Throws<XunitException>()
					.WithMessage("*not all are within namespace*NamespaceScope*").AsWildcard();
			}
		}
	}
}
