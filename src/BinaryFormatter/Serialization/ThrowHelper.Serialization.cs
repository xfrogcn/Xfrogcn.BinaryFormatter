using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Xfrogcn.BinaryFormatter.Resources;

namespace Xfrogcn.BinaryFormatter
{
    internal static partial class ThrowHelper
    {
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowInvalidOperationException_SerializerPropertyNameNull(Type parentType, BinaryPropertyInfo binaryPropertyInfo)
        {
            throw new InvalidOperationException(string.Format(Strings.SerializerPropertyNameNull, parentType, binaryPropertyInfo.MemberInfo?.Name));
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowInvalidOperationException_IgnoreConditionOnValueTypeInvalid(BinaryPropertyInfo binaryPropertyInfo)
        {
            MemberInfo memberInfo = binaryPropertyInfo.MemberInfo!;
            throw new InvalidOperationException(string.Format(Strings.IgnoreConditionOnValueTypeInvalid, memberInfo.Name, memberInfo.DeclaringType));
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowJsonException_DeserializeUnableToConvertValue(Type propertyType)
        {
            var ex = new JsonException(SR.Format(SR.DeserializeUnableToConvertValue, propertyType));
            ex.AppendPathInformation = true;
            throw ex;
        }
    }
}
