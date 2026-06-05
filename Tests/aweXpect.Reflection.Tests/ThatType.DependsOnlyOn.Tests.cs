using aweXpect.Reflection.Tests.TestHelpers.Dependencies.Consumers;
using Xunit.Sdk;

namespace aweXpect.Reflection.Tests;

public sealed partial class ThatType
{
	public sealed class DependsOnlyOn
	{
		private const string Layer1Namespace = "aweXpect.Reflection.Tests.TestHelpers.Dependencies.Layer1";
		private const string Layer2Namespace = "aweXpect.Reflection.Tests.TestHelpers.Dependencies.Layer2";

		public sealed class Tests
		{
			[Fact]
			public async Task WhenAllDependenciesAreAllowed_ShouldSucceed()
			{
				Type subject = typeof(OnlyLayer1);

				async Task Act()
					=> await That(subject).DependsOnlyOn(Layer1Namespace);

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenDependingOnDisallowedNamespace_ShouldFail()
			{
				Type subject = typeof(Layer1AndLayer2);

				async Task Act()
					=> await That(subject).DependsOnlyOn(Layer1Namespace);

				await That(Act).Throws<XunitException>()
					.WithMessage($"""
					              Expected that subject
					              depends only on namespace "{Layer1Namespace}",
					              but it also depended on *{Layer2Namespace}*
					              """).AsWildcard();
			}

			[Fact]
			public async Task WhenAllowingMultipleNamespaces_ShouldSucceed()
			{
				Type subject = typeof(Layer1AndLayer2);

				async Task Act()
					=> await That(subject).DependsOnlyOn(Layer1Namespace, Layer2Namespace);

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenWidenedWithOr_ShouldSucceed()
			{
				Type subject = typeof(Layer1AndLayer2);

				async Task Act()
					=> await That(subject).DependsOnlyOn(Layer1Namespace).Or(Layer2Namespace);

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenDependingOnlyOnOwnNamespace_ShouldSucceed()
			{
				Type subject = typeof(ReferencesOwnNamespace);

				async Task Act()
					=> await That(subject).DependsOnlyOn(Layer1Namespace);

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenDependingOnlyOnFramework_ShouldSucceed()
			{
				Type subject = typeof(FrameworkConsumer);

				async Task Act()
					=> await That(subject).DependsOnlyOn(Layer1Namespace);

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenDependingOnGlobalNamespace_ShouldReportGlobalNamespace()
			{
				Type subject = typeof(ReferencesGlobal);

				async Task Act()
					=> await That(subject).DependsOnlyOn(Layer1Namespace);

				await That(Act).Throws<XunitException>()
					.WithMessage("*<global namespace>*").AsWildcard();
			}

			[Fact]
			public async Task WhenSubjectIsInGlobalNamespaceAndDependsOnlyOnFramework_ShouldSucceed()
			{
				Type subject = typeof(GlobalNamespaceTarget);

				async Task Act()
					=> await That(subject).DependsOnlyOn(Layer1Namespace);

				await That(Act).DoesNotThrow();
			}
		}

		public sealed class NegatedTests
		{
			[Fact]
			public async Task WhenDependingOnDisallowedNamespace_ShouldSucceed()
			{
				Type subject = typeof(Layer1AndLayer2);

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.DependsOnlyOn(Layer1Namespace));

				await That(Act).DoesNotThrow();
			}
		}
	}
}
