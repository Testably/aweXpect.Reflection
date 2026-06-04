namespace aweXpect.Reflection.Helpers;

/// <summary>
///     Maps the <see cref="Operator" /> enum to the compiler-emitted <c>op_*</c> metadata name.
/// </summary>
internal static class OperatorNames
{
	/// <summary>
	///     Gets the compiler-emitted <c>op_*</c> metadata name for the <paramref name="operator" />
	///     (e.g. <see cref="Operator.Addition" /> maps to <c>op_Addition</c>).
	/// </summary>
	public static string Of(Operator @operator)
		=> "op_" + @operator;

	/// <summary>
	///     Gets the compiler-emitted metadata name of an implicit (<c>op_Implicit</c>) or explicit
	///     (<c>op_Explicit</c>) conversion operator.
	/// </summary>
	public static string Conversion(bool isImplicit)
		=> isImplicit ? "op_Implicit" : "op_Explicit";
}
