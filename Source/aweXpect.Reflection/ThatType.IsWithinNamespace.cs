using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using aweXpect.Core;
using aweXpect.Core.Constraints;
using aweXpect.Options;
using aweXpect.Reflection.Helpers;
using aweXpect.Results;

// ReSharper disable PossibleMultipleEnumeration

namespace aweXpect.Reflection;

public static partial class ThatType
{
	/// <summary>
	///     Verifies that the <see cref="Type" /> is within the <paramref name="expected" /> namespace
	///     (including sub-namespaces).
	/// </summary>
	public static StringEqualityResult<Type?, IThat<Type?>> IsWithinNamespace(
		this IThat<Type?> subject, string expected)
	{
		StringEqualityOptions options = new();
		return new StringEqualityResult<Type?, IThat<Type?>>(subject.Get().ExpectationBuilder.AddConstraint(
				(it, grammars)
					=> new IsWithinNamespaceConstraint(it, grammars, expected, options)),
			subject,
			options);
	}

	private sealed class IsWithinNamespaceConstraint(
		string it,
		ExpectationGrammars grammars,
		string expected,
		StringEqualityOptions options)
		: ConstraintResult.WithNotNullValue<Type?>(it, grammars),
			IAsyncConstraint<Type?>
	{
		public async Task<ConstraintResult> IsMetBy(Type? actual, CancellationToken cancellationToken)
		{
			Actual = actual;
			Outcome = await actual.IsWithinNamespace(expected, options) ? Outcome.Success : Outcome.Failure;
			return this;
		}

		protected override void AppendNormalExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append("is within namespace ").Append(Formatter.Format(expected)).Append(options);

		protected override void AppendNormalResult(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append(It).Append(" was in namespace ").Append(Formatter.Format(Actual?.Namespace));

		protected override void AppendNegatedExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append("is not within namespace ").Append(Formatter.Format(expected)).Append(options);

		protected override void AppendNegatedResult(StringBuilder stringBuilder, string? indentation = null)
			=> AppendNormalResult(stringBuilder, indentation);
	}
}
