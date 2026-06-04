using aweXpect.Reflection.Tests.TestHelpers.Types;

namespace aweXpect.Reflection.Tests;

public sealed partial class ThatType
{
	public sealed class HasOperator
	{
		public sealed class Tests
		{
			[Fact]
			public async Task WhenTypeHasTheOperator_ShouldSucceed()
			{
				Type subject = typeof(Money);

				async Task Act()
				{
					await That(subject).HasOperator(Operator.Addition);
				}

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenTypeDoesNotHaveTheOperator_ShouldFail()
			{
				Type subject = typeof(Money);

				async Task Act()
				{
					await That(subject).HasOperator(Operator.Modulus);
				}

				await That(Act).ThrowsException()
					.WithMessage($"""
					              Expected that subject
					              has the operator op_Modulus,
					              but it did not have the operator op_Modulus {Formatter.Format(subject)}
					              """);
			}

			[Fact]
			public async Task WhenTypeIsNull_ShouldFail()
			{
				Type? subject = null;

				async Task Act()
				{
					await That(subject).HasOperator(Operator.Addition);
				}

				await That(Act).ThrowsException()
					.WithMessage("""
					             Expected that subject
					             has the operator op_Addition,
					             but it was <null>
					             """);
			}

			[Fact]
			public async Task WhenOperatorIsOnlyInherited_WithoutInherit_ShouldFail()
			{
				Type subject = typeof(OperatorDerivedClass);

				async Task Act()
				{
					await That(subject).HasOperator(Operator.Subtraction);
				}

				await That(Act).ThrowsException()
					.WithMessage($"""
					              Expected that subject
					              has the operator op_Subtraction,
					              but it did not have the operator op_Subtraction {Formatter.Format(subject)}
					              """);
			}

			[Fact]
			public async Task WhenOperatorIsOnlyInherited_WithInherit_ShouldSucceed()
			{
				Type subject = typeof(OperatorDerivedClass);

				async Task Act()
				{
					await That(subject).HasOperator(Operator.Subtraction, inherit: true);
				}

				await That(Act).DoesNotThrow();
			}
		}

		public sealed class WithOperandTests
		{
			[Fact]
			public async Task WhenTypeHasTheOperatorWithGenericOperand_ShouldSucceed()
			{
				Type subject = typeof(Money);

				async Task Act()
				{
					await That(subject).HasOperator<int>(Operator.Addition);
				}

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenTypeHasTheOperatorWithTypeOperand_ShouldSucceed()
			{
				Type subject = typeof(Money);

				async Task Act()
				{
					await That(subject).HasOperator(Operator.Addition, typeof(Money));
				}

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenTypeDoesNotHaveTheOperatorWithOperand_ShouldFail()
			{
				Type subject = typeof(Money);

				async Task Act()
				{
					await That(subject).HasOperator<string>(Operator.Addition);
				}

				await That(Act).ThrowsException()
					.WithMessage($"""
					              Expected that subject
					              has the operator op_Addition with operand {Formatter.Format(typeof(string))},
					              but it did not have the operator op_Addition with operand {Formatter.Format(typeof(string))} {Formatter.Format(subject)}
					              """);
			}
		}

		public sealed class DoesNotHaveOperatorTests
		{
			[Fact]
			public async Task WhenTypeDoesNotHaveTheOperator_ShouldSucceed()
			{
				Type subject = typeof(Money);

				async Task Act()
				{
					await That(subject).DoesNotHaveOperator(Operator.Modulus);
				}

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenTypeHasTheOperator_ShouldFail()
			{
				Type subject = typeof(Money);

				async Task Act()
				{
					await That(subject).DoesNotHaveOperator(Operator.Addition);
				}

				await That(Act).ThrowsException()
					.WithMessage($"""
					              Expected that subject
					              does not have the operator op_Addition,
					              but it had the operator op_Addition {Formatter.Format(subject)}
					              """);
			}
		}

		public sealed class NegatedTests
		{
			[Fact]
			public async Task WhenTypeHasTheOperator_ShouldFail()
			{
				Type subject = typeof(Money);

				async Task Act()
				{
					await That(subject).DoesNotComplyWith(it => it.HasOperator(Operator.Addition));
				}

				await That(Act).ThrowsException()
					.WithMessage($"""
					              Expected that subject
					              does not have the operator op_Addition,
					              but it had the operator op_Addition {Formatter.Format(subject)}
					              """);
			}

			[Fact]
			public async Task WhenTypeDoesNotHaveTheOperator_ShouldSucceed()
			{
				Type subject = typeof(Money);

				async Task Act()
				{
					await That(subject).DoesNotComplyWith(it => it.HasOperator(Operator.Modulus));
				}

				await That(Act).DoesNotThrow();
			}
		}
	}
}
