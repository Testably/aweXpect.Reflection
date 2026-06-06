namespace aweXpect.Reflection.Tests.TestHelpers.Types;

#pragma warning disable CS0414 // Field is assigned but its value is never used
public class ImmutableClass
{
	public const int ConstantField = 4;
	public static int StaticMutableField = 1;
	public readonly int ReadOnlyField = 2;
	private readonly string _privateReadOnlyField = "";
	public static int StaticSettableProperty { get; set; }
	public int GetOnlyProperty { get; }
	public string ComputedProperty => _privateReadOnlyField;
}
#pragma warning restore CS0414

public class ImmutableClassWithInitProperty
{
	public int Value { get; init; }
}

public class ImmutableDerivedClass : ImmutableClass
{
	public readonly int DerivedReadOnlyField = 3;
}

public class ClassWithMutableField
{
	public int Value = 1;
}

public class ClassWithSettableProperty
{
	public int Value { get; set; }
}

public class ClassWithMutableFieldAndSettableProperty
{
	public int Field = 1;
	public int Property { get; set; }
}

public class MutableBaseClass
{
	private int _value = 1;

	public int GetValue() => _value;

	public void SetValue(int value) => _value = value;
}

public class ClassInheritingMutableField : MutableBaseClass;
