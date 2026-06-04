using aweXpect.Customization;
using aweXpect.Reflection.Collections;
using aweXpect.Reflection.Tests.TestHelpers;
using aweXpect.Reflection.Tests.TestHelpers.Types;

namespace aweXpect.Reflection.Tests.Filters;

public sealed partial class MethodFilters
{
	public sealed class WhichAreOperators
	{
		public sealed class Tests
		{
			[Fact]
			public async Task ShouldAllowFilteringForOperators()
			{
				using (Customize.aweXpect.Reflection().IncludedSpecialNameMembers()
					       .Set(SpecialNameMembers.Operators))
				{
					Filtered.Methods methods = In.AssemblyContaining<MethodFilters>()
						.Types().Methods().WhichAreOperators();

					await That(methods).All().Satisfy(x => x.IsReallyOperator()).And.IsNotEmpty();
					await That(methods.GetDescription())
						.IsEqualTo("operator methods in types in assembly").AsPrefix();
				}
			}
		}

		public sealed class WithOperatorTests
		{
			[Fact]
			public async Task ShouldAllowFilteringForSpecificOperatorWithoutCustomization()
			{
				Filtered.Methods methods = In.Type<ClassWithOperators>()
					.Methods().WhichAreOperators(Operator.Addition);

				await That(methods).All().Satisfy(x => x!.Name == "op_Addition").And.IsNotEmpty();
				await That(methods.GetDescription())
					.IsEqualTo("op_Addition operator methods in").AsPrefix();
			}

			[Fact]
			public async Task ShouldNotMatchADifferentOperator()
			{
				Filtered.Methods methods = In.Type<ClassWithOperators>()
					.Methods().WhichAreOperators(Operator.Modulus);

				await That(methods).IsEmpty();
			}
		}
	}
}
