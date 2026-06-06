using System;
using aweXpect.Reflection.Collections;
using aweXpect.Reflection.Helpers;

namespace aweXpect.Reflection;

public static partial class TypeFilters
{
	/// <summary>
	///     Filters for types whose fields and properties are all nullable.
	/// </summary>
	public static Filtered.Types WhichOnlyHaveNullableMembers(this Filtered.Types @this)
		=> @this.Which(Filter.Suffix<Type>(
			type => type.GetNotNullableMembers().Length == 0,
			"which only have nullable members "));

	/// <summary>
	///     Filters for types whose fields and properties are all non-nullable.
	/// </summary>
	public static Filtered.Types WhichOnlyHaveNonNullableMembers(this Filtered.Types @this)
		=> @this.Which(Filter.Suffix<Type>(
			type => type.GetNullableMembers().Length == 0,
			"which only have non-nullable members "));
}
