using System;
using System.Collections.Generic;
using System.Linq;
using aweXpect.Core;
using aweXpect.Reflection.Helpers;

namespace aweXpect.Reflection.Options;

/// <summary>
///     Options for a namespace-based type dependency assertion or filter.
/// </summary>
/// <remarks>
///     The instance is shared between the chainable result and the underlying constraint/filter, so that
///     <c>Or(…)</c> and <c>ExcludingSubNamespaces(…)</c> widen or refine the (lazily evaluated) expression.
/// </remarks>
internal sealed class NamespaceDependencyOptions
{
	private readonly List<string> _namespaces = [];
	private SubNamespaceExclusion _subNamespaceExclusion = SubNamespaceExclusion.None;

	public NamespaceDependencyOptions(IEnumerable<string> namespaces)
	{
		_namespaces.AddRange(namespaces);
		if (_namespaces.Count == 0)
		{
			throw new ArgumentException("At least one namespace must be specified.");
		}
	}

	private NamespaceDependencyOptions(IEnumerable<string> namespaces, SubNamespaceExclusion subNamespaceExclusion)
	{
		_namespaces.AddRange(namespaces);
		_subNamespaceExclusion = subNamespaceExclusion;
	}

	/// <summary>
	///     Creates an independent copy, so that refining a (reusable) filter does not mutate the shared instance.
	/// </summary>
	public NamespaceDependencyOptions Copy()
		=> new(_namespaces, _subNamespaceExclusion);

	/// <summary>
	///     The namespaces that are targeted (for depends-on / does-not-depend-on) or allowed (for depends-only-on).
	/// </summary>
	public IReadOnlyList<string> Namespaces => _namespaces;

	/// <summary>
	///     Indicates whether sub-namespaces are excluded from matching.
	/// </summary>
	public bool ExcludeSubNamespaces => _subNamespaceExclusion != SubNamespaceExclusion.None;

	/// <summary>
	///     Indicates whether sub-namespaces of the type's own namespace are still allowed (for depends-only-on).
	/// </summary>
	/// <remarks>
	///     The type's own namespace is always allowed; its sub-namespaces stay allowed unless the caller opted into
	///     <see cref="SubNamespaceExclusion.IncludingOwnNamespace" />.
	/// </remarks>
	public bool IncludeOwnSubNamespaces => _subNamespaceExclusion != SubNamespaceExclusion.IncludingOwnNamespace;

	/// <summary>
	///     Widens the set of targeted/allowed namespaces by the given <paramref name="namespaces" />.
	/// </summary>
	public void Or(IEnumerable<string> namespaces)
		=> _namespaces.AddRange(namespaces);

	/// <summary>
	///     Sets how sub-namespaces are excluded for the whole expression.
	/// </summary>
	public void ExcludingSubNamespaces(SubNamespaceExclusion subNamespaceExclusion)
		=> _subNamespaceExclusion = subNamespaceExclusion;

	/// <summary>
	///     Checks whether the <paramref name="dependencyNamespace" /> matches any of the configured namespaces.
	/// </summary>
	public bool Matches(string? dependencyNamespace)
		=> _namespaces.Any(@namespace
			=> TypeHelpers.NamespaceMatches(dependencyNamespace, @namespace, !ExcludeSubNamespaces));

	/// <summary>
	///     Describes the configured namespaces for an expectation message.
	/// </summary>
	/// <remarks>
	///     The constructor guarantees at least one namespace, so an empty set cannot occur here.
	/// </remarks>
	public string Describe()
		=> $"namespace {string.Join(" or ", _namespaces.Select(@namespace => Formatter.Format(@namespace)))}";
}
