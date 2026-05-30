using System.Reflection;
using aweXpect.Reflection.Tests.TestHelpers.Types;
using Xunit.Sdk;

namespace aweXpect.Reflection.Tests;

public sealed partial class ThatAssembly
{
	public sealed class HasVersion
	{
		public sealed class Tests
		{
			[Fact]
			public async Task WhenVersionDoesNotMatch_ShouldFail()
			{
				Assembly assembly = typeof(PublicAbstractClass).Assembly;

				async Task Act()
				{
					await That(assembly).HasVersion(version => version.Major < 0);
				}

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that assembly
					             has version matching version => version.Major < 0,
					             but it had version *
					             """).AsWildcard();
			}

			[Fact]
			public async Task WhenVersionMatches_ShouldSucceed()
			{
				Assembly assembly = typeof(PublicAbstractClass).Assembly;

				async Task Act()
				{
					await That(assembly).HasVersion(version => version.Major >= 0);
				}

				await That(Act).DoesNotThrow();
			}
		}

		public sealed class ComponentTests
		{
			[Fact]
			public async Task ShouldSupportAllComponents()
			{
				Assembly assembly = typeof(PublicAbstractClass).Assembly;

				async Task Act()
				{
					await That(assembly).HasVersion()
						.WithMajor.GreaterThanOrEqualTo(0)
						.WithMinor.GreaterThanOrEqualTo(0)
						.WithBuild.GreaterThanOrEqualTo(0)
						.WithRevision.GreaterThanOrEqualTo(0);
				}

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenAllComparisonsAreSatisfied_ShouldSucceed()
			{
				Assembly assembly = typeof(PublicAbstractClass).Assembly;

				async Task Act()
				{
					await That(assembly).HasVersion()
						.WithMajor.GreaterThan(0)
						.WithMajor.GreaterThanOrEqualTo(1)
						.WithMajor.LessThan(2)
						.WithMajor.LessThanOrEqualTo(1)
						.WithMajor.EqualTo(1)
						.WithMajor.NotEqualTo(0);
				}

				await That(Act).DoesNotThrow();
			}

			[Theory]
			[InlineData("GreaterThan", 5, "greater than 5")]
			[InlineData("GreaterThanOrEqualTo", 5, "greater than or equal to 5")]
			[InlineData("LessThan", 0, "less than 0")]
			[InlineData("LessThanOrEqualTo", 0, "less than or equal to 0")]
			[InlineData("EqualTo", 5, "equal to 5")]
			[InlineData("NotEqualTo", 1, "not equal to 1")]
			public async Task WhenComparisonFails_MessageShouldDescribeComparison(
				string comparison, int expected, string expectedText)
			{
				Assembly assembly = typeof(PublicAbstractClass).Assembly;

				async Task Act()
				{
					await (comparison switch
					{
						"GreaterThan" => That(assembly).HasVersion().WithMajor.GreaterThan(expected),
						"GreaterThanOrEqualTo" => That(assembly).HasVersion().WithMajor.GreaterThanOrEqualTo(expected),
						"LessThan" => That(assembly).HasVersion().WithMajor.LessThan(expected),
						"LessThanOrEqualTo" => That(assembly).HasVersion().WithMajor.LessThanOrEqualTo(expected),
						"EqualTo" => That(assembly).HasVersion().WithMajor.EqualTo(expected),
						_ => That(assembly).HasVersion().WithMajor.NotEqualTo(expected),
					});
				}

				await That(Act).Throws<XunitException>()
					.WithMessage($"*has major version {expectedText}*but it had major version 1*").AsWildcard();
			}

			[Theory]
			[InlineData("GreaterThan", 1)]
			[InlineData("GreaterThanOrEqualTo", 2)]
			[InlineData("LessThan", 1)]
			[InlineData("LessThanOrEqualTo", 0)]
			[InlineData("EqualTo", 2)]
			[InlineData("NotEqualTo", 1)]
			public async Task WhenComparisonIsNotSatisfied_ShouldFail(string comparison, int expected)
			{
				Assembly assembly = typeof(PublicAbstractClass).Assembly;

				async Task Act()
				{
					await (comparison switch
					{
						"GreaterThan" => That(assembly).HasVersion().WithMajor.GreaterThan(expected),
						"GreaterThanOrEqualTo" => That(assembly).HasVersion().WithMajor.GreaterThanOrEqualTo(expected),
						"LessThan" => That(assembly).HasVersion().WithMajor.LessThan(expected),
						"LessThanOrEqualTo" => That(assembly).HasVersion().WithMajor.LessThanOrEqualTo(expected),
						"EqualTo" => That(assembly).HasVersion().WithMajor.EqualTo(expected),
						_ => That(assembly).HasVersion().WithMajor.NotEqualTo(expected),
					});
				}

				await That(Act).Throws<XunitException>();
			}

			[Theory]
			[InlineData("GreaterThan", 0)]
			[InlineData("GreaterThanOrEqualTo", 1)]
			[InlineData("LessThan", 2)]
			[InlineData("LessThanOrEqualTo", 1)]
			[InlineData("EqualTo", 1)]
			[InlineData("NotEqualTo", 0)]
			public async Task WhenComparisonIsSatisfied_ShouldSucceed(string comparison, int expected)
			{
				Assembly assembly = typeof(PublicAbstractClass).Assembly;

				async Task Act()
				{
					await (comparison switch
					{
						"GreaterThan" => That(assembly).HasVersion().WithMajor.GreaterThan(expected),
						"GreaterThanOrEqualTo" => That(assembly).HasVersion().WithMajor.GreaterThanOrEqualTo(expected),
						"LessThan" => That(assembly).HasVersion().WithMajor.LessThan(expected),
						"LessThanOrEqualTo" => That(assembly).HasVersion().WithMajor.LessThanOrEqualTo(expected),
						"EqualTo" => That(assembly).HasVersion().WithMajor.EqualTo(expected),
						_ => That(assembly).HasVersion().WithMajor.NotEqualTo(expected),
					});
				}

				await That(Act).DoesNotThrow();
			}

			[Theory]
			[InlineData("major", "major version", "1")]
			[InlineData("minor", "minor version", "0")]
			[InlineData("build", "build version", "0")]
			[InlineData("revision", "revision version", "0")]
			public async Task WhenComponentDoesNotMatch_MessageShouldNameComponent(
				string name, string text, string actual)
			{
				Assembly assembly = typeof(PublicAbstractClass).Assembly;

				async Task Act()
				{
					await (name switch
					{
						"major" => That(assembly).HasVersion().WithMajor.EqualTo(99),
						"minor" => That(assembly).HasVersion().WithMinor.EqualTo(99),
						"build" => That(assembly).HasVersion().WithBuild.EqualTo(99),
						_ => That(assembly).HasVersion().WithRevision.EqualTo(99),
					});
				}

				await That(Act).Throws<XunitException>()
					.WithMessage($"*has {text} equal to 99*but it had {text} {actual}*").AsWildcard();
			}

			[Fact]
			public async Task WhenComponentDoesNotMatch_ShouldFail()
			{
				Assembly assembly = typeof(PublicAbstractClass).Assembly;

				async Task Act()
				{
					await That(assembly).HasVersion().WithMajor.LessThan(0);
				}

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that assembly
					             has major version less than 0,
					             but it had major version *
					             """).AsWildcard();
			}

			[Fact]
			public async Task WhenComponentMatches_ShouldSucceed()
			{
				Assembly assembly = typeof(PublicAbstractClass).Assembly;

				async Task Act()
				{
					await That(assembly).HasVersion().WithMajor.GreaterThanOrEqualTo(0);
				}

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenMultipleComponentsAreCombined_ShouldReportFailingComponent()
			{
				Assembly assembly = typeof(PublicAbstractClass).Assembly;

				async Task Act()
				{
					await That(assembly).HasVersion().WithMajor.GreaterThan(0).WithMinor.GreaterThan(0);
				}

				await That(Act).Throws<XunitException>()
					.WithMessage(
						"*has major version greater than 0 and minor version greater than 0*but it had minor version 0*")
					.AsWildcard();
			}

			[Fact]
			public async Task WithoutComponent_ShouldVerifyThatVersionExists()
			{
				Assembly assembly = typeof(PublicAbstractClass).Assembly;

				async Task Act()
				{
					await That(assembly).HasVersion();
				}

				await That(Act).DoesNotThrow();
			}
		}

		public sealed class NegatedComponentTests
		{
			[Fact]
			public async Task WhenComponentDoesNotMatch_ShouldSucceed()
			{
				Assembly assembly = typeof(PublicAbstractClass).Assembly;

				async Task Act()
				{
					await That(assembly).DoesNotComplyWith(it => it.HasVersion().WithMajor.GreaterThan(1));
				}

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenComponentMatches_ShouldFail()
			{
				Assembly assembly = typeof(PublicAbstractClass).Assembly;

				async Task Act()
				{
					await That(assembly).DoesNotComplyWith(it => it.HasVersion().WithMajor.GreaterThan(0));
				}

				await That(Act).Throws<XunitException>()
					.WithMessage("*does not have major version greater than 0*").AsWildcard();
			}

			[Fact]
			public async Task WhenVersionExists_ShouldFail()
			{
				Assembly assembly = typeof(PublicAbstractClass).Assembly;

				async Task Act()
				{
					await That(assembly).DoesNotComplyWith(it => it.HasVersion());
				}

				await That(Act).Throws<XunitException>()
					.WithMessage("*does not have a version*").AsWildcard();
			}
		}
	}
}
