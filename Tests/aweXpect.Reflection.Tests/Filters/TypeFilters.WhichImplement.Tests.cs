using System;
using Xunit.Sdk;

namespace aweXpect.Reflection.Tests.Filters;

public sealed partial class TypeFilters
{
	public sealed class WhichImplement
	{
		public sealed class Tests
		{
			[Fact]
			public async Task ShouldApplyFilterForInterface()
			{
				async Task Act()
				{
					await That(In.AssemblyContaining<WhichImplement>().Types().WhichImplement<IFoo>())
						.AreNotAbstract();
				}

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task ShouldIncludeImplementInformationInErrorMessage()
			{
				async Task Act()
				{
					await That(In.AssemblyContaining<WhichImplement>().Types().WhichImplement<IFoo>())
						.AreAbstract();
				}

				await That(Act).ThrowsException()
					.WithMessage("""
					             Expected that types which implement TypeFilters.WhichImplement.Tests.IFoo in assembly containing type TypeFilters.WhichImplement
					             are all abstract,
					             but it contained non-abstract types [
					               TypeFilters.WhichImplement.Tests.FooImpl
					             ]
					             """);
			}

			[Fact]
			public async Task WhenInterfaceTypeIsAClass_ShouldThrowArgumentException()
			{
				async Task Act()
				{
					await That(In.AssemblyContaining<WhichImplement>().Types().WhichImplement(typeof(FooBase)))
						.AreNotAbstract();
				}

				await That(Act).Throws<ArgumentException>()
					.WithMessage(
						"The type to check implementation of must be an interface, but it was TypeFilters.WhichImplement.Tests.FooBase. Use 'InheritsFrom' to check for base-class inheritance.");
			}

			private interface IFoo;

			// ReSharper disable once UnusedType.Local
			private class FooImpl : IFoo;

			private class FooBase;
		}
	}
}
