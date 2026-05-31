using System;
using aweXpect.Core;
using aweXpect.Options;
using aweXpect.Reflection.Collections;
using aweXpect.Reflection.Helpers;

namespace aweXpect.Reflection;

public static partial class TypeFilters
{
	/// <summary>
	///     Filter for types within the <paramref name="expected" /> namespace (including sub-namespaces).
	/// </summary>
	/// <remarks>
	///     Unlike <see cref="WithNamespace(Filtered.Types, string)" /> with <c>AsPrefix()</c>, this matches the
	///     <paramref name="expected" /> namespace and its sub-namespaces, but not namespaces that merely start with the same
	///     text (e.g. <c>Foo.Bar</c> does not include <c>Foo.BarBaz</c>).
	/// </remarks>
	public static Filtered.Types.StringEqualityResult WithinNamespace(this Filtered.Types @this, string expected)
	{
		StringEqualityOptions options = new();
		return new Filtered.Types.StringEqualityResult(@this.Which(Filter.Suffix<Type>(
				type => type.IsWithinNamespace(expected, options),
				() => $"within namespace {Formatter.Format(expected)}{options} ")),
			options);
	}
}
