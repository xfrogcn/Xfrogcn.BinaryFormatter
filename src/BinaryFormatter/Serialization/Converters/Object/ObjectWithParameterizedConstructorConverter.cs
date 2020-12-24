using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using FoundPropertyAsync = System.ValueTuple<Xfrogcn.BinaryFormatter.BinaryPropertyInfo, object, string>;


namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal abstract partial class ObjectWithParameterizedConstructorConverter<T> : ObjectDefaultConverter<T> where T : notnull
    {
        internal sealed override bool OnTryRead(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options, ref ReadStack state, [MaybeNullWhen(false)] out T value)
        {
            object obj;
            ArgumentState argumentState = state.Current.CtorArgumentState!;

            //if (state.UseFastPath)
            //{
            //    // Fast path that avoids maintaining state variables.

            //    ReadOnlySpan<byte> originalSpan = reader.OriginalSpan;

            //    ReadConstructorArguments(ref state, ref reader, options);

            //    obj = CreateObject(ref state.Current);

            //    if (argumentState.FoundPropertyCount > 0)
            //    {
            //        Utf8JsonReader tempReader;

            //        FoundProperty[]? properties = argumentState.FoundProperties;
            //        Debug.Assert(properties != null);

            //        for (int i = 0; i < argumentState.FoundPropertyCount; i++)
            //        {
            //            JsonPropertyInfo jsonPropertyInfo = properties[i].Item1;
            //            long resumptionByteIndex = properties[i].Item3;
            //            byte[]? propertyNameArray = properties[i].Item4;
            //            string? dataExtKey = properties[i].Item5;

            //            tempReader = new Utf8JsonReader(
            //                originalSpan.Slice(checked((int)resumptionByteIndex)),
            //                isFinalBlock: true,
            //                state: properties[i].Item2);

            //            Debug.Assert(tempReader.TokenType == JsonTokenType.PropertyName);

            //            state.Current.JsonPropertyName = propertyNameArray;
            //            state.Current.JsonPropertyInfo = jsonPropertyInfo;

            //            bool useExtensionProperty = dataExtKey != null;

            //            if (useExtensionProperty)
            //            {
            //                Debug.Assert(jsonPropertyInfo == state.Current.JsonClassInfo.DataExtensionProperty);
            //                state.Current.JsonPropertyNameAsString = dataExtKey;
            //                JsonSerializer.CreateDataExtensionProperty(obj, jsonPropertyInfo);
            //            }

            //            ReadPropertyValue(obj, ref state, ref tempReader, jsonPropertyInfo, useExtensionProperty);
            //        }

            //        ArrayPool<FoundProperty>.Shared.Return(argumentState.FoundProperties!, clearArray: true);
            //        argumentState.FoundProperties = null;
            //    }
            //}
            //else
            {
                // Slower path that supports continuation.

                if (state.Current.ObjectState == StackFrameObjectState.None)
                {
                    if (!reader.ReadStartToken())
                    {
                        value = default;
                        return false;
                    }
                    if (reader.TokenType == BinaryTokenType.StartObject)
                    {
                        state.Current.ObjectState = StackFrameObjectState.StartToken;
                    }
                    else if (reader.TokenType == BinaryTokenType.ObjectRef)
                    {
                        // 检查Resolver中是否存在对应id的实例，如果有则直接使用，否则跳转读取
                        state.Current.ObjectState = StackFrameObjectState.GotoRef;

                        value = default;
                        return false;
                    }
                    BeginRead(ref state, ref reader, options);
                    // 初始化中可能会修改状态对象
                    argumentState = state.Current.CtorArgumentState!;
                }

                // 读取构造参数
                if (!ReadConstructorArgumentsWithContinuation(ref state, ref reader, options))
                {
                    value = default;
                    return false;
                }

                obj = CreateObject(ref state.Current);

                if (argumentState.FoundPropertyCount > 0)
                {
                    for (int i = 0; i < argumentState.FoundPropertyCount; i++)
                    {
                        BinaryPropertyInfo binaryPropertyInfo = argumentState.FoundPropertiesAsync![i].Item1;
                        object propValue = argumentState.FoundPropertiesAsync![i].Item2;
                        string dataExtKey = argumentState.FoundPropertiesAsync![i].Item3;

                        if (dataExtKey == null)
                        {
                            binaryPropertyInfo.SetExtensionDictionaryAsObject(obj, propValue);
                        }
                        else
                        {
                            Debug.Assert(binaryPropertyInfo == state.Current.BinaryClassInfo.DataExtensionProperty);

                            //BinarySerializer.CreateDataExtensionProperty(obj, binaryPropertyInfo);
                            //object extDictionary = binaryPropertyInfo.GetValueAsObject(obj)!;

                            //if (extDictionary is IDictionary<string, JsonElement> dict)
                            //{
                            //    dict[dataExtKey] = (JsonElement)propValue!;
                            //}
                            //else
                            //{
                            //    ((IDictionary<string, object>)extDictionary)[dataExtKey] = propValue!;
                            //}
                        }
                    }

                    ArrayPool<FoundPropertyAsync>.Shared.Return(argumentState.FoundPropertiesAsync!, clearArray: true);
                    argumentState.FoundPropertiesAsync = null;
                }
            }

            // Check if we are trying to build the sorted cache.
            //if (state.Current.PropertyRefCache != null)
            //{
            //    state.Current.BinaryClassInfo.UpdateSortedPropertyCache(ref state.Current);
            //}

            // Check if we are trying to build the sorted parameter cache.
            if (argumentState.ParameterRefCache != null)
            {
                state.Current.BinaryClassInfo.UpdateSortedParameterCache(ref state.Current);
            }

            EndRead(ref state);

            value = (T)obj;

            return true;
        }

        protected abstract void InitializeConstructorArgumentCaches(ref ReadStack state, BinarySerializerOptions options);

        protected abstract bool ReadAndCacheConstructorArgument(ref ReadStack state, ref BinaryReader reader, BinaryParameterInfo binaryParameterInfo);

        protected abstract object CreateObject(ref ReadStackFrame frame);

        ///// <summary>
        ///// Performs a full first pass of the JSON input and deserializes the ctor args.
        ///// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private void ReadConstructorArguments(ref ReadStack state, ref BinaryReader reader, BinarySerializerOptions options)
        //{
        //    BeginRead(ref state, ref reader, options);

        //    while (true)
        //    {
        //        // Read the next property name or EndObject.
        //        reader.ReadWithVerify();

        //        JsonTokenType tokenType = reader.TokenType;

        //        if (tokenType == JsonTokenType.EndObject)
        //        {
        //            return;
        //        }

        //        // Read method would have thrown if otherwise.
        //        Debug.Assert(tokenType == JsonTokenType.PropertyName);

        //        if (TryLookupConstructorParameter(ref state, ref reader, options, out JsonParameterInfo? jsonParameterInfo))
        //        {
        //            // Set the property value.
        //            reader.ReadWithVerify();

        //            if (!(jsonParameterInfo!.ShouldDeserialize))
        //            {
        //                reader.Skip();
        //                state.Current.EndConstructorParameter();
        //                continue;
        //            }

        //            ReadAndCacheConstructorArgument(ref state, ref reader, jsonParameterInfo);

        //            state.Current.EndConstructorParameter();
        //        }
        //        else
        //        {
        //            ReadOnlySpan<byte> unescapedPropertyName = JsonSerializer.GetPropertyName(ref state, ref reader, options);
        //            JsonPropertyInfo jsonPropertyInfo = JsonSerializer.LookupProperty(
        //                obj: null!,
        //                unescapedPropertyName,
        //                ref state,
        //                out _,
        //                createExtensionProperty: false);

        //            if (jsonPropertyInfo.ShouldDeserialize)
        //            {
        //                ArgumentState argumentState = state.Current.CtorArgumentState!;

        //                if (argumentState.FoundProperties == null)
        //                {
        //                    argumentState.FoundProperties =
        //                        ArrayPool<FoundProperty>.Shared.Rent(Math.Max(1, state.Current.JsonClassInfo.PropertyCache!.Count));
        //                }
        //                else if (argumentState.FoundPropertyCount == argumentState.FoundProperties.Length)
        //                {
        //                    // Rare case where we can't fit all the JSON properties in the rented pool; we have to grow.
        //                    // This could happen if there are duplicate properties in the JSON.

        //                    var newCache = ArrayPool<FoundProperty>.Shared.Rent(argumentState.FoundProperties.Length * 2);

        //                    argumentState.FoundProperties.CopyTo(newCache, 0);

        //                    ArrayPool<FoundProperty>.Shared.Return(argumentState.FoundProperties, clearArray: true);

        //                    argumentState.FoundProperties = newCache!;
        //                }

        //                argumentState.FoundProperties[argumentState.FoundPropertyCount++] = (
        //                    jsonPropertyInfo,
        //                    reader.CurrentState,
        //                    reader.BytesConsumed,
        //                    state.Current.JsonPropertyName,
        //                    state.Current.JsonPropertyNameAsString);
        //            }

        //            reader.Skip();

        //            state.Current.EndProperty();
        //        }
        //    }
        //}

        private bool ReadConstructorArgumentsWithContinuation(ref ReadStack state, ref BinaryReader reader, BinarySerializerOptions options)
        {
            // Process all properties.
            while (true)
            {
                // Determine the property.
                if (state.Current.PropertyState == StackFramePropertyState.None)
                {
                    if (!reader.ReadPropertyName())
                    {
                        return false;
                    }

                    state.Current.PropertyState = StackFramePropertyState.ReadName;
                }

                BinaryParameterInfo binaryParameterInfo;
                BinaryPropertyInfo binaryPropertyInfo;

                if (state.Current.PropertyState <= StackFramePropertyState.Name)
                {
                    state.Current.PropertyState = StackFramePropertyState.Name;

                    BinaryTokenType tokenType = reader.TokenType;

                    if (tokenType == BinaryTokenType.EndObject)
                    {
                        return true;
                    }

                    // Read method would have thrown if otherwise.
                    Debug.Assert(tokenType == BinaryTokenType.PropertyName);
                    ushort propertySeq = reader.CurrentPropertySeq;
                    BinaryMemberInfo mi = state.GetMemberInfo(propertySeq);
                    Debug.Assert(mi != null);


                    if (TryLookupConstructorParameter(
                        ref state,
                        ref reader,
                        mi,
                        options,
                        out binaryParameterInfo))
                    {
                        binaryPropertyInfo = null;
                    }
                    else
                    {
                        binaryPropertyInfo = BinarySerializer.LookupProperty(
                            obj: null!,
                            mi.NameAsUtf8Bytes,
                            ref state,
                            out bool useExtensionProperty,
                            createExtensionProperty: false);

                        state.Current.UseExtensionProperty = useExtensionProperty;
                    }
                }
                else
                {
                    binaryParameterInfo = state.Current.CtorArgumentState!.BinaryParameterInfo;
                    binaryPropertyInfo = state.Current.BinaryPropertyInfo;
                }

                if (binaryParameterInfo != null)
                {
                    Debug.Assert(binaryPropertyInfo == null);

                    if (!HandleConstructorArgumentWithContinuation(ref state, ref reader, binaryParameterInfo))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!HandlePropertyWithContinuation(ref state, ref reader, binaryPropertyInfo!))
                    {
                        return false;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HandleConstructorArgumentWithContinuation(
            ref ReadStack state,
            ref BinaryReader reader,
            BinaryParameterInfo binaryParameterInfo)
        {
            if (state.Current.PropertyState < StackFramePropertyState.ReadValue)
            {
                if (!binaryParameterInfo.ShouldDeserialize)
                {
                    if (!reader.TrySkip(state.Options))
                    {
                        return false;
                    }

                    state.Current.EndConstructorParameter();
                    return true;
                }

                // Returning false below will cause the read-ahead functionality to finish the read.
                state.Current.PropertyState = StackFramePropertyState.ReadValue;

                //if (!SingleValueReadWithReadAhead(binaryParameterInfo.ConverterBase.ClassType, ref reader, ref state))
                //{
                //    return false;
                //}
            }

            if (!ReadAndCacheConstructorArgument(ref state, ref reader, binaryParameterInfo))
            {
                return false;
            }

            state.Current.EndConstructorParameter();
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HandlePropertyWithContinuation(
            ref ReadStack state,
            ref BinaryReader reader,
            BinaryPropertyInfo binaryPropertyInfo)
        {
            if (state.Current.PropertyState < StackFramePropertyState.ReadValue)
            {
                if (!binaryPropertyInfo.ShouldDeserialize)
                {
                    if (!reader.TrySkip(state.Options))
                    {
                        return false;
                    }

                    state.Current.EndProperty();
                    return true;
                }

                //if (!ReadAheadPropertyValue(ref state, ref reader, binaryPropertyInfo))
                //{
                //    return false;
                //}
            }

            object propValue = default;

            if (state.Current.UseExtensionProperty)
            {
                //if (!binaryPropertyInfo.ReadJsonExtensionDataValue(ref state, ref reader, out propValue))
                //{
                //    return false;
                //}
            }
            else
            {
                if (!binaryPropertyInfo.ReadBinaryAsObject(ref state, ref reader, out propValue))
                {
                    return false;
                }
            }

            Debug.Assert(binaryPropertyInfo.ShouldDeserialize);

            // Ensure that the cache has enough capacity to add this property.

            ArgumentState argumentState = state.Current.CtorArgumentState!;

            if (argumentState.FoundPropertiesAsync == null)
            {
                argumentState.FoundPropertiesAsync = ArrayPool<FoundPropertyAsync>.Shared.Rent(Math.Max(1, state.Current.BinaryClassInfo.PropertyCache!.Count));
            }
            else if (argumentState.FoundPropertyCount == argumentState.FoundPropertiesAsync!.Length)
            {
                var newCache = ArrayPool<FoundPropertyAsync>.Shared.Rent(argumentState.FoundPropertiesAsync!.Length * 2);

                argumentState.FoundPropertiesAsync!.CopyTo(newCache, 0);

                ArrayPool<FoundPropertyAsync>.Shared.Return(argumentState.FoundPropertiesAsync!, clearArray: true);

                argumentState.FoundPropertiesAsync = newCache!;
            }

            // Cache the property name and value.
            argumentState.FoundPropertiesAsync![argumentState.FoundPropertyCount++] = (
                binaryPropertyInfo,
                propValue,
                state.Current.BinaryPropertyNameAsString);

            state.Current.EndProperty();
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BeginRead(ref ReadStack state, ref BinaryReader reader, BinarySerializerOptions options)
        {
            if (reader.TokenType != BinaryTokenType.StartObject)
            {
                ThrowHelper.ThrowBinaryException_DeserializeUnableToConvertValue(TypeToConvert);
            }

            

            // Set current JsonPropertyInfo to null to avoid conflicts on push.
            state.Current.BinaryPropertyInfo = null;

            

            InitializeConstructorArgumentCaches(ref state, options);

            Debug.Assert(state.Current.CtorArgumentState != null);

            if (state.Current.BinaryClassInfo.ParameterCount != state.Current.BinaryClassInfo.ParameterCache!.Count)
            {
                ThrowHelper.ThrowInvalidOperationException_ConstructorParameterIncompleteBinding(ConstructorInfo!, TypeToConvert);
            }
        }

        protected virtual void EndRead(ref ReadStack state) { }

        /// <summary>
        /// Lookup the constructor parameter given its name in the reader.
        /// </summary>
        protected virtual bool TryLookupConstructorParameter(
            ref ReadStack state,
            ref BinaryReader reader,
            BinaryMemberInfo mi,
            BinarySerializerOptions options,
            out BinaryParameterInfo binaryParameterInfo)
        {
            Debug.Assert(state.Current.BinaryClassInfo.ClassType == ClassType.Object);

            ReadOnlySpan<byte> unescapedPropertyName = mi.NameAsUtf8Bytes;

            binaryParameterInfo = state.Current.BinaryClassInfo.GetParameter(
                unescapedPropertyName,
                ref state.Current,
                out byte[] utf8PropertyName);

            // Increment ConstructorParameterIndex so GetParameter() checks the next parameter first when called again.
            state.Current.CtorArgumentState!.ParameterIndex++;

            // For case insensitive and missing property support of JsonPath, remember the value on the temporary stack.
            state.Current.BinaryPropertyName = utf8PropertyName;

            state.Current.CtorArgumentState.BinaryParameterInfo = binaryParameterInfo;

            return binaryParameterInfo != null;
        }

        internal override bool ConstructorIsParameterized => true;
    }
}
