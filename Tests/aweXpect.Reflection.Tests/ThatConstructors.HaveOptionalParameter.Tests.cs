using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;

namespace aweXpect.Reflection.Tests;

public sealed partial class ThatConstructors
{
	public sealed class HaveOptionalParameter
	{
		public sealed class Tests
		{
			[Fact]
			public async Task WhenAllHaveOptionalParameter_ShouldSucceed()
			{
				IEnumerable<ConstructorInfo> constructors = new[]
				{
					typeof(ClassWithOptionalParameter).GetConstructors().Single(),
					typeof(AnotherClassWithOptionalParameter).GetConstructors().Single(),
				};

				async Task Act()
				{
					await That(constructors).HaveOptionalParameter();
				}

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenNotAllHaveOptionalParameter_ShouldFail()
			{
				IEnumerable<ConstructorInfo> constructors = new[]
				{
					typeof(ClassWithOptionalParameter).GetConstructors().Single(),
					typeof(ClassWithoutModifiers).GetConstructors().Single(),
				};

				async Task Act()
				{
					await That(constructors).HaveOptionalParameter();
				}

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that constructors
					             all have an optional parameter,
					             but it contained constructors without an optional parameter *
					             """).AsWildcard();
			}

			[Fact]
			public async Task ByType_WhenAllHaveOptionalParameter_ShouldSucceed()
			{
				IEnumerable<ConstructorInfo> constructors = new[]
				{
					typeof(ClassWithOptionalParameter).GetConstructors().Single(),
				};

				async Task Act()
				{
					await That(constructors).HaveOptionalParameter(typeof(int));
				}

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task ByType_WhenNotAllHaveOptionalParameter_ShouldFail()
			{
				IEnumerable<ConstructorInfo> constructors = new[]
				{
					typeof(ClassWithOptionalParameter).GetConstructors().Single(),
					typeof(ClassWithoutModifiers).GetConstructors().Single(),
				};

				async Task Act()
				{
					await That(constructors).HaveOptionalParameter(typeof(int));
				}

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that constructors
					             all have parameter of type int with optional modifier,
					             but at least one did not
					             """);
			}

			[Fact]
			public async Task ByTypeAndName_WhenAllHaveOptionalParameter_ShouldSucceed()
			{
				IEnumerable<ConstructorInfo> constructors = new[]
				{
					typeof(ClassWithOptionalParameter).GetConstructors().Single(),
				};

				async Task Act()
				{
					await That(constructors).HaveOptionalParameter(typeof(int), "value");
				}

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task ByTypeExactly_WhenAllHaveOptionalParameter_ShouldSucceed()
			{
				IEnumerable<ConstructorInfo> constructors = new[]
				{
					typeof(ClassWithOptionalParameter).GetConstructors().Single(),
				};

				async Task Act()
				{
					await That(constructors).HaveOptionalParameterExactly(typeof(int));
				}

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task ByTypeExactlyAndName_WhenAllHaveOptionalParameter_ShouldSucceed()
			{
				IEnumerable<ConstructorInfo> constructors = new[]
				{
					typeof(ClassWithOptionalParameter).GetConstructors().Single(),
				};

				async Task Act()
				{
					await That(constructors).HaveOptionalParameterExactly(typeof(int), "value");
				}

				await That(Act).DoesNotThrow();
			}
		}

		public sealed class NegatedTests
		{
			[Fact]
			public async Task WhenAllHaveOptionalParameter_ShouldFail()
			{
				IEnumerable<ConstructorInfo> constructors = new[]
				{
					typeof(ClassWithOptionalParameter).GetConstructors().Single(),
					typeof(AnotherClassWithOptionalParameter).GetConstructors().Single(),
				};

				async Task Act()
				{
					await That(constructors).DoesNotComplyWith(they => they.HaveOptionalParameter());
				}

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that constructors
					             not all have an optional parameter,
					             but it only contained constructors with an optional parameter *
					             """).AsWildcard();
			}

			[Fact]
			public async Task WhenNotAllHaveOptionalParameter_ShouldSucceed()
			{
				IEnumerable<ConstructorInfo> constructors = new[]
				{
					typeof(ClassWithOptionalParameter).GetConstructors().Single(),
					typeof(ClassWithoutModifiers).GetConstructors().Single(),
				};

				async Task Act()
				{
					await That(constructors).DoesNotComplyWith(they => they.HaveOptionalParameter());
				}

				await That(Act).DoesNotThrow();
			}
		}

		// ReSharper disable UnusedParameter.Local
		// ReSharper disable UnusedMember.Local
		private class ClassWithOptionalParameter
		{
			public ClassWithOptionalParameter(int value = 0)
			{
			}
		}

		private class AnotherClassWithOptionalParameter
		{
			public AnotherClassWithOptionalParameter(string text = "")
			{
			}
		}

		private class ClassWithoutModifiers
		{
			public ClassWithoutModifiers(int value)
			{
			}
		}
		// ReSharper restore UnusedMember.Local
		// ReSharper restore UnusedParameter.Local
	}
}
