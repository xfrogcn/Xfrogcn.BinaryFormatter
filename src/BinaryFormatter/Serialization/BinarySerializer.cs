﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xfrogcn.BinaryFormatter.Serialization;

namespace Xfrogcn.BinaryFormatter
{
    public static partial class BinarySerializer
    {
        internal static bool WriteReferenceForObject(
            BinaryConverter binaryConverter,
            object currentValue,
            ref WriteStack state,
            BinaryWriter writer)
        {
            ulong offset = (ulong)(writer.BytesCommitted + writer.BytesPending);
            uint seq = state.ReferenceResolver.GetReference(currentValue, offset, out bool alreadyExists);
            if(alreadyExists)
            {
                // 写引用及引用序号
                writer.WriteByteValue(0xFF);
                writer.WriteUInt32Value(seq);
                
                return true;
            }
            else
            {
                // 标记为非引用
                writer.WriteByteValue(0x00);
                return false;
            }
            
        }


        internal static BinaryPropertyInfo LookupProperty(
            object obj,
            ReadOnlySpan<byte> unescapedPropertyName,
            ref ReadStack state,
            out bool useExtensionProperty,
            bool createExtensionProperty = true)
        {
            Debug.Assert(state.Current.BinaryClassInfo.ClassType == ClassType.Object);

            useExtensionProperty = false;

            BinaryPropertyInfo binaryPropertyInfo = state.Current.BinaryClassInfo.GetProperty(
                unescapedPropertyName,
                ref state.Current,
                out byte[] utf8PropertyName);

            // Increment PropertyIndex so GetProperty() checks the next property first when called again.
            state.Current.PropertyIndex++;

            // For case insensitive and missing property support of JsonPath, remember the value on the temporary stack.
            state.Current.BinaryPropertyName = utf8PropertyName;

            // Determine if we should use the extension property.
            if (binaryPropertyInfo == BinaryPropertyInfo.s_missingProperty)
            {
                BinaryPropertyInfo dataExtProperty = state.Current.BinaryClassInfo.DataExtensionProperty;
                if (dataExtProperty != null && dataExtProperty.HasGetter && dataExtProperty.HasSetter)
                {
                    state.Current.BinaryPropertyNameAsString = BinaryReaderHelper.TranscodeHelper(unescapedPropertyName);

                    if (createExtensionProperty)
                    {
                        CreateDataExtensionProperty(obj, dataExtProperty);
                    }

                    binaryPropertyInfo = dataExtProperty;
                    useExtensionProperty = true;
                }
            }

            state.Current.BinaryPropertyInfo = binaryPropertyInfo;
            return binaryPropertyInfo;
        }

        internal static void CreateDataExtensionProperty(
         object obj,
         BinaryPropertyInfo binaryPropertyInfo)
        {
            Debug.Assert(binaryPropertyInfo != null);

            object extensionData = binaryPropertyInfo.GetValueAsObject(obj);
            if (extensionData == null)
            {
                // Create the appropriate dictionary type. We already verified the types.
#if DEBUG
                Type underlyingIDictionaryType = binaryPropertyInfo.DeclaredPropertyType.GetCompatibleGenericInterface(typeof(IDictionary<,>))!;
                Type[] genericArgs = underlyingIDictionaryType.GetGenericArguments();

                Debug.Assert(underlyingIDictionaryType.IsGenericType);
                Debug.Assert(genericArgs.Length == 2);
                Debug.Assert(genericArgs[0].UnderlyingSystemType == typeof(string));
                Debug.Assert(
                    genericArgs[1].UnderlyingSystemType == BinaryClassInfo.ObjectType ||
                    genericArgs[1].UnderlyingSystemType == typeof(BinaryElement));
#endif
                if (binaryPropertyInfo.RuntimeClassInfo.CreateObject == null)
                {
                    ThrowHelper.ThrowNotSupportedException_SerializationNotSupported(binaryPropertyInfo.DeclaredPropertyType);
                }

                extensionData = binaryPropertyInfo.RuntimeClassInfo.CreateObject();
                binaryPropertyInfo.SetExtensionDictionaryAsObject(obj, extensionData);
            }

            // We don't add the value to the dictionary here because we need to support the read-ahead functionality for Streams.
        }
    }
}
