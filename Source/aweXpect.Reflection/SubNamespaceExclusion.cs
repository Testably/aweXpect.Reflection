namespace aweXpect.Reflection;

/// <summary>
///     Controls whether sub-namespaces are excluded when matching namespace dependencies, and whether that exclusion
///     also applies to a type's own namespace.
/// </summary>
public enum SubNamespaceExclusion
{
	/// <summary>
	///     Sub-namespaces are not excluded: a namespace matches itself and all of its sub-namespaces (so
	///     <c>Foo.Bar</c> matches <c>Foo.Bar.Baz</c> but never <c>Foo.BarBaz</c>).
	/// </summary>
	None,

	/// <summary>
	///     The configured namespaces are matched exactly, but a type's own sub-namespaces stay allowed (for
	///     <c>DependsOnlyOn</c>, so a <c>Foo</c> type may still reference <c>Foo.Bar</c>).
	/// </summary>
	ExceptOwnNamespace,

	/// <summary>
	///     All namespaces are matched exactly, including the type's own namespace (so a <c>Foo</c> type referencing
	///     <c>Foo.Bar</c> becomes a violation for <c>DependsOnlyOn</c>).
	/// </summary>
	IncludingOwnNamespace,
}
