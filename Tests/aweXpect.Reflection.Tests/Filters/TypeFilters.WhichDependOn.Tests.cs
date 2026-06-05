using aweXpect.Reflection.Collections;
using aweXpect.Reflection.Tests.TestHelpers.Dependencies.Consumers;

namespace aweXpect.Reflection.Tests.Filters;

public sealed partial class TypeFilters
{
	public sealed class WhichDependOn
	{
		private const string ConsumersNamespace = "aweXpect.Reflection.Tests.TestHelpers.Dependencies.Consumers";
		private const string Layer1Namespace = "aweXpect.Reflection.Tests.TestHelpers.Dependencies.Layer1";

		public sealed class Tests
		{
			[Fact]
			public async Task ShouldFilterForTypesDependingOnNamespace()
			{
				Filtered.Types types = In.Namespace(ConsumersNamespace).WhichDependOn(Layer1Namespace);

				await That(types).Contains(typeof(ViaField));
				await That(types).Contains(typeof(ViaSubNamespace));
				await That(types).DoesNotContain(typeof(FrameworkConsumer));
			}

			[Fact]
			public async Task WhenExcludingSubNamespaces_ShouldNotMatchSubNamespace()
			{
				Filtered.Types types = In.Namespace(ConsumersNamespace)
					.WhichDependOn(Layer1Namespace).ExcludingSubNamespaces();

				await That(types).Contains(typeof(ViaField));
				await That(types).DoesNotContain(typeof(ViaSubNamespace));
			}

			[Fact]
			public async Task WhenWidenedWithOr_ShouldMatchEither()
			{
				Filtered.Types types = In.Namespace(ConsumersNamespace)
					.WhichDependOn("Non.Existent.Namespace").Or(Layer1Namespace);

				await That(types).Contains(typeof(ViaField));
			}
		}
	}
}
