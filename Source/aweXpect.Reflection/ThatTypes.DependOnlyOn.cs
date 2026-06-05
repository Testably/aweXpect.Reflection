using System;
using System.Collections.Generic;
using System.Text;
using aweXpect.Core;
using aweXpect.Core.Constraints;
using aweXpect.Customization;
using aweXpect.Reflection.Helpers;
using aweXpect.Reflection.Options;
using aweXpect.Reflection.Results;
#if NET8_0_OR_GREATER
using System.Threading;
using System.Threading.Tasks;
#endif

// ReSharper disable PossibleMultipleEnumeration

namespace aweXpect.Reflection;

public static partial class ThatTypes
{
	/// <summary>
	///     Verifies that all items in the filtered collection of <see cref="Type" /> depend on (reference in their
	///     signature) only types in the <paramref name="namespaces" /> (including sub-namespaces), their own namespace
	///     or framework assemblies.
	/// </summary>
	/// <remarks>
	///     Dependencies on types whose assembly name starts with one of the
	///     <see cref="AwexpectCustomization.ReflectionCustomizationValue.ExcludedAssemblyPrefixes" /> are ignored, so
	///     that framework namespaces do not have to be listed explicitly.
	/// </remarks>
	public static NamespaceDependencyResult<IEnumerable<Type?>> DependOnlyOn(
		this IThat<IEnumerable<Type?>> subject, params IEnumerable<string> namespaces)
	{
		NamespaceDependencyOptions options = new(namespaces);
		return new NamespaceDependencyResult<IEnumerable<Type?>>(subject.Get().ExpectationBuilder
				.AddConstraint<IEnumerable<Type?>>((it, grammars)
					=> new DependOnlyOnConstraint(it, grammars, options)),
			subject,
			options);
	}

#if NET8_0_OR_GREATER
	/// <summary>
	///     Verifies that all items in the filtered collection of <see cref="Type" /> depend on (reference in their
	///     signature) only types in the <paramref name="namespaces" /> (including sub-namespaces), their own namespace
	///     or framework assemblies.
	/// </summary>
	/// <remarks>
	///     Dependencies on types whose assembly name starts with one of the
	///     <see cref="AwexpectCustomization.ReflectionCustomizationValue.ExcludedAssemblyPrefixes" /> are ignored, so
	///     that framework namespaces do not have to be listed explicitly.
	/// </remarks>
	public static NamespaceDependencyResult<IAsyncEnumerable<Type?>> DependOnlyOn(
		this IThat<IAsyncEnumerable<Type?>> subject, params IEnumerable<string> namespaces)
	{
		NamespaceDependencyOptions options = new(namespaces);
		return new NamespaceDependencyResult<IAsyncEnumerable<Type?>>(subject.Get().ExpectationBuilder
				.AddConstraint<IAsyncEnumerable<Type?>>((it, grammars)
					=> new DependOnlyOnConstraint(it, grammars, options)),
			subject,
			options);
	}
#endif

	private sealed class DependOnlyOnConstraint(
		string it,
		ExpectationGrammars grammars,
		NamespaceDependencyOptions options)
		: CollectionConstraintResult<Type?>(grammars),
			IValueConstraint<IEnumerable<Type?>>
#if NET8_0_OR_GREATER
			, IAsyncConstraint<IAsyncEnumerable<Type?>>
#endif
	{
		private readonly Dictionary<Type, IReadOnlyList<string>> _violations = new();

		private bool DependsOnlyOnAllowed(Type? type)
		{
			if (type is null)
			{
				return false;
			}

			IReadOnlyList<string> violations = type.GetDependencyNamespaceViolations(options);
			if (violations.Count > 0)
			{
				_violations[type] = violations;
			}

			return violations.Count == 0;
		}

#if NET8_0_OR_GREATER
		public async Task<ConstraintResult> IsMetBy(IAsyncEnumerable<Type?> actual, CancellationToken cancellationToken)
			=> await SetAsyncValue(actual, DependsOnlyOnAllowed);
#endif

		public ConstraintResult IsMetBy(IEnumerable<Type?> actual)
			=> SetValue(actual, DependsOnlyOnAllowed);

		protected override void AppendNormalExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append("all depend only on ").Append(options.Describe());

		protected override void AppendNormalResult(StringBuilder stringBuilder, string? indentation = null)
		{
			string itemIndentation = (indentation ?? string.Empty) + "  ";
			stringBuilder.Append(it).Append(" contained types with disallowed dependencies [");
			for (int index = 0; index < NotMatching.Length; index++)
			{
				Type? type = NotMatching[index];
				stringBuilder.Append(Environment.NewLine).Append(itemIndentation)
					.Append(Formatter.Format(type));

				// A null item has no violations to list; it fails because it cannot satisfy the expectation.
				if (type is not null && _violations.TryGetValue(type, out IReadOnlyList<string>? violations))
				{
					stringBuilder.Append(" depends on ").Append(Formatter.Format(violations));
				}

				if (index < NotMatching.Length - 1)
				{
					stringBuilder.Append(',');
				}
			}

			stringBuilder.Append(Environment.NewLine).Append(indentation).Append(']');
		}

		protected override void AppendNegatedExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append("not all depend only on ").Append(options.Describe());

		protected override void AppendNegatedResult(StringBuilder stringBuilder, string? indentation = null)
		{
			stringBuilder.Append(it).Append(" only contained types depending only on the allowed namespaces ");
			Formatter.Format(stringBuilder, Matching, FormattingOptions.Indented(indentation));
		}
	}
}
