// ReSharper disable All

#pragma warning disable CS0169 // field is never used
#pragma warning disable CS0649 // field is never assigned
#pragma warning disable CS0067 // event is never used
#pragma warning disable CS8618 // non-nullable field must contain a non-null value

using System;
using System.Collections.Generic;
using System.Text;

namespace aweXpect.Reflection.Tests.TestHelpers.Dependencies.Layer1
{
	public class TargetA;

	public interface ITargetInterface;

	[AttributeUsage(AttributeTargets.All)]
	public sealed class TargetAttribute : Attribute;

	[AttributeUsage(AttributeTargets.All)]
	public sealed class TargetTypeAttribute(Type type) : Attribute
	{
		public Type Type { get; } = type;
	}

	public delegate void TargetEventHandler();

	public class TargetGeneric<T>;
}

namespace aweXpect.Reflection.Tests.TestHelpers.Dependencies.Layer1.Sub
{
	public class SubTarget;
}

namespace aweXpect.Reflection.Tests.TestHelpers.Dependencies.Layer2
{
	public class TargetB;
}

namespace aweXpect.Reflection.Tests.TestHelpers.Dependencies.Consumers
{
	using aweXpect.Reflection.Tests.TestHelpers.Dependencies.Layer1;
	using aweXpect.Reflection.Tests.TestHelpers.Dependencies.Layer2;

	public class ViaBaseType : TargetA;

	public class ViaInterface : ITargetInterface;

	public class ViaField
	{
		private TargetA _target;
	}

	public class ViaProperty
	{
		public TargetA Target { get; set; }
	}

	public class ViaIndexer
	{
		public int this[TargetA index] => 0;
	}

	public class ViaEvent
	{
		public event TargetEventHandler Raised;
	}

	public class ViaMethodParameter
	{
		public static void Method(TargetA target)
		{
		}
	}

	public class ViaMethodReturn
	{
		public static TargetA Method() => null!;
	}

	public class ViaGenericArgument
	{
		private List<TargetA> _targets;
	}

	[Target]
	public class ViaAttribute;

	// Layer2's TargetB is referenced ONLY through the attribute's typeof(...) argument; the attribute type
	// TargetTypeAttribute itself lives in Layer1.
	[TargetType(typeof(TargetB))]
	public class ViaAttributeArgument;

	public class ViaGenericConstraint<T>
		where T : TargetA;

	public class ViaSubNamespace
	{
		private Layer1.Sub.SubTarget _target;
	}

	public class OnlyLayer1
	{
		private TargetA _target;
	}

	public class OnlyLayer2
	{
		private TargetB _target;
	}

	public class Layer1AndLayer2
	{
		private TargetA _a;
		private TargetB _b;
	}

	public class ReferencesOwnNamespace
	{
		private OnlyLayer1 _other;
	}

	public class ReferencesOwnSubNamespace
	{
		private OwnSub.OwnSubTarget _target;
	}

	public class FrameworkConsumer
	{
		private List<int> _values;
		private StringBuilder _builder;
	}

	public class ReferencesGlobal
	{
		private global::GlobalNamespaceTarget _target;
	}
}

namespace aweXpect.Reflection.Tests.TestHelpers.Dependencies.Consumers.OwnSub
{
	public class OwnSubTarget;
}

// Types whose only "System" or Layer1 surface is what the compiler or the inheritance chain contributes;
// kept in a separate namespace so that the Consumers-based filter tests are unaffected.
namespace aweXpect.Reflection.Tests.TestHelpers.Dependencies.Synthetic
{
	using System.Threading.Tasks;
	using aweXpect.Reflection.Tests.TestHelpers.Dependencies.Layer1;

	public class WithRequiredProperty
	{
		public required TargetA Target { get; set; }
	}

	public class BaseWithLayer1Interface : ITargetInterface;

	public class DerivedWithoutOwnReferences : BaseWithLayer1Interface;

	public record RecordWithLayer1Target(TargetA Target);

	public delegate TargetA TargetProviderDelegate(Layer2.TargetB input);

	public class WithAsyncMethod
	{
		public async void MethodAsync() => await Task.CompletedTask;
	}

	public class WithIteratorMethod
	{
		public IEnumerable<int> Numbers()
		{
			yield return 1;
		}
	}
}
