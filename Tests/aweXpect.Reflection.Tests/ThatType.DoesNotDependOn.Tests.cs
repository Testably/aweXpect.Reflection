using System.Collections.Generic;
using aweXpect.Reflection.Tests.TestHelpers.Dependencies.Consumers;
using aweXpect.Reflection.Tests.TestHelpers.Dependencies.Layer1;
using aweXpect.Reflection.Tests.TestHelpers.Dependencies.Layer2;
using aweXpect.Reflection.Tests.TestHelpers.Dependencies.Synthetic;
using Xunit.Sdk;

// These tests intentionally exercise the non-generic DoesNotDependOn(Type) overload (closed generic
// constructions and array targets), so the "prefer generic overload" hint does not apply here.
#pragma warning disable CA2263

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
			public async Task WhenCompilerEmitsAttributesForRequiredMember_ShouldNotCountAsDependency()
			{
				// `required` makes the compiler emit [RequiredMember] on the type and property and
				// [Obsolete] + [CompilerFeatureRequired] on the constructor; none of these are authored.
				Type subject = typeof(WithRequiredProperty);

				async Task Act()
					=> await That(subject).DoesNotDependOn("System");

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenCompilerEmitsAttributesForAsyncMethod_ShouldNotCountAsDependency()
			{
				// An async method makes the compiler emit [AsyncStateMachine(typeof(<M>d__0))] and
				// [DebuggerStepThrough]; neither attribute is authored. (The authored `void` return type
				// still counts as a reference to "System", so the attribute namespaces are asserted.)
				Type subject = typeof(WithAsyncMethod);

				async Task Act()
				{
					await That(subject).DoesNotDependOn("System.Runtime.CompilerServices");
					await That(subject).DoesNotDependOn("System.Diagnostics");
				}

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenCompilerEmitsStateMachineForAsyncMethod_ShouldNotCountAsDependency()
			{
				// The nested <MethodAsync>d__0 state machine lives in the type's own namespace; the
				// typeof(...) argument of [AsyncStateMachine] must not surface it as a dependency.
				Type subject = typeof(WithAsyncMethod);

				async Task Act()
					=> await That(subject)
						.DoesNotDependOn("aweXpect.Reflection.Tests.TestHelpers.Dependencies.Synthetic");

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenCompilerEmitsAttributesForIteratorMethod_ShouldNotCountAsDependency()
			{
				// An iterator method makes the compiler emit [IteratorStateMachine(typeof(<M>d__0))];
				// the authored IEnumerable<int> return type lives in System.Collections.Generic, not here.
				Type subject = typeof(WithIteratorMethod);

				async Task Act()
					=> await That(subject).DoesNotDependOn("System.Runtime.CompilerServices");

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenInterfaceIsImplementedOnlyByBaseType_ShouldNotCountAsDependency()
			{
				// GetInterfaces() returns the transitive closure: DerivedWithoutOwnReferences inherits
				// ITargetInterface from its base type without writing the reference itself.
				Type subject = typeof(DerivedWithoutOwnReferences);

				async Task Act()
					=> await That(subject).DoesNotDependOn(Layer1Namespace);

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenRecordSynthesizesEquatableInterface_ShouldNotCountAsDependency()
			{
				// The compiler synthesizes IEquatable<T> for records; the author never wrote that reference.
				Type subject = typeof(RecordWithLayer1Target);

				async Task Act()
					=> await That(subject).DoesNotDependOn("System");

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenDelegateInfrastructureIsRuntimeSupplied_ShouldNotCountAsDependency()
			{
				// The MulticastDelegate base, the (object, IntPtr) constructor and BeginInvoke/EndInvoke
				// are runtime-supplied; only the Invoke signature of a delegate is authored.
				Type subject = typeof(TargetProviderDelegate);

				async Task Act()
					=> await That(subject).DoesNotDependOn("System");

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenOnlyOtherConstructionOfGenericIsReferenced_ShouldSucceed()
			{
				// ViaGenericArgument references List<TargetA>; List<TargetB> is a different construction
				// and must not be reported as a dependency.
				Type subject = typeof(ViaGenericArgument);

				async Task Act()
					=> await That(subject).DoesNotDependOn(typeof(List<TargetB>));

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenArrayTargetIsUsed_ShouldMatchElementType()
			{
				// The array wrapper is unwrapped on both sides: WithArrayField references TargetA[],
				// so it depends on typeof(TargetA[]) just like on typeof(TargetA).
				Type subject = typeof(WithArrayField);

				async Task Act()
					=> await That(subject).DoesNotDependOn(typeof(TargetA[]));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             does not depend on type TargetA[],
					             but it depended on [TargetA]
					             """);
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
