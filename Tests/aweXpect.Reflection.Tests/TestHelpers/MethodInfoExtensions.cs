using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace aweXpect.Reflection.Tests.TestHelpers;

public static class MethodInfoExtensions
{
	/// <summary>
	///     Checks if the <paramref name="methodInfo" /> is asynchronous (declared with the <see langword="async" /> keyword).
	/// </summary>
	/// <param name="methodInfo">The <see cref="MethodInfo" /> to check.</param>
	public static bool IsReallyAsync(this MethodInfo? methodInfo)
		=> methodInfo?.GetCustomAttribute<AsyncStateMachineAttribute>() is not null;

	/// <summary>
	///     Checks if the <paramref name="methodInfo" /> is an extension method (classic <see langword="this" /> parameter or
	///     declared with the C# extension block syntax).
	/// </summary>
	/// <remarks>
	///     This is an independent re-implementation used to validate the production detection, so it must not call into it.
	/// </remarks>
	/// <param name="methodInfo">The <see cref="MethodInfo" /> to check.</param>
	public static bool IsReallyExtensionMethod(this MethodInfo? methodInfo)
	{
		if (methodInfo is null)
		{
			return false;
		}

		if (methodInfo.IsDefined(typeof(ExtensionAttribute), false))
		{
			return true;
		}

#if NET10_0_OR_GREATER
		Type? declaringType = methodInfo.DeclaringType;
		if (declaringType?.IsDefined(typeof(ExtensionAttribute), false) != true)
		{
			return false;
		}

		return declaringType
			.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
			.Where(nestedType => nestedType.Name.StartsWith("<", StringComparison.Ordinal) &&
			                     nestedType.IsDefined(typeof(ExtensionAttribute), false))
			.SelectMany(groupingType => groupingType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
			                                                    BindingFlags.Static | BindingFlags.DeclaredOnly))
			.Any(skeleton => !skeleton.IsSpecialName &&
			                 string.Equals(skeleton.Name, methodInfo.Name, StringComparison.Ordinal) &&
			                 ParametersMatch(skeleton, methodInfo));
#else
		return false;
#endif
	}

#if NET10_0_OR_GREATER
	private static bool ParametersMatch(MethodInfo skeleton, MethodInfo implementation)
	{
		ParameterInfo[] skeletonParameters = skeleton.GetParameters();
		ParameterInfo[] implementationParameters = implementation.GetParameters();
		if (skeletonParameters.Length != implementationParameters.Length)
		{
			return false;
		}

		for (int i = 0; i < skeletonParameters.Length; i++)
		{
			Type skeletonType = skeletonParameters[i].ParameterType;
			Type implementationType = implementationParameters[i].ParameterType;

			// Generic parameters are named differently on the grouping type ($T0, …) than on the implementation, so
			// compare them by position; compare other types by their full name (falling back to the simple name).
			bool matches = skeletonType.IsGenericParameter || implementationType.IsGenericParameter
				? skeletonType.IsGenericParameter && implementationType.IsGenericParameter &&
				  skeletonType.GenericParameterPosition == implementationType.GenericParameterPosition
				: string.Equals(skeletonType.FullName ?? skeletonType.Name,
					implementationType.FullName ?? implementationType.Name, StringComparison.Ordinal);
			if (!matches)
			{
				return false;
			}
		}

		return true;
	}
#endif

	/// <summary>
	///     Checks if the <paramref name="methodInfo" /> is an operator (e.g. <c>op_Addition</c>, <c>op_Equality</c>, …).
	/// </summary>
	/// <param name="methodInfo">The <see cref="MethodInfo" /> to check.</param>
	public static bool IsReallyOperator(this MethodInfo? methodInfo)
		=> methodInfo is { IsSpecialName: true, }
		   && methodInfo.Name.StartsWith("op_", System.StringComparison.Ordinal);
}
