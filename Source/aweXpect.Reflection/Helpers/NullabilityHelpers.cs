using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace aweXpect.Reflection.Helpers;

/// <summary>
///     Helper methods to determine the nullability of members.
/// </summary>
/// <remarks>
///     A member is considered nullable if its type is a <see cref="Nullable{T}" /> value type or a reference
///     type whose declared annotation is nullable (e.g. <c>string?</c>). The declared annotation is decoded
///     from the <c>NullableAttribute</c> / <c>NullableContextAttribute</c> metadata on all target frameworks,
///     so the behavior cannot diverge between them: members without annotations (oblivious code) and
///     unconstrained generic type parameters count as non-nullable, and post-condition attributes like
///     <c>[AllowNull]</c> or <c>[MaybeNull]</c> are ignored.
/// </remarks>
internal static class NullabilityHelpers
{
	/// <summary>
	///     Checks if the <paramref name="propertyInfo" /> is nullable.
	/// </summary>
	/// <remarks>
	///     A property is considered nullable if its type is a <see cref="Nullable{T}" /> value type or a
	///     reference type annotated as nullable (according to the nullable reference type metadata).
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

		return IsNullableReferenceType(propertyInfo);
	}

	/// <summary>
	///     Checks if the <paramref name="fieldInfo" /> is nullable.
	/// </summary>
	/// <remarks>
	///     A field is considered nullable if its type is a <see cref="Nullable{T}" /> value type or a
	///     reference type annotated as nullable (according to the nullable reference type metadata).
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

		return IsNullableReferenceType(fieldInfo);
	}

	/// <summary>
	///     Returns the nullable fields and properties of the <paramref name="type" />.
	/// </summary>
	public static MemberInfo[] GetNullableMembers(this Type type)
		=> type.GetMembersByNullability().Nullable;

	/// <summary>
	///     Returns the non-nullable fields and properties of the <paramref name="type" />.
	/// </summary>
	public static MemberInfo[] GetNotNullableMembers(this Type type)
		=> type.GetMembersByNullability().NotNullable;

	/// <summary>
	///     Partitions the declared fields and properties of the <paramref name="type" /> into nullable and
	///     non-nullable members in a single pass.
	/// </summary>
	public static (MemberInfo[] Nullable, MemberInfo[] NotNullable) GetMembersByNullability(this Type type)
	{
		List<MemberInfo> nullable = [];
		List<MemberInfo> notNullable = [];
		foreach (FieldInfo field in type.GetDeclaredFields())
		{
			(field.IsNullable() ? nullable : notNullable).Add(field);
		}

		foreach (PropertyInfo property in type.GetDeclaredProperties())
		{
			(property.IsNullable() ? nullable : notNullable).Add(property);
		}

		return (nullable.ToArray(), notNullable.ToArray());
	}

	/// <summary>
	///     Determines the nullability of a reference type member from the nullable reference type metadata.
	/// </summary>
	/// <remarks>
	///     The compiler stores the annotation in a <c>NullableAttribute</c> on the member (a scalar
	///     <see cref="byte" /> or a <see cref="byte" /> array whose first element describes the top-level type)
	///     and omits it when the value equals the context stored in a <c>NullableContextAttribute</c> on one of
	///     the declaring types. A flag value of <c>2</c> means "annotated" (nullable).
	///     <para />
	///     The metadata is decoded directly (instead of using <c>NullabilityInfoContext</c>, which is unavailable
	///     on netstandard2.0) so that the same member yields the same result on every target framework.
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

				return argument.Value is IReadOnlyList<CustomAttributeTypedArgument> { Count: > 0, } flags &&
				       flags[0].Value is byte firstFlag &&
				       firstFlag == annotated;
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
}
