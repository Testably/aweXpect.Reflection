using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;

namespace aweXpect.Reflection.Tests;

public sealed partial class ThatConstructors
{
	public sealed class ParameterModifierExactlyOverloads
	{
		public sealed class Tests
		{
			[Fact]
			public async Task HaveRefParameterExactly_WhenAllMatch_ShouldSucceed()
			{
				IEnumerable<ConstructorInfo> constructors = new[]
				{
					Constructor<RefIntCtor>(),
				};

				async Task Act()
				{
					await That(constructors).HaveRefParameterExactly<int>();
				}

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task HaveRefParameterExactly_WhenNotAllMatch_ShouldFailWithExactTypeWording()
			{
				IEnumerable<ConstructorInfo> constructors = new[]
				{
					Constructor<RefIntCtor>(),
					Constructor<PlainIntCtor>(),
				};

				async Task Act()
				{
					await That(constructors).HaveRefParameterExactly<int>();
				}

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that constructors
					             all have parameter of exact type int with ref modifier,
					             but at least one did not
					             """);
			}

			[Fact]
			public async Task HaveRefParameterExactly_OfBaseType_ShouldNotMatchDerivedParameterType()
			{
				IEnumerable<ConstructorInfo> constructors = new[]
				{
					Constructor<RefMemoryStreamCtor>(),
				};

				async Task Act()
				{
					await That(constructors).HaveRefParameterExactly<Stream>();
				}

				await That(Act).Throws<XunitException>();
			}

			[Fact]
			public async Task HaveRefParameter_OfBaseType_ShouldMatchDerivedParameterType()
			{
				IEnumerable<ConstructorInfo> constructors = new[]
				{
					Constructor<RefMemoryStreamCtor>(),
				};

				async Task Act()
				{
					await That(constructors).HaveRefParameter<Stream>();
				}

				await That(Act).DoesNotThrow();
			}
		}

		private static ConstructorInfo Constructor<T>()
			=> typeof(T).GetConstructors().Single();

		// ReSharper disable UnusedParameter.Local
		// ReSharper disable UnusedMember.Local
		private class RefIntCtor
		{
			public RefIntCtor(ref int value)
			{
				value = 0;
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
		// ReSharper restore UnusedMember.Local
		// ReSharper restore UnusedParameter.Local
	}
}
