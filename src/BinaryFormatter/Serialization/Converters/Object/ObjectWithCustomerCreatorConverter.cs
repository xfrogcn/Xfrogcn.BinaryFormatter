using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal abstract class ObjectWithCustomerCreatorConverter<T> : ObjectDefaultConverter<T>
        where T:notnull
    {
        internal sealed override bool OnTryRead(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options, ref ReadStack state, [MaybeNullWhen(false)] out T value)
        {
            object obj = default;

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

                    RefState refState = BinarySerializer.ReadReferenceForObject(this, ref state, ref reader, out object refValue);
                    if (refState == RefState.None)
                    {
                        state.Current.ObjectState = StackFrameObjectState.StartToken;
                        BeginRead(ref state, ref reader, options);


                    }
                    else if (refState == RefState.Created)
                    {
                        state.Current.ObjectState = StackFrameObjectState.CreatedObject;
                        obj = (T)refValue;
                    }
                    else
                    {
                        value = default;
                        return false;
                    }
                    //if (reader.TokenType == BinaryTokenType.StartObject)
                    //{
                    //    state.Current.ObjectState = StackFrameObjectState.StartToken;
                    //}
                    //else if (reader.TokenType == BinaryTokenType.ObjectRef)
                    //{
                    //    // 检查Resolver中是否存在对应id的实例，如果有则直接使用，否则跳转读取
                    //    state.Current.ObjectState = StackFrameObjectState.GotoRef;

                    //    value = default;
                    //    return false;
                    //}
                   
                }



                // 读取构造参数
                if (!ReadCreatorArgumentsWithContinuation(ref state, ref reader, options))
                {
                    value = default;
                    return false;
                }

                if( state.Current.ObjectState< StackFrameObjectState.CreatedObject)
                {
                    obj = CreateObject(ref state.Current);
                    state.ReferenceResolver.AddReferenceObject(state.Current.RefId, obj);
                }
               

            }


            EndRead(ref state);

            value = (T)obj;

            return true;
        }

        protected abstract void InitializeCreatorArgumentCaches(ref ReadStack state, BinarySerializerOptions options);

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

        private bool ReadCreatorArgumentsWithContinuation(ref ReadStack state, ref BinaryReader reader, BinarySerializerOptions options)
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


                    binaryPropertyInfo = BinarySerializer.LookupProperty(
                        obj: null!,
                        mi.NameAsUtf8Bytes,
                        ref state,
                        out bool useExtensionProperty,
                        createExtensionProperty: false);

                    state.Current.UseExtensionProperty = useExtensionProperty;

                }
                else
                {
                    binaryPropertyInfo = state.Current.BinaryPropertyInfo;
                }


                if (!HandlePropertyWithContinuation(ref state, ref reader, binaryPropertyInfo!))
                {
                    return false;
                }

            }
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

        
            Dictionary<string, object> argumentState = state.Current.PropertyValueCache!;
            if (argumentState == null)
            {
                state.Current.PropertyValueCache = new Dictionary<string, object>(state.Current.BinaryClassInfo.PropertyCacheArray.Length);
                argumentState = state.Current.PropertyValueCache;
            }

            argumentState[binaryPropertyInfo.NameAsString] = propValue;
          
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

            // Set current BinaryPropertyInfo to null to avoid conflicts on push.
            state.Current.BinaryPropertyInfo = null;


            InitializeCreatorArgumentCaches(ref state, options);
       
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

        internal override bool ConstructorIsParameterized => false;
    }
}
