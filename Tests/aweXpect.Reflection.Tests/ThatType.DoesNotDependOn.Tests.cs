using aweXpect.Reflection.Tests.TestHelpers.Dependencies.Consumers;
using aweXpect.Reflection.Tests.TestHelpers.Dependencies.Layer1;
using aweXpect.Reflection.Tests.TestHelpers.Dependencies.Layer2;
using Xunit.Sdk;

namespace aweXpect.Reflection.Tests;

public sealed partial class ThatType
{
	public sealed class DoesNotDependOn
	{
		private const string Layer1Namespace = "aweXpect.Reflection.Tests.TestHelpers.Dependencies.Layer1";
		private const string Layer2Namespace = "aweXpect.Reflection.Tests.TestHelpers.Dependencies.Layer2";

		public sealed class Tests
		{
			[Fact]
			public async Task WhenTypeDoesNotDependOnNamespace_ShouldSucceed()
			{
				Type subject = typeof(OnlyLayer1);

				async Task Act()
					=> await That(subject).DoesNotDependOn(Layer2Namespace);

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenTypeDependsOnNamespace_ShouldFail()
			{
				Type subject = typeof(ViaField);

				async Task Act()
					=> await That(subject).DoesNotDependOn(Layer1Namespace);

				await That(Act).Throws<XunitException>()
					.WithMessage($"""
					              Expected that subject
					              does not depend on namespace "{Layer1Namespace}",
					              but it depended on ["{Layer1Namespace}"]
					              """);
			}

			[Fact]
			public async Task WhenOnlySystemReferenceIsImplicitBaseAndNullableAttributes_ShouldSucceed()
			{
				// OnlyLayer1's only authored dependency is Layer1.TargetA; the implicit object base type and the
				// compiler-emitted nullable attributes must not count as a dependency on "System".
				Type subject = typeof(OnlyLayer1);

				async Task Act()
					=> await That(subject).DoesNotDependOn("System");

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenNamingFrameworkNamespaceThatIsReferenced_ShouldFail()
			{
				Type subject = typeof(FrameworkConsumer);

				async Task Act()
					=> await That(subject).DoesNotDependOn("System.Collections.Generic");

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             does not depend on namespace "System.Collections.Generic",
					             but it depended on ["System.Collections.Generic"]
					             """);
			}

			[Fact]
			public async Task WhenTypeDoesNotReferenceSpecificType_ShouldSucceed()
			{
				Type subject = typeof(ViaField);

				async Task Act()
					=> await That(subject).DoesNotDependOn<TargetB>();

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenTypeReferencesSpecificType_ShouldFail()
			{
				Type subject = typeof(ViaField);

				async Task Act()
					=> await That(subject).DoesNotDependOn<TargetA>();

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             does not depend on type TargetA,
					             but it depended on [TargetA]
					             """);
			}

			[Fact]
			public async Task WhenNoNamespaceIsSpecified_ShouldThrowArgumentException()
			{
				Type subject = typeof(OnlyLayer1);

				async Task Act()
					=> await That(subject).DoesNotDependOn();

				await That(Act).Throws<ArgumentException>()
					.WithMessage("At least one namespace must be specified.");
			}

			[Fact]
			public async Task WhenWidenedWithOr_ShouldFailIfAnyMatches()
			{
				Type subject = typeof(Layer1AndLayer2);

				async Task Act()
					=> await That(subject).DoesNotDependOn(Layer2Namespace).Or(Layer1Namespace);

				await That(Act).Throws<XunitException>()
					.WithMessage($"""
					              Expected that subject
					              does not depend on namespace "{Layer2Namespace}" or "{Layer1Namespace}",
					              but it depended on ["{Layer1Namespace}", "{Layer2Namespace}"]
					              """);
			}
		}

		public sealed class NegatedTests
		{
			[Fact]
			public async Task WhenTypeDependsOnNamespace_ShouldSucceed()
			{
				Type subject = typeof(ViaField);

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.DoesNotDependOn(Layer1Namespace));

				await That(Act).DoesNotThrow();
			}
		}
	}
}
