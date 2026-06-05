using System.Collections.Generic;
using aweXpect.Reflection.Tests.TestHelpers.Dependencies.Consumers;
using aweXpect.Reflection.Tests.TestHelpers.Dependencies.Layer1;
using aweXpect.Reflection.Tests.TestHelpers.Dependencies.Layer2;
using aweXpect.Reflection.Tests.TestHelpers.Dependencies.Synthetic;
using Xunit.Sdk;

// These tests intentionally exercise the non-generic DependsOn(Type) overload (open/closed generic
// constructions and array targets), so the "prefer generic overload" hint does not apply here.
#pragma warning disable CA2263

namespace aweXpect.Reflection.Tests;

public sealed partial class ThatType
{
	public sealed class DependsOn
	{
		private const string Layer1Namespace = "aweXpect.Reflection.Tests.TestHelpers.Dependencies.Layer1";
		private const string Layer2Namespace = "aweXpect.Reflection.Tests.TestHelpers.Dependencies.Layer2";

		public sealed class Tests
		{
			[Theory]
			[InlineData(typeof(ViaBaseType))]
			[InlineData(typeof(ViaInterface))]
			[InlineData(typeof(ViaField))]
			[InlineData(typeof(ViaProperty))]
			[InlineData(typeof(ViaIndexer))]
			[InlineData(typeof(ViaEvent))]
			[InlineData(typeof(ViaMethodParameter))]
			[InlineData(typeof(ViaMethodReturn))]
			[InlineData(typeof(ViaGenericArgument))]
			[InlineData(typeof(ViaAttribute))]
			[InlineData(typeof(ViaGenericConstraint<>))]
			public async Task WhenTypeReferencesNamespaceInSignature_ShouldSucceed(Type subject)
			{
				async Task Act()
					=> await That(subject).DependsOn(Layer1Namespace);

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenTypeDoesNotDependOnNamespace_ShouldFail()
			{
				Type subject = typeof(OnlyLayer1);

				async Task Act()
					=> await That(subject).DependsOn(Layer2Namespace);

				await That(Act).Throws<XunitException>()
					.WithMessage($"""
					              Expected that subject
					              depends on namespace "{Layer2Namespace}",
					              but it depended on ["{Layer1Namespace}"]
					              """);
			}

			[Fact]
			public async Task WhenDelegateSignatureReferencesNamespace_ShouldSucceed()
			{
				// The authored Invoke signature (return type Layer1.TargetA, parameter Layer2.TargetB)
				// still counts as a dependency of the delegate type.
				Type subject = typeof(TargetProviderDelegate);

				async Task Act()
				{
					await That(subject).DependsOn(Layer1Namespace);
					await That(subject).DependsOn(Layer2Namespace);
				}

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenSubNamespaceMatchesViaSubtree_ShouldSucceed()
			{
				Type subject = typeof(ViaSubNamespace);

				async Task Act()
					=> await That(subject).DependsOn(Layer1Namespace);

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenExcludingSubNamespaces_ShouldNotMatchSubNamespace()
			{
				Type subject = typeof(ViaSubNamespace);

				async Task Act()
					=> await That(subject).DependsOn(Layer1Namespace).ExcludingSubNamespaces();

				await That(Act).Throws<XunitException>();
			}

			[Fact]
			public async Task WhenAnyOfMultipleNamespacesMatches_ShouldSucceed()
			{
				Type subject = typeof(OnlyLayer1);

				async Task Act()
					=> await That(subject).DependsOn(Layer2Namespace, Layer1Namespace);

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenWidenedWithOrOn_ShouldSucceed()
			{
				Type subject = typeof(OnlyLayer1);

				async Task Act()
					=> await That(subject).DependsOn(Layer2Namespace).OrOn(Layer1Namespace);

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenNamingFrameworkNamespace_ShouldSucceed()
			{
				Type subject = typeof(FrameworkConsumer);

				async Task Act()
					=> await That(subject).DependsOn("System.Collections.Generic");

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenTypeReferencesSpecificType_ShouldSucceed()
			{
				Type subject = typeof(ViaField);

				async Task Act()
					=> await That(subject).DependsOn<TargetA>();

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenTypeReferencesTypeOnlyViaAttributeArgument_ShouldSucceed()
			{
				Type subject = typeof(ViaAttributeArgument);

				async Task Act()
					=> await That(subject).DependsOn<TargetB>();

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenTypeReferencesEnumOnlyViaAttributeArgument_ShouldSucceed()
			{
				// Layer2's TargetSeverity is referenced ONLY through the attribute's enum argument; the
				// attribute type itself lives in Layer1.
				Type subject = typeof(ViaEnumAttributeArgument);

				async Task Act()
					=> await That(subject).DependsOn(Layer2Namespace);

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenTypeReferencesNamespaceOnlyViaParameterAttribute_ShouldSucceed()
			{
				// Layer1's TargetAttribute is referenced ONLY through the attribute on a method parameter.
				Type subject = typeof(ViaParameterAttribute);

				async Task Act()
					=> await That(subject).DependsOn(Layer1Namespace);

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenTypeReferencesNamespaceOnlyViaReturnValueAttribute_ShouldSucceed()
			{
				// Layer1's TargetAttribute is referenced ONLY through the [return: ...] attribute.
				Type subject = typeof(ViaReturnValueAttribute);

				async Task Act()
					=> await That(subject).DependsOn(Layer1Namespace);

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenDebuggerStepThroughIsAuthoredOnAsyncMethod_ShouldCountAsDependency()
			{
				// In Debug builds the authored [DebuggerStepThrough] appears next to the compiler-emitted
				// one (two entries); in Release builds it is the only occurrence in an optimized assembly —
				// in both cases it is recognized as authored and counts.
				Type subject = typeof(WithAuthoredDebuggerStepThroughAsyncMethod);

				async Task Act()
					=> await That(subject).DependsOn("System.Diagnostics");

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenTypeDoesNotReferenceSpecificType_ShouldFail()
			{
				Type subject = typeof(ViaField);

				async Task Act()
					=> await That(subject).DependsOn<TargetB>();

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             depends on type TargetB,
					             but it did not
					             """);
			}

			[Fact]
			public async Task WhenTargetingGlobalNamespaceWithEmptyString_ShouldSucceed()
			{
				// An empty string targets exactly the global namespace.
				Type subject = typeof(ReferencesGlobal);

				async Task Act()
					=> await That(subject).DependsOn("");

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenExactGenericConstructionIsReferenced_ShouldSucceed()
			{
				Type subject = typeof(ViaGenericArgument);

				async Task Act()
					=> await That(subject).DependsOn(typeof(List<TargetA>));

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenGenericTypeDefinitionIsTargeted_ShouldMatchAnyConstruction()
			{
				Type subject = typeof(ViaGenericArgument);

				async Task Act()
					=> await That(subject).DependsOn(typeof(List<>));

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenOnlyOtherConstructionOfGenericIsReferenced_ShouldFail()
			{
				// ViaGenericArgument references List<TargetA>; List<TargetB> is a different construction.
				Type subject = typeof(ViaGenericArgument);

				async Task Act()
					=> await That(subject).DependsOn(typeof(List<TargetB>));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             depends on type List<TargetB>,
					             but it did not
					             """);
			}

			[Fact]
			public async Task WhenArrayTargetIsUsed_ShouldMatchElementType()
			{
				// Dependencies are collected with array wrappers stripped, so an array target is
				// unwrapped symmetrically: typeof(TargetA[]) matches like typeof(TargetA).
				Type subject = typeof(WithArrayField);

				async Task Act()
				{
					await That(subject).DependsOn(typeof(TargetA[]));
					await That(subject).DependsOn(typeof(TargetA));
				}

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenWidenedWithOrOnType_ShouldSucceed()
			{
				Type subject = typeof(ViaField);

				async Task Act()
					=> await That(subject).DependsOn<TargetB>().OrOn<TargetA>();

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenTypeIsNull_ShouldFail()
			{
				Type? subject = null;

				async Task Act()
					=> await That(subject).DependsOn(Layer1Namespace);

				await That(Act).ThrowsException()
					.WithMessage($"""
					              Expected that subject
					              depends on namespace "{Layer1Namespace}",
					              but it was <null>
					              """);
			}

			[Fact]
			public async Task WhenNoNamespaceIsSpecified_ShouldThrowArgumentException()
			{
				Type subject = typeof(OnlyLayer1);

				async Task Act()
					=> await That(subject).DependsOn();

				await That(Act).Throws<ArgumentException>()
					.WithMessage("At least one namespace must be specified.");
			}
		}

		public sealed class NegatedTests
		{
			[Fact]
			public async Task WhenTypeDependsOnNamespace_ShouldFail()
			{
				Type subject = typeof(ViaField);

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.DependsOn(Layer1Namespace));

				await That(Act).Throws<XunitException>()
					.WithMessage($"""
					              Expected that subject
					              does not depend on namespace "{Layer1Namespace}",
					              but it depended on ["{Layer1Namespace}"]
					              """);
			}
		}
	}
}
