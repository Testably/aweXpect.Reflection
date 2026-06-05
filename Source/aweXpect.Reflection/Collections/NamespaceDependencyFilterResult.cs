using System.Collections.Generic;
using aweXpect.Reflection.Options;

namespace aweXpect.Reflection.Collections;

/// <summary>
///     A filtered collection of <see cref="System.Type" /> from a namespace-based dependency filter, allowing to
///     widen the targeted/allowed namespaces and to opt out of sub-namespace matching.
/// </summary>
public sealed class NamespaceDependencyFilterResult : Filtered.Types
{
	private readonly NamespaceDependencyOptions _options;

	internal NamespaceDependencyFilterResult(Filtered.Types inner, NamespaceDependencyOptions options)
		: base(inner)
		=> _options = options;

	/// <summary>
	///     Widens the filter by the given <paramref name="namespaces" /> (including sub-namespaces unless
	///     <see cref="ExcludingSubNamespaces" /> is used).
	/// </summary>
	public NamespaceDependencyFilterResult Or(params IEnumerable<string> namespaces)
	{
		_options.Or(namespaces);
		return this;
	}

	/// <summary>
	///     Opts out of sub-namespace matching for the whole filter (including any <see cref="Or" /> additions),
	///     according to the <paramref name="exclude" /> parameter.
	/// </summary>
	/// <remarks>
	///     Without this call, a namespace matches itself and all its sub-namespaces (so <c>Foo.Bar</c> includes
	///     <c>Foo.Bar.Baz</c> but not <c>Foo.BarBaz</c>).
	/// </remarks>
	public NamespaceDependencyFilterResult ExcludingSubNamespaces(bool exclude = true)
	{
		_options.ExcludingSubNamespaces(exclude);
		return this;
	}
}
