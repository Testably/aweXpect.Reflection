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
///     <c>OrOn(…)</c> and <c>ExcludingSubNamespaces(…)</c> widen or refine the (lazily evaluated) expression.
/// </remarks>
internal sealed class NamespaceDependencyOptions
{
	private readonly List<string> _namespaces = [];
	private bool _excludeOwnSubNamespaces;
	private bool _excludeSubNamespaces;

	public NamespaceDependencyOptions(IEnumerable<string> namespaces)
	{
		_namespaces.AddRange(namespaces);
		if (_namespaces.Count == 0)
		{
			throw new ArgumentException("At least one namespace must be specified.");
		}

		if (_namespaces.Contains(null!))
		{
			throw new ArgumentNullException(nameof(namespaces), "The namespaces must not contain null.");
		}
	}

	private NamespaceDependencyOptions(IEnumerable<string> namespaces, bool excludeSubNamespaces,
		bool excludeOwnSubNamespaces)
	{
		_namespaces.AddRange(namespaces);
		_excludeSubNamespaces = excludeSubNamespaces;
		_excludeOwnSubNamespaces = excludeOwnSubNamespaces;
	}

	/// <summary>
	///     Creates an independent copy, so that refining a (reusable) filter does not mutate the shared instance.
	/// </summary>
	public NamespaceDependencyOptions Copy()
		=> new(_namespaces, _excludeSubNamespaces, _excludeOwnSubNamespaces);

	/// <summary>
	///     The namespaces that are targeted (for depends-on / does-not-depend-on) or allowed (for depends-only-on).
	/// </summary>
	public IReadOnlyList<string> Namespaces => _namespaces;

	/// <summary>
	///     Indicates whether sub-namespaces are excluded from matching.
	/// </summary>
	public bool ExcludeSubNamespaces => _excludeSubNamespaces;

	/// <summary>
	///     Indicates whether sub-namespaces of the type's own namespace are still allowed (for depends-only-on).
	/// </summary>
	/// <remarks>
	///     The type's own namespace is always allowed; its sub-namespaces stay allowed unless the caller opted into
	///     <see cref="ExcludingOwnSubNamespaces" />.
	/// </remarks>
	public bool IncludeOwnSubNamespaces => !_excludeOwnSubNamespaces;

	/// <summary>
	///     Widens the set of targeted/allowed namespaces by the given <paramref name="namespaces" />.
	/// </summary>
	public void OrOn(IEnumerable<string> namespaces)
	{
		List<string> added = namespaces.ToList();
		if (added.Count == 0)
		{
			throw new ArgumentException("At least one namespace must be specified.");
		}

		if (added.Contains(null!))
		{
			throw new ArgumentNullException(nameof(namespaces), "The namespaces must not contain null.");
		}

		_namespaces.AddRange(added);
	}

	/// <summary>
	///     Excludes sub-namespaces of the targeted/allowed namespaces from matching for the whole expression.
	/// </summary>
	public void ExcludingSubNamespaces()
		=> _excludeSubNamespaces = true;

	/// <summary>
	///     Excludes sub-namespaces of the type's own namespace from being implicitly allowed (for depends-only-on).
	/// </summary>
	public void ExcludingOwnSubNamespaces()
		=> _excludeOwnSubNamespaces = true;

	/// <summary>
	///     Checks whether the <paramref name="dependencyNamespace" /> matches any of the configured namespaces.
	/// </summary>
	public bool Matches(string? dependencyNamespace)
		=> _namespaces.Any(@namespace
			=> TypeHelpers.NamespaceMatches(dependencyNamespace, @namespace, !ExcludeSubNamespaces));

	/// <summary>
	///     Checks whether the <paramref name="type" /> has at least one dependency in a configured namespace.
	/// </summary>
	public bool IsMatchedBy(Type type)
		=> type.ResolveDependencies().Any(dependency => Matches(dependency.Namespace));

	/// <summary>
	///     Describes the configured namespaces for an expectation message.
	/// </summary>
	/// <remarks>
	///     The constructor guarantees at least one namespace, so an empty set cannot occur here.
	/// </remarks>
	public string Describe()
		=> $"namespace {string.Join(" or ", _namespaces.Select(@namespace => Formatter.Format(@namespace)))}";
}
