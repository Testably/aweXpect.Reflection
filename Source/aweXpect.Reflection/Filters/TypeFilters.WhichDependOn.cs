using System;
using System.Collections.Generic;
using System.Linq;
using aweXpect.Reflection.Collections;
using aweXpect.Reflection.Helpers;
using aweXpect.Reflection.Options;

namespace aweXpect.Reflection;

public static partial class TypeFilters
{
	/// <summary>
	///     Filter for types which depend on (reference in their signature) at least one type in one of the
	///     <paramref name="namespaces" /> (including sub-namespaces).
	/// </summary>
	public static NamespaceDependencyFilterResult WhichDependOn(
		this Filtered.Types @this, params IEnumerable<string> namespaces)
		=> new(new NamespaceDependencyOptions(namespaces),
			options => @this.Which(Filter.Suffix<Type>(
				type => type.ResolveDependencies().Any(dependency => options.Matches(dependency.Namespace)),
				() => $"which depend on {options.Describe()} ")));

	/// <summary>
	///     Filter for types which do not depend on (do not reference in their signature) any type in one of the
	///     <paramref name="namespaces" /> (including sub-namespaces).
	/// </summary>
	public static NamespaceDependencyFilterResult WhichDoNotDependOn(
		this Filtered.Types @this, params IEnumerable<string> namespaces)
		=> new(new NamespaceDependencyOptions(namespaces),
			options => @this.Which(Filter.Suffix<Type>(
				type => !type.ResolveDependencies().Any(dependency => options.Matches(dependency.Namespace)),
				() => $"which do not depend on {options.Describe()} ")));
}
