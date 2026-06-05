using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using aweXpect.Reflection.Collections;
using aweXpect.Reflection.Helpers;

namespace aweXpect.Reflection.Options;

/// <summary>
///     Options for a dependency assertion or filter whose targets are filtered collections of types
///     (<see cref="Filtered.Types" />).
/// </summary>
/// <remarks>
///     The instance is shared between the chainable result and the underlying constraint/filter, so that
///     <c>OrOn(…)</c> widens the (lazily evaluated) expression.
///     <para />
///     The target collections are resolved once per assertion via <see cref="Resolve" /> (the union of all
///     collections), so the resolved set can be reused across the subject's items. <see cref="Matches" /> and
///     <see cref="IsMatchedBy" /> require <see cref="Resolve" /> to have been awaited before.
/// </remarks>
internal sealed class TypeSetDependencyOptions
{
	private readonly List<Filtered.Types> _targets = [];
	private HashSet<Type>? _resolved;

	public TypeSetDependencyOptions(Filtered.Types target, Filtered.Types[] additional)
	{
		_targets.Add(target ?? throw new ArgumentNullException(nameof(target),
			"The target collection of types must not be null."));
		if (additional is null)
		{
			throw new ArgumentNullException(nameof(additional),
				"The additional target collections of types must not be null.");
		}

		Add(additional);
	}

	private TypeSetDependencyOptions(IEnumerable<Filtered.Types> targets)
		=> _targets.AddRange(targets);

	/// <summary>
	///     Creates an independent copy, so that refining a (reusable) filter does not mutate the shared instance.
	/// </summary>
	public TypeSetDependencyOptions Copy()
		=> new(_targets);

	/// <summary>
	///     Widens the set of targeted/allowed types by the given <paramref name="targets" />.
	/// </summary>
	public void OrOn(Filtered.Types[] targets)
	{
		if (targets is null)
		{
			throw new ArgumentNullException(nameof(targets), "The target collections of types must not be null.");
		}

		if (targets.Length == 0)
		{
			throw new ArgumentException("At least one collection of types must be specified.");
		}

		Add(targets);
	}

	private void Add(Filtered.Types[] targets)
	{
		foreach (Filtered.Types target in targets)
		{
			_targets.Add(target ?? throw new ArgumentNullException(nameof(targets),
				"The target collections of types must not contain null."));
		}

		// Widening invalidates a previously resolved set.
		_resolved = null;
	}

	/// <summary>
	///     Resolves the target collections once into their union set; subsequent <see cref="Matches" /> /
	///     <see cref="IsMatchedBy" /> calls use this set.
	/// </summary>
#if NET8_0_OR_GREATER
	public async ValueTask Resolve()
	{
		if (_resolved is not null)
		{
			return;
		}

		HashSet<Type> resolved = [];
		foreach (Filtered.Types target in _targets)
		{
			await foreach (Type type in target)
			{
				resolved.Add(type);
			}
		}

		_resolved = resolved;
	}
#else
	public Task Resolve()
	{
		if (_resolved is null)
		{
			HashSet<Type> resolved = [];
			foreach (Filtered.Types target in _targets)
			{
				resolved.UnionWith(target);
			}

			_resolved = resolved;
		}

		return Task.CompletedTask;
	}
#endif

	/// <summary>
	///     Checks whether the <paramref name="dependency" /> is a member of the resolved target set.
	/// </summary>
	/// <remarks>
	///     Membership is by <see cref="Type" /> identity. Because dependencies keep constructed generic types as
	///     written (e.g. <c>List&lt;Foo&gt;</c>) while a scanned target collection contains the generic type
	///     definition (<c>List&lt;&gt;</c>), a dependency on any construction additionally matches when its
	///     definition is a member of the set — mirroring how a generic type definition target matches any
	///     construction in the specific-type overloads.
	/// </remarks>
	public bool Matches(Type dependency)
	{
		HashSet<Type> resolved = _resolved
		                         ?? throw new InvalidOperationException("Resolve must be awaited before Matches.");
		return resolved.Contains(dependency) ||
		       (dependency.IsGenericType && !dependency.IsGenericTypeDefinition &&
		        resolved.Contains(dependency.GetGenericTypeDefinition()));
	}

	/// <summary>
	///     Checks whether the <paramref name="type" /> has at least one dependency in the resolved target set.
	/// </summary>
	public bool IsMatchedBy(Type type)
		=> type.ResolveDependencies().Any(Matches);

	/// <summary>
	///     Describes the configured target collections for an expectation message.
	/// </summary>
	/// <remarks>
	///     The constructor guarantees at least one target, so an empty set cannot occur here.
	/// </remarks>
	public string Describe()
		=> string.Join(" or ", _targets.Select(target => target.GetDescription().TrimEnd()));
}
