using System;
using aweXpect.Core;
using aweXpect.Reflection.Options;
using aweXpect.Results;

namespace aweXpect.Reflection.Results;

/// <summary>
///     The result of a specific-type dependency assertion, allowing to widen the targeted types.
/// </summary>
public sealed class TypeDependencyResult<TThat>
	: AndOrResult<TThat, IThat<TThat>>
{
	private readonly TypeDependencyOptions _options;

	internal TypeDependencyResult(
		ExpectationBuilder expectationBuilder,
		IThat<TThat> subject,
		TypeDependencyOptions options)
		: base(expectationBuilder, subject)
		=> _options = options;

	/// <summary>
	///     Widens the expression by the type <typeparamref name="T" />.
	/// </summary>
	public new TypeDependencyResult<TThat> Or<T>()
	{
		_options.Or(typeof(T));
		return this;
	}

	/// <summary>
	///     Widens the expression by the <paramref name="type" />.
	/// </summary>
	public new TypeDependencyResult<TThat> Or(Type type)
	{
		_options.Or(type);
		return this;
	}
}
