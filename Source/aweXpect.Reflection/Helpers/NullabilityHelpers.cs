using System;
using System.Linq;
using System.Reflection;
#if !NET8_0_OR_GREATER
using System.Collections.Generic;
#endif

namespace aweXpect.Reflection.Helpers;

/// <summary>
///     Helper methods to determine the nullability of members.
/// </summary>
internal static class NullabilityHelpers
{
	/// <summary>
	///     Checks if the <paramref name="propertyInfo" /> is nullable.
	/// </summary>
	/// <remarks>
	///     A property is considered nullable if its type is a <see cref="Nullable{T}" /> value type or a reference
	///     type annotated as nullable (according to the nullable reference type metadata).
	/// </remarks>
	public static bool IsNullable(this PropertyInfo? propertyInfo)
	{
		if (propertyInfo is null)
		{
			return false;
		}

		if (propertyInfo.PropertyType.IsValueType)
		{
			return Nullable.GetUnderlyingType(propertyInfo.PropertyType) != null;
		}

#if NET8_0_OR_GREATER
		NullabilityInfo nullabilityInfo = new NullabilityInfoContext().Create(propertyInfo);
		return nullabilityInfo.ReadState == NullabilityState.Nullable ||
		       nullabilityInfo.WriteState == NullabilityState.Nullable;
#else
		return IsNullableReferenceType(propertyInfo);
#endif
	}

	/// <summary>
	///     Checks if the <paramref name="fieldInfo" /> is nullable.
	/// </summary>
	/// <remarks>
	///     A field is considered nullable if its type is a <see cref="Nullable{T}" /> value type or a reference
	///     type annotated as nullable (according to the nullable reference type metadata).
	/// </remarks>
	public static bool IsNullable(this FieldInfo? fieldInfo)
	{
		if (fieldInfo is null)
		{
			return false;
		}

		if (fieldInfo.FieldType.IsValueType)
		{
			return Nullable.GetUnderlyingType(fieldInfo.FieldType) != null;
		}

#if NET8_0_OR_GREATER
		NullabilityInfo nullabilityInfo = new NullabilityInfoContext().Create(fieldInfo);
		return nullabilityInfo.ReadState == NullabilityState.Nullable ||
		       nullabilityInfo.WriteState == NullabilityState.Nullable;
#else
		return IsNullableReferenceType(fieldInfo);
#endif
	}

	/// <summary>
	///     Returns the nullable fields and properties of the <paramref name="type" />.
	/// </summary>
	public static MemberInfo[] GetNullableMembers(this Type type)
		=> type.GetDeclaredFields().Where(field => field.IsNullable()).Cast<MemberInfo>()
			.Concat(type.GetDeclaredProperties().Where(property => property.IsNullable()))
			.ToArray();

	/// <summary>
	///     Returns the non-nullable fields and properties of the <paramref name="type" />.
	/// </summary>
	public static MemberInfo[] GetNotNullableMembers(this Type type)
		=> type.GetDeclaredFields().Where(field => !field.IsNullable()).Cast<MemberInfo>()
			.Concat(type.GetDeclaredProperties().Where(property => !property.IsNullable()))
			.ToArray();

#if !NET8_0_OR_GREATER
	/// <summary>
	///     Determines the nullability of a reference type member from the nullable reference type metadata,
	///     as <c>NullabilityInfoContext</c> is unavailable on this target framework.
	/// </summary>
	/// <remarks>
	///     The compiler stores the annotation in a <c>NullableAttribute</c> on the member (a scalar
	///     <see cref="byte" /> or a <see cref="byte" /> array whose first element describes the top-level type)
	///     and omits it when the value equals the context stored in a <c>NullableContextAttribute</c> on one of
	///     the declaring types. A flag value of <c>2</c> means "annotated" (nullable).
	/// </remarks>
	private static bool IsNullableReferenceType(MemberInfo memberInfo)
	{
		const byte annotated = 2;
		const string nullableAttributeName = "System.Runtime.CompilerServices.NullableAttribute";
		const string nullableContextAttributeName = "System.Runtime.CompilerServices.NullableContextAttribute";

		foreach (CustomAttributeData attributeData in memberInfo.CustomAttributes)
		{
			if (attributeData.AttributeType.FullName == nullableAttributeName &&
			    attributeData.ConstructorArguments.Count == 1)
			{
				CustomAttributeTypedArgument argument = attributeData.ConstructorArguments[0];
				if (argument.Value is byte flag)
				{
					return flag == annotated;
				}

				if (argument.Value is IReadOnlyList<CustomAttributeTypedArgument> { Count: > 0, } flags &&
				    flags[0].Value is byte firstFlag)
				{
					return firstFlag == annotated;
				}
			}
		}

		for (Type? declaringType = memberInfo.DeclaringType;
		     declaringType is not null;
		     declaringType = declaringType.DeclaringType)
		{
			foreach (CustomAttributeData attributeData in declaringType.CustomAttributes)
			{
				if (attributeData.AttributeType.FullName == nullableContextAttributeName &&
				    attributeData.ConstructorArguments.Count == 1 &&
				    attributeData.ConstructorArguments[0].Value is byte contextFlag)
				{
					return contextFlag == annotated;
				}
			}
		}

		return false;
	}
#endif
}
