using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using FoundPropertyAsync = System.ValueTuple<Xfrogcn.BinaryFormatter.BinaryPropertyInfo, object, string>;
using FoundProperty = System.ValueTuple<Xfrogcn.BinaryFormatter.BinaryPropertyInfo, Xfrogcn.BinaryFormatter.BinaryReaderState, long, byte[], string>;


namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal abstract partial class ObjectWithParameterizedConstructorConverter<T> : ObjectDefaultConverter<T> where T : notnull
    {
        internal sealed override bool OnTryRead(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options, ref ReadStack state, [MaybeNullWhen(false)] out T value)
        {
            object obj;
            ArgumentState argumentState = state.Current.CtorArgumentState!;


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
                    // 初始化中可能会修改状态对象
                    argumentState = state.Current.CtorArgumentState!;
                }
                else
                {
                    state.Current.ObjectState = StackFrameObjectState.CreatedObject;
                    state.Current.ReturnValue = refValue;
                    state.Current.RefState = refState;
                }

            }

            // 读取构造参数
            if (!ReadConstructorArgumentsWithContinuation(ref state, ref reader, options))
            {
                value = default;
                return false;
            }
            if (state.Current.ObjectState < StackFrameObjectState.CreatedObject)
            {
                obj = CreateObject(ref state.Current);
                state.ReferenceResolver.AddReferenceObject(state.Current.RefId, obj);
            }
            else
            {
                obj = state.Current.ReturnValue;
            }

            if (argumentState.FoundPropertyCount > 0)
            {
                for (int i = 0; i < argumentState.FoundPropertyCount; i++)
                {
                    BinaryPropertyInfo binaryPropertyInfo = argumentState.FoundPropertiesAsync![i].Item1;
                    object propValue = argumentState.FoundPropertiesAsync![i].Item2;
                    string dataExtKey = argumentState.FoundPropertiesAsync![i].Item3;

                    if (dataExtKey == null)
                    {
                        binaryPropertyInfo.SetExtensionDictionaryAsObject(ref state, obj, propValue);
                    }
                    else
                    {
                        Debug.Assert(binaryPropertyInfo == state.Current.BinaryClassInfo.DataExtensionProperty);

                        // TODO 扩展属性
                    }
                }

                ArrayPool<FoundPropertyAsync>.Shared.Return(argumentState.FoundPropertiesAsync!, clearArray: true);
                argumentState.FoundPropertiesAsync = null;
            }


            // Check if we are trying to build the sorted parameter cache.
            if (argumentState.ParameterRefCache != null)
            {
                state.Current.BinaryClassInfo.UpdateSortedParameterCache(ref state.Current);
            }

            EndRead(ref state);

            if (state.Current.RefState != RefState.Start)
            {
                value = (T)obj;
            }
            else
            {
                value = default;
            }
            return true;
        }

        protected abstract void InitializeConstructorArgumentCaches(ref ReadStack state, BinarySerializerOptions options);

        protected abstract bool ReadAndCacheConstructorArgument(ref ReadStack state, ref BinaryReader reader, BinaryParameterInfo binaryParameterInfo);

        protected abstract object CreateObject(ref ReadStackFrame frame);

        

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
                // TODO 扩展属性
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

            

            // Set current BinaryPropertyInfo to null to avoid conflicts on push.
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

            // For case insensitive and missing property support of BinaryPath, remember the value on the temporary stack.
            state.Current.BinaryPropertyName = utf8PropertyName;

            state.Current.CtorArgumentState.BinaryParameterInfo = binaryParameterInfo;

            return binaryParameterInfo != null;
        }

        internal override bool ConstructorIsParameterized => true;
    }
}
