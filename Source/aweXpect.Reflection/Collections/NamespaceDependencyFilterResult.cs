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
	///     Opts out of sub-namespace matching for the whole filter (including any <see cref="Or" /> additions),
	///     according to the <paramref name="exclude" /> parameter.
	/// </summary>
	/// <remarks>
	///     Without this call, a namespace matches itself and all its sub-namespaces (so <c>Foo.Bar</c> includes
	///     <c>Foo.Bar.Baz</c> but not <c>Foo.BarBaz</c>).
	/// </remarks>
	public NamespaceDependencyFilterResult ExcludingSubNamespaces(bool exclude = true)
	{
		NamespaceDependencyOptions refined = _options.Copy();
		refined.ExcludingSubNamespaces(exclude);
		return new NamespaceDependencyFilterResult(refined, _build);
	}
}
