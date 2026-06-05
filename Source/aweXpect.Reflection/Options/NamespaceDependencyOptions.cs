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
	private bool _excludeSubNamespaces;

	public NamespaceDependencyOptions(IEnumerable<string> namespaces)
		=> _namespaces.AddRange(namespaces);

	/// <summary>
	///     The namespaces that are targeted (for depends-on / does-not-depend-on) or allowed (for depends-only-on).
	/// </summary>
	public IReadOnlyList<string> Namespaces => _namespaces;

	/// <summary>
	///     Indicates whether sub-namespaces are excluded from matching.
	/// </summary>
	public bool ExcludeSubNamespaces => _excludeSubNamespaces;

	/// <summary>
	///     Widens the set of targeted/allowed namespaces by the given <paramref name="namespaces" />.
	/// </summary>
	public void Or(IEnumerable<string> namespaces)
		=> _namespaces.AddRange(namespaces);

	/// <summary>
	///     Opts out of (or back into) sub-namespace matching for the whole expression.
	/// </summary>
	public void ExcludingSubNamespaces(bool exclude)
		=> _excludeSubNamespaces = exclude;

	/// <summary>
	///     Checks whether the <paramref name="dependencyNamespace" /> matches any of the configured namespaces.
	/// </summary>
	public bool Matches(string? dependencyNamespace)
		=> _namespaces.Any(@namespace
			=> TypeHelpers.NamespaceMatches(dependencyNamespace, @namespace, !_excludeSubNamespaces));

	/// <summary>
	///     Describes the configured namespaces for an expectation message.
	/// </summary>
	public string Describe()
		=> _namespaces.Count == 0
			? "no namespace"
			: $"namespace {string.Join(" or ", _namespaces.Select(@namespace => Formatter.Format(@namespace)))}";
}
