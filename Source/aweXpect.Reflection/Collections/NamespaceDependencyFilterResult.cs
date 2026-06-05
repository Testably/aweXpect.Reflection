using System;
using System.Collections.Generic;
using aweXpect.Reflection.Options;

namespace aweXpect.Reflection.Collections;

/// <summary>
///     A filtered collection of <see cref="System.Type" /> from a namespace-based dependency filter, allowing to
///     widen the targeted/allowed namespaces and to opt out of sub-namespace matching.
/// </summary>
/// <remarks>
///     Like all filtered collections, this is an immutable value object: <see cref="Or" /> and
///     <see cref="ExcludingSubNamespaces" /> do not mutate this instance but rebuild a fresh filter from the original
///     base collection, so deriving multiple views from the same instance cannot corrupt each other.
/// </remarks>
public sealed class NamespaceDependencyFilterResult : Filtered.Types
{
	private readonly Func<NamespaceDependencyOptions, Filtered.Types> _build;
	private readonly NamespaceDependencyOptions _options;

	internal NamespaceDependencyFilterResult(
		NamespaceDependencyOptions options,
		Func<NamespaceDependencyOptions, Filtered.Types> build)
		: base(build(options))
	{
		_options = options;
		_build = build;
	}

	/// <summary>
	///     Widens the filter by the given <paramref name="namespaces" /> (including sub-namespaces unless
	///     <see cref="ExcludingSubNamespaces" /> is used).
	/// </summary>
	public NamespaceDependencyFilterResult Or(params IEnumerable<string> namespaces)
	{
		NamespaceDependencyOptions widened = _options.Copy();
		widened.Or(namespaces);
		return new NamespaceDependencyFilterResult(widened, _build);
	}

	/// <summary>
	///     Excludes sub-namespaces from matching for the whole filter (including any <see cref="Or" /> additions),
	///     according to the <paramref name="exclusion" /> parameter.
	/// </summary>
	/// <remarks>
	///     Without this call, a namespace matches itself and all its sub-namespaces (so <c>Foo.Bar</c> includes
	///     <c>Foo.Bar.Baz</c> but not <c>Foo.BarBaz</c>).
	///     <para />
	///     For <c>WhichDependOnlyOn</c>, the type's own namespace is always allowed; with the default
	///     <see cref="SubNamespaceExclusion.ExceptOwnNamespace" /> its sub-namespaces stay allowed too. Use
	///     <see cref="SubNamespaceExclusion.IncludingOwnNamespace" /> to also exclude the own sub-namespaces, or
	///     <see cref="SubNamespaceExclusion.None" /> to keep including sub-namespaces.
	/// </remarks>
	public NamespaceDependencyFilterResult ExcludingSubNamespaces(
		SubNamespaceExclusion exclusion = SubNamespaceExclusion.ExceptOwnNamespace)
	{
		NamespaceDependencyOptions refined = _options.Copy();
		refined.ExcludingSubNamespaces(exclusion);
		return new NamespaceDependencyFilterResult(refined, _build);
	}
}
