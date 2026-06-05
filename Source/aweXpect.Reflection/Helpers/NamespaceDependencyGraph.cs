using System;
using System.Collections.Generic;
using System.Linq;

namespace aweXpect.Reflection.Helpers;

/// <summary>
///     Builds the directed namespace (or slice) dependency graph of a set of types and detects its dependency cycles.
/// </summary>
/// <remarks>
///     Nodes are the namespaces (or, when a slice root is given, the slices) that the analyzed types live in. An edge
///     <c>A → B</c> exists when some type in node <c>A</c> has a (resolved) dependency on a type whose node is <c>B</c>
///     and <c>B</c> is also part of the analyzed set — dependencies on framework or otherwise out-of-set namespaces
///     never create an edge, consistent with the namespace dependency assertions. Self-edges (<c>A → A</c>) are ignored,
///     so a single namespace referencing itself is not a cycle.
///     <para />
///     Dependencies are read exclusively through <see cref="TypeHelpers.ResolveDependencies" />, so a configured custom
///     dependency resolver automatically sharpens the cycle detection too.
///     <para />
///     Cycles are the strongly-connected components with more than one node, found via Tarjan's algorithm. For each one
///     a readable path back to its starting node is rendered (e.g. <c>A -&gt; B -&gt; A</c>).
/// </remarks>
internal sealed class NamespaceDependencyGraph
{
	private NamespaceDependencyGraph(IReadOnlyList<IReadOnlyList<string>> cycles)
		=> Cycles = cycles;

	/// <summary>
	///     The detected dependency cycles, each as an ordered list of node names that returns to its first node
	///     (e.g. <c>["A", "B", "A"]</c>). Empty when the graph is acyclic.
	/// </summary>
	public IReadOnlyList<IReadOnlyList<string>> Cycles { get; }

	/// <summary>
	///     Builds the graph for the <paramref name="types" />, grouping namespaces into slices below
	///     <paramref name="sliceRoot" /> when it is not <see langword="null" /> (a namespace below the root collapses
	///     to the root plus its next segment), and detects all dependency cycles.
	/// </summary>
	public static NamespaceDependencyGraph Build(IEnumerable<Type?> types, string? sliceRoot)
	{
		Dictionary<string, HashSet<string>> adjacency = new(StringComparer.Ordinal);
		List<Type> nodes = [];

		// First pass: every node that the analyzed types occupy (the only legal edge endpoints).
		foreach (Type? type in types)
		{
			if (type is null)
			{
				continue;
			}

			nodes.Add(type);
			string node = SliceKey(type.Namespace, sliceRoot);
			if (!adjacency.ContainsKey(node))
			{
				adjacency[node] = new HashSet<string>(StringComparer.Ordinal);
			}
		}

		// Second pass: edges between nodes (ignoring self-edges and dependencies outside the analyzed set).
		foreach (Type type in nodes)
		{
			string from = SliceKey(type.Namespace, sliceRoot);
			foreach (Type dependency in type.ResolveDependencies())
			{
				string to = SliceKey(dependency.Namespace, sliceRoot);
				if (!string.Equals(from, to, StringComparison.Ordinal) && adjacency.ContainsKey(to))
				{
					adjacency[from].Add(to);
				}
			}
		}

		return new NamespaceDependencyGraph(DetectCycles(adjacency));
	}

	/// <summary>
	///     Maps the <paramref name="namespace" /> to its node name: the namespace itself by default, or — when a
	///     <paramref name="sliceRoot" /> is given and the namespace lies below it — the root plus its next segment.
	/// </summary>
	private static string SliceKey(string? @namespace, string? sliceRoot)
	{
		if (@namespace is null)
		{
			return TypeHelpers.GlobalNamespaceDisplay;
		}

		if (sliceRoot is null ||
		    !TypeHelpers.NamespaceMatches(@namespace, sliceRoot, includeSubNamespaces: true) ||
		    string.Equals(@namespace, sliceRoot, StringComparison.Ordinal))
		{
			return @namespace;
		}

		int segmentStart = sliceRoot.Length + 1;
		int nextDot = @namespace.IndexOf('.', segmentStart);
		return nextDot < 0 ? @namespace : @namespace.Substring(0, nextDot);
	}

	/// <summary>
	///     Returns the strongly-connected components with more than one node (the cycles), each rendered as a path
	///     that returns to its starting node. The result is ordered (and the path within each cycle starts at its
	///     ordinally smallest node) so failure messages are deterministic.
	/// </summary>
	private static IReadOnlyList<IReadOnlyList<string>> DetectCycles(
		Dictionary<string, HashSet<string>> adjacency)
	{
		List<IReadOnlyList<string>> cycles = [];
		foreach (HashSet<string> component in new TarjanScc(adjacency).StronglyConnectedComponents())
		{
			if (component.Count > 1)
			{
				cycles.Add(ExtractCyclePath(component, adjacency));
			}
		}

		cycles.Sort((left, right) => string.Compare(string.Join(" -> ", left), string.Join(" -> ", right),
			StringComparison.Ordinal));
		return cycles;
	}

	/// <summary>
	///     Finds the shortest cycle through the ordinally smallest node of the strongly-connected
	///     <paramref name="component" /> via a breadth-first search over the component-internal edges.
	/// </summary>
	private static IReadOnlyList<string> ExtractCyclePath(
		HashSet<string> component, Dictionary<string, HashSet<string>> adjacency)
	{
		string start = component.OrderBy(node => node, StringComparer.Ordinal).First();
		Dictionary<string, string> predecessor = new(StringComparer.Ordinal);
		HashSet<string> visited = new(StringComparer.Ordinal) { start, };
		Queue<string> queue = new();
		queue.Enqueue(start);

		while (queue.Count > 0)
		{
			string current = queue.Dequeue();
			foreach (string next in adjacency[current]
				         .Where(component.Contains)
				         .OrderBy(node => node, StringComparer.Ordinal))
			{
				if (string.Equals(next, start, StringComparison.Ordinal))
				{
					return BuildPath(predecessor, start, current);
				}

				if (visited.Add(next))
				{
					predecessor[next] = current;
					queue.Enqueue(next);
				}
			}
		}

		// A strongly-connected component with more than one node always contains a cycle through its start node,
		// so the breadth-first search above always returns; this is unreachable.
		return [start, start,];
	}

	/// <summary>
	///     Reconstructs the path <c>start → … → end → start</c> from the breadth-first <paramref name="predecessor" />
	///     map (the trailing <paramref name="start" /> closes the cycle).
	/// </summary>
	private static IReadOnlyList<string> BuildPath(
		Dictionary<string, string> predecessor, string start, string end)
	{
		List<string> path = [];
		for (string node = end; !string.Equals(node, start, StringComparison.Ordinal); node = predecessor[node])
		{
			path.Add(node);
		}

		path.Add(start);
		path.Reverse();
		path.Add(start);
		return path;
	}

	/// <summary>
	///     Tarjan strongly-connected-components search over the namespace/slice graph. The node count is the number of
	///     distinct namespaces/slices in the analyzed set, so the recursive walk cannot reach a problematic depth.
	/// </summary>
	private sealed class TarjanScc(Dictionary<string, HashSet<string>> adjacency)
	{
		private readonly Dictionary<string, int> _indices = new(StringComparer.Ordinal);
		private readonly Dictionary<string, int> _lowLinks = new(StringComparer.Ordinal);
		private readonly HashSet<string> _onStack = new(StringComparer.Ordinal);
		private readonly Stack<string> _stack = new();
		private readonly List<HashSet<string>> _components = [];
		private int _nextIndex;

		public IReadOnlyList<HashSet<string>> StronglyConnectedComponents()
		{
			foreach (string node in adjacency.Keys.OrderBy(node => node, StringComparer.Ordinal))
			{
				if (!_indices.ContainsKey(node))
				{
					StrongConnect(node);
				}
			}

			return _components;
		}

		private void StrongConnect(string node)
		{
			_indices[node] = _nextIndex;
			_lowLinks[node] = _nextIndex;
			_nextIndex++;
			_stack.Push(node);
			_onStack.Add(node);

			foreach (string next in adjacency[node])
			{
				if (!_indices.ContainsKey(next))
				{
					StrongConnect(next);
					_lowLinks[node] = Math.Min(_lowLinks[node], _lowLinks[next]);
				}
				else if (_onStack.Contains(next))
				{
					_lowLinks[node] = Math.Min(_lowLinks[node], _indices[next]);
				}
			}

			if (_lowLinks[node] != _indices[node])
			{
				return;
			}

			HashSet<string> component = new(StringComparer.Ordinal);
			string member;
			do
			{
				member = _stack.Pop();
				_onStack.Remove(member);
				component.Add(member);
			} while (!string.Equals(member, node, StringComparison.Ordinal));

			_components.Add(component);
		}
	}
}
