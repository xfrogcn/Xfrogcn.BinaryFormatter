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

            }



            // 读取构造参数
            if (!ReadCreatorArgumentsWithContinuation(ref state, ref reader, options))
            {
                value = default;
                return false;
            }

            if (state.Current.ObjectState < StackFrameObjectState.CreatedObject)
            {
                obj = CreateObject(ref state.Current);
                state.ReferenceResolver.AddReferenceObject(state.Current.RefId, obj);
            }





            EndRead(ref state);

            value = (T)obj;

            return true;
        }

        protected abstract void InitializeCreatorArgumentCaches(ref ReadStack state, BinarySerializerOptions options);

        protected abstract object CreateObject(ref ReadStackFrame frame);

        [SuppressMessage("Style", "IDE0060:删除未使用的参数", Justification = "<挂起>")]
        private bool ReadCreatorArgumentsWithContinuation(ref ReadStack state,
                                                          ref BinaryReader reader,
                                                          BinarySerializerOptions options)
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

            // For case insensitive and missing property support of BinaryPath, remember the value on the temporary stack.
            state.Current.BinaryPropertyName = utf8PropertyName;

            state.Current.CtorArgumentState.BinaryParameterInfo = binaryParameterInfo;

            return binaryParameterInfo != null;
        }

        internal override bool ConstructorIsParameterized => false;
    }
}
