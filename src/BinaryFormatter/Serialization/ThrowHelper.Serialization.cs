﻿using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xfrogcn.BinaryFormatter.Resources;
using Xfrogcn.BinaryFormatter.Serialization;

namespace Xfrogcn.BinaryFormatter
{
    internal static partial class ThrowHelper
    {
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowInvalidOperationException_CannotSerializeInvalidType(Type type, Type parentClassType, MemberInfo memberInfo)
        {
            if (parentClassType == null)
            {
                Debug.Assert(memberInfo == null);
                throw new InvalidOperationException(string.Format(Strings.CannotSerializeInvalidType, type));
            }

            Debug.Assert(memberInfo != null);
            throw new InvalidOperationException(string.Format(Strings.CannotSerializeInvalidMember, type, memberInfo.Name, parentClassType));
        }



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
            var ex = new BinaryException(string.Format(Strings.DeserializeUnableToConvertValue, propertyType));
            ex.AppendPathInformation = true;
            throw ex;
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ReThrowWithPath(in WriteStack state, Exception ex)
        {
            BinaryException binaryException = new BinaryException(null, ex);
            AddBinaryExceptionInformation(state, binaryException);
            throw binaryException;
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ReThrowWithPath(in ReadStack state, BinaryReaderException ex)
        {
            Debug.Assert(ex.Path == null);

            string path = "";//state.JsonPath();
            string message = ex.Message;

            //// Insert the "Path" portion before "LineNumber" and "BytePositionInLine".
            //int iPos = message.LastIndexOf(" LineNumber: ", StringComparison.InvariantCulture);
            //if (iPos >= 0)
            //{
            //    message = $"{message.Substring(0, iPos)} Path: {path} |{message.Substring(iPos)}";
            //}
            //else
            //{
            //    message += $" Path: {path}.";
            //}

            throw new BinaryException(message, path, ex.BytePosition, ex);
        }


        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ReThrowWithPath(in ReadStack state, in BinaryReader reader, Exception ex)
        {
            BinaryException binaryException = new BinaryException(null, ex);
            AddBinaryExceptionInformation(state, reader, binaryException);
            throw binaryException;
        }

        public static void AddBinaryExceptionInformation(in ReadStack state, in BinaryReader reader, BinaryException ex)
        {
           

            //long bytePositionInLine = reader.CurrentState._bytePositionInLine;
            //ex.BytePositionInLine = bytePositionInLine;

            //string path = state.JsonPath();
            //ex.Path = path;

            //string? message = ex._message;

            //if (string.IsNullOrEmpty(message))
            //{
            //    // Use a default message.
            //    Type? propertyType = state.Current.JsonPropertyInfo?.RuntimePropertyType;
            //    if (propertyType == null)
            //    {
            //        propertyType = state.Current.JsonClassInfo?.Type;
            //    }

            //    message = SR.Format(SR.DeserializeUnableToConvertValue, propertyType);
            //    ex.AppendPathInformation = true;
            //}

            //if (ex.AppendPathInformation)
            //{
            //    message += $" Path: {path} | LineNumber: {lineNumber} | BytePositionInLine: {bytePositionInLine}.";
            //    ex.SetMessage(message);
            //}
        }

        public static void AddBinaryExceptionInformation(in WriteStack state, BinaryException ex)
        {
            //string path = state.PropertyPath();
            //ex.Path = path;

            //string? message = ex._message;
            //if (string.IsNullOrEmpty(message))
            //{
            //    // Use a default message.
            //    message = string.Format(Strings.SerializeUnableToSerialize);
            //    ex.AppendPathInformation = true;
            //}

            //if (ex.AppendPathInformation)
            //{
            //    message += $" Path: {path}.";
            //    ex.SetMessage(message);
            //}
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException(in WriteStack state, NotSupportedException ex)
        {
            //string message = ex.Message;

            //// The caller should check to ensure path is not already set.
            //Debug.Assert(!message.Contains(" Path: "));

            //// Obtain the type to show in the message.
            //Type propertyType = state.Current.DeclaredJsonPropertyInfo?.RuntimePropertyType;
            //if (propertyType == null)
            //{
            //    propertyType = state.Current.JsonClassInfo.Type;
            //}

            //if (!message.Contains(propertyType.ToString()))
            //{
            //    if (message.Length > 0)
            //    {
            //        message += " ";
            //    }

            //    message += string.Format(Strings.SerializationNotSupportedParentType, propertyType);
            //}

            //message += $" Path: {state.PropertyPath()}.";

            //throw new NotSupportedException(message, ex);
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException(in ReadStack state, in BinaryReader reader, NotSupportedException ex)
        {
            string message = ex.Message;

            //// The caller should check to ensure path is not already set.
            //Debug.Assert(!message.Contains(" Path: "));

            //// Obtain the type to show in the message.
            //Type? propertyType = state.Current.JsonPropertyInfo?.RuntimePropertyType;
            //if (propertyType == null)
            //{
            //    propertyType = state.Current.JsonClassInfo.Type;
            //}

            //if (!message.Contains(propertyType.ToString()))
            //{
            //    if (message.Length > 0)
            //    {
            //        message += " ";
            //    }

            //    message += SR.Format(SR.SerializationNotSupportedParentType, propertyType);
            //}

            //long lineNumber = reader.CurrentState._lineNumber;
            //long bytePositionInLine = reader.CurrentState._bytePositionInLine;
            //message += $" Path: {state.JsonPath()} | LineNumber: {lineNumber} | BytePositionInLine: {bytePositionInLine}.";

            throw new NotSupportedException(message, ex);
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowNotSupportedException_SerializationNotSupported(Type propertyType)
        {
            throw new NotSupportedException(string.Format(Strings.SerializationNotSupportedType, propertyType));
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowInvalidOperationException_SerializationDuplicateAttribute(Type attribute, Type classType, MemberInfo? memberInfo)
        {
            string location = classType.ToString();
            if (memberInfo != null)
            {
                location += $".{memberInfo.Name}";
            }

            throw new InvalidOperationException(string.Format(Strings.SerializationDuplicateAttribute, attribute, location));
        }


        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowInvalidOperationException_ConverterCanConvertNullableRedundant(Type runtimePropertyType, BinaryConverter binaryConverter)
        {
            throw new InvalidOperationException(string.Format(Strings.ConverterCanConvertNullableRedundant, binaryConverter.GetType(), binaryConverter.TypeToConvert, runtimePropertyType));
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowInvalidOperationException_SerializationConverterOnAttributeNotCompatible(Type classTypeAttributeIsOn, MemberInfo? memberInfo, Type typeToConvert)
        {
            string location = classTypeAttributeIsOn.ToString();

            if (memberInfo != null)
            {
                location += $".{memberInfo.Name}";
            }

            throw new InvalidOperationException(string.Format(Strings.SerializationConverterOnAttributeNotCompatible, location, typeToConvert));
        }


        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowInvalidOperationException_SerializationConverterOnAttributeInvalid(Type classType, MemberInfo? memberInfo)
        {
            string location = classType.ToString();
            if (memberInfo != null)
            {
                location += $".{memberInfo.Name}";
            }

            throw new InvalidOperationException(string.Format(Strings.SerializationConverterOnAttributeInvalid, location));
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowInvalidOperationException_SerializationConverterNotCompatible(Type converterType, Type type)
        {
            throw new InvalidOperationException(string.Format(Strings.SerializationConverterNotCompatible, converterType, type));
        }


        [DoesNotReturn]
        public static void ThrowInvalidOperationException_SerializerConverterFactoryReturnsNull(Type converterType)
        {
            throw new InvalidOperationException(string.Format(Strings.SerializerConverterFactoryReturnsNull, converterType));
        }
    }
}
