using System.IO;
using System.Linq;
using System.Reflection;
using aweXpect.Reflection.Collections;

namespace aweXpect.Reflection.Tests.Filters;

public sealed partial class ConstructorFilters
{
	public sealed class ParameterModifierExactlyOverloads
	{
		public sealed class Tests
		{
			[Fact]
			public async Task WithRefParameterExactly_ShouldFilterAndDescribe()
			{
				Filtered.Constructors constructors = In.AssemblyContaining<AssemblyFilters>()
					.Constructors().WithRefParameterExactly<int>();

				await That(constructors).Contains(Constructor<RefIntCtor>());
				await That(constructors).DoesNotContain(Constructor<PlainIntCtor>());
				await That(constructors).DoesNotContain(Constructor<OutIntCtor>());
				await That(constructors.GetDescription())
					.IsEqualTo("constructors with parameter of exact type int and with ref modifier in assembly")
					.AsPrefix();
			}

			[Fact]
			public async Task WithRefParameterExactly_OfBaseType_ShouldNotMatchDerivedParameterType()
			{
				Filtered.Constructors constructors = In.AssemblyContaining<AssemblyFilters>()
					.Constructors().WithRefParameterExactly<Stream>();

				await That(constructors).DoesNotContain(Constructor<RefMemoryStreamCtor>());
			}

			[Fact]
			public async Task WithRefParameter_OfBaseType_ShouldMatchDerivedParameterType()
			{
				Filtered.Constructors constructors = In.AssemblyContaining<AssemblyFilters>()
					.Constructors().WithRefParameter<Stream>();

				await That(constructors).Contains(Constructor<RefMemoryStreamCtor>());
			}

			[Fact]
			public async Task WithRefParameterExactly_OfExactType_ShouldMatchParameterType()
			{
				Filtered.Constructors constructors = In.AssemblyContaining<AssemblyFilters>()
					.Constructors().WithRefParameterExactly<MemoryStream>();

				await That(constructors).Contains(Constructor<RefMemoryStreamCtor>());
			}

			[Fact]
			public async Task WithOptionalParameterExactly_ShouldFilter()
			{
				Filtered.Constructors constructors = In.AssemblyContaining<AssemblyFilters>()
					.Constructors().WithOptionalParameterExactly<int>();

				await That(constructors).Contains(Constructor<OptionalIntCtor>());
				await That(constructors).DoesNotContain(Constructor<PlainIntCtor>());
			}

			[Fact]
			public async Task WithParamsParameterExactly_ShouldFilter()
			{
				Filtered.Constructors constructors = In.AssemblyContaining<AssemblyFilters>()
					.Constructors().WithParamsParameterExactly<int[]>();

				await That(constructors).Contains(Constructor<ParamsIntCtor>());
			}
		}

		private static ConstructorInfo? Constructor<T>()
			=> typeof(T).GetConstructors().Single();

		// ReSharper disable UnusedMember.Local
		// ReSharper disable UnusedParameter.Local
		private class RefIntCtor
		{
			public RefIntCtor(ref int value)
			{
				value = 0;
			}
		}

		private class OutIntCtor
		{
			public OutIntCtor(out int value)
			{
				value = 0;
			}
		}

		private class OptionalIntCtor
		{
			public OptionalIntCtor(int value = 0)
			{
			}
		}

		private class ParamsIntCtor
		{
			public ParamsIntCtor(params int[] values)
			{
			}
		}

		private class PlainIntCtor
		{
			public PlainIntCtor(int value)
			{
			}
		}

		private class RefMemoryStreamCtor
		{
			public RefMemoryStreamCtor(ref MemoryStream value)
			{
				value = null!;
			}
		}
		// ReSharper restore UnusedParameter.Local
		// ReSharper restore UnusedMember.Local
	}
}
