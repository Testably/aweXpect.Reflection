#if NET10_0_OR_GREATER
using System;

namespace aweXpect.Reflection.Tests.TestHelpers.Types;

/// <summary>
///     Uses the C# extension block syntax (C# 14 / .NET 10) to exercise detection of new-syntax extension properties.
/// </summary>
public static class StaticClassWithNewExtensionProperties
{
	extension(string text)
	{
		// Instance extension property - the real PropertyInfo is held by the compiler-generated grouping type,
		// while the public class only exposes its (non-special-name) get_ accessor implementation.
		public bool IsBlankText => string.IsNullOrWhiteSpace(text);

		// Instance extension method (new syntax) - its public implementation carries the [Extension] attribute and
		// produces a non-special-name skeleton in the grouping type (so accessor detection must skip it).
		public bool IsLongText() => text.Length > 10;
	}

	extension(string)
	{
		// Static extension property - emitted as a (special-name) accessor in the grouping type.
		public static string DefaultValue => "default";
	}

	// Regular static property in an extension-bearing class - not an extension property.
	public static int RegularProperty => 42;

	// Regular static method whose public accessor-shaped sibling must not be confused for an accessor.
	public static void RegularStaticMethod() { }

	// Contains a lambda, so the class also owns a compiler-generated <>c nested type which must be
	// distinguished from the (also unspeakably named) extension grouping types.
	public static int WithClosure()
	{
		Func<int, int> increment = value => value + 1;
		return increment(41);
	}

	// Non-grouping nested type, to ensure it is skipped while scanning for grouping types.
	public sealed class NestedHelper { }
}
#endif
