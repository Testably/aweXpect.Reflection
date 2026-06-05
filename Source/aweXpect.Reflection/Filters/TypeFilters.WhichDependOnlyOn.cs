using System;
using System.Collections.Generic;
using aweXpect.Customization;
using aweXpect.Reflection.Collections;
using aweXpect.Reflection.Helpers;
using aweXpect.Reflection.Options;

namespace aweXpect.Reflection;

public static partial class TypeFilters
{
	/// <summary>
	///     Filter for types which depend on (reference in their signature) only types in the
	///     <paramref name="namespaces" /> (including sub-namespaces), their own namespace or framework assemblies.
	/// </summary>
	/// <remarks>
	///     Dependencies on types whose assembly name starts with one of the
	///     <see cref="AwexpectCustomization.ReflectionCustomizationValue.ExcludedAssemblyPrefixes" /> are ignored, so
	///     that framework namespaces do not have to be listed explicitly.
	/// </remarks>
	public static NamespaceDependencyFilterResult WhichDependOnlyOn(
		this Filtered.Types @this, params IEnumerable<string> namespaces)
		=> new(new NamespaceDependencyOptions(namespaces),
			options => @this.Which(Filter.Suffix<Type>(
				type => type.GetDependencyNamespaceViolations(options).Count == 0,
				() => $"which depend only on {options.Describe()} ")));
}
