namespace aweXpect.Reflection.Tests.TestHelpers.Types;

#pragma warning disable CA1815
#pragma warning disable CA2225

/// <summary>
///     A value type that declares a representative set of operators (arithmetic, equality and conversions) used by the
///     operator assertion and filter tests.
/// </summary>
public readonly struct Money
{
	public Money(decimal amount) => Amount = amount;

	public decimal Amount { get; }

	public static Money operator +(Money left, Money right) => new(left.Amount + right.Amount);

	public static Money operator +(Money left, int right) => new(left.Amount + right);

	public static bool operator ==(Money left, Money right) => left.Amount == right.Amount;

	public static bool operator !=(Money left, Money right) => left.Amount != right.Amount;

	public static implicit operator decimal(Money money) => money.Amount;

	public static explicit operator int(Money money) => (int)money.Amount;

	public override bool Equals(object? obj) => obj is Money other && other.Amount == Amount;

	public override int GetHashCode() => Amount.GetHashCode();
}

/// <summary>
///     A base type that declares an operator, used to test the <c>inherit</c> flag of the operator assertions.
/// </summary>
public class OperatorBaseClass
{
	public OperatorBaseClass(int value) => Value = value;

	public int Value { get; }

	public static OperatorBaseClass operator -(OperatorBaseClass left, OperatorBaseClass right)
		=> new(left.Value - right.Value);
}

/// <summary>
///     A derived type that only declares the operator of its <see cref="OperatorBaseClass" /> through inheritance.
/// </summary>
public class OperatorDerivedClass(int value) : OperatorBaseClass(value);

/// <summary>
///     A type that declares no operators at all.
/// </summary>
public class ClassWithoutOperators
{
	public int Value { get; set; }
}

#pragma warning restore CA2225
#pragma warning restore CA1815
