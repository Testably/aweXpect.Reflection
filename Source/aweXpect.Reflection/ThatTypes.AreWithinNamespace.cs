using System;
using System.Collections.Generic;
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

public static partial class ThatTypes
{
	/// <summary>
	///     Verifies that all items in the filtered collection of <see cref="Type" /> are within
	///     the <paramref name="expected" /> namespace (including sub-namespaces).
	/// </summary>
	public static StringEqualityResult<IEnumerable<Type?>, IThat<IEnumerable<Type?>>> AreWithinNamespace(
		this IThat<IEnumerable<Type?>> subject, string expected)
	{
		StringEqualityOptions options = new();
		return new StringEqualityResult<IEnumerable<Type?>, IThat<IEnumerable<Type?>>>(subject.Get()
				.ExpectationBuilder.AddConstraint<IEnumerable<Type?>>((it, grammars)
					=> new AreWithinNamespaceConstraint(it, grammars, expected, options)),
			subject,
			options);
	}

#if NET8_0_OR_GREATER
	/// <summary>
	///     Verifies that all items in the filtered collection of <see cref="Type" /> are within
	///     the <paramref name="expected" /> namespace (including sub-namespaces).
	/// </summary>
	public static StringEqualityResult<IAsyncEnumerable<Type?>, IThat<IAsyncEnumerable<Type?>>> AreWithinNamespace(
		this IThat<IAsyncEnumerable<Type?>> subject, string expected)
	{
		StringEqualityOptions options = new();
		return new StringEqualityResult<IAsyncEnumerable<Type?>, IThat<IAsyncEnumerable<Type?>>>(subject.Get()
				.ExpectationBuilder.AddConstraint<IAsyncEnumerable<Type?>>((it, grammars)
					=> new AreWithinNamespaceConstraint(it, grammars, expected, options)),
			subject,
			options);
	}
#endif

	private sealed class AreWithinNamespaceConstraint(
		string it,
		ExpectationGrammars grammars,
		string expected,
		StringEqualityOptions options)
		: CollectionConstraintResult<Type?>(grammars),
			IAsyncConstraint<IEnumerable<Type?>>
#if NET8_0_OR_GREATER
			, IAsyncConstraint<IAsyncEnumerable<Type?>>
#endif
	{
#if NET8_0_OR_GREATER
		public async Task<ConstraintResult> IsMetBy(IAsyncEnumerable<Type?> actual, CancellationToken cancellationToken)
			=> await SetAsyncValue(actual, type => type.IsWithinNamespace(expected, options));
#endif

		public async Task<ConstraintResult> IsMetBy(IEnumerable<Type?> actual, CancellationToken cancellationToken)
			=> await SetValue(actual, type => type.IsWithinNamespace(expected, options));

		protected override void AppendNormalExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append("all are within namespace ").Append(Formatter.Format(expected)).Append(options);

		protected override void AppendNormalResult(StringBuilder stringBuilder, string? indentation = null)
		{
			stringBuilder.Append(it).Append(" contained not matching types ");
			Formatter.Format(stringBuilder, NotMatching, FormattingOptions.Indented(indentation));
		}

		protected override void AppendNegatedExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append("not all are within namespace ").Append(Formatter.Format(expected)).Append(options);

		protected override void AppendNegatedResult(StringBuilder stringBuilder, string? indentation = null)
		{
			stringBuilder.Append(it).Append(" only contained matching types ");
			Formatter.Format(stringBuilder, Matching, FormattingOptions.Indented(indentation));
		}
	}
}
