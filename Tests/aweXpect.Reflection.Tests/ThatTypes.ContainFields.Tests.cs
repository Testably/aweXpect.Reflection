using aweXpect.Reflection.Collections;
using Xunit.Sdk;

namespace aweXpect.Reflection.Tests;

public sealed partial class ThatTypes
{
	public sealed class ContainFields
	{
		public sealed class Tests
		{
			[Fact]
			public async Task WhenAllTypesContainMatchingField_ShouldSucceed()
			{
				Filtered.Types subject = In.Types<ClassWithMarkedField, ClassWithTwoMarkedFields>();

				async Task Act()
					=> await That(subject).ContainFields(fields => fields.With<MarkerAttribute>());

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenSomeTypeContainsNoMatchingField_ShouldFail()
			{
				Filtered.Types subject = In.Types<ClassWithMarkedField, ClassWithoutMarkedField>();

				async Task Act()
					=> await That(subject).ContainFields(fields => fields.With<MarkerAttribute>());

				await That(Act).ThrowsException()
					.WithMessage("""
					             Expected that in types ThatTypes.ContainFields.ClassWithMarkedField and ThatTypes.ContainFields.ClassWithoutMarkedField
					             all contain fields with ThatTypes.ContainFields.MarkerAttribute at least once,
					             but it contained not matching types [
					               ThatTypes.ContainFields.ClassWithoutMarkedField
					             ]
					             """);
			}
		}

		public sealed class NegatedTests
		{
			[Fact]
			public async Task WhenSomeTypeContainsNoMatchingField_ShouldSucceed()
			{
				Filtered.Types subject = In.Types<ClassWithMarkedField, ClassWithoutMarkedField>();

				async Task Act()
					=> await That(subject).DoesNotComplyWith(they
						=> they.ContainFields(fields => fields.With<MarkerAttribute>()));

				await That(Act).DoesNotThrow();
			}
		}

		[AttributeUsage(AttributeTargets.Field)]
		private class MarkerAttribute : Attribute
		{
		}

		private class ClassWithMarkedField
		{
			[Marker] public int Value;
		}

		private class ClassWithTwoMarkedFields
		{
			[Marker] public int First;

			[Marker] public int Second;
		}

		private class ClassWithoutMarkedField
		{
			public int Value;
		}
	}
}
