using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal abstract class IEnumerableDefaultConverter<TCollection, TElement>
        : BinaryCollectionConverter<TCollection, TElement>
    {
        protected abstract void Add(in TElement value, ref ReadStack state);
        protected abstract void CreateCollection(ref BinaryReader reader, ref ReadStack state, BinarySerializerOptions options, ulong len);

        protected virtual void ConvertCollection(ref ReadStack state, BinarySerializerOptions options) { }

        protected static BinaryConverter<TElement> GetElementConverter(BinaryClassInfo elementClassInfo)
        {
            BinaryConverter<TElement> converter = (BinaryConverter<TElement>)elementClassInfo.PropertyInfoForClassInfo.ConverterBase;
            Debug.Assert(converter != null); // It should not be possible to have a null converter at this point.

            return converter;
        }

        protected static BinaryConverter<TElement> GetElementConverter(ref WriteStack state)
        {
            BinaryConverter<TElement> converter = (BinaryConverter<TElement>)state.Current.DeclaredBinaryPropertyInfo!.ConverterBase;
            Debug.Assert(converter != null); // It should not be possible to have a null converter at this point.

            return converter;
        }


        internal override bool OnTryRead(
             ref BinaryReader reader,
             Type typeToConvert,
             BinarySerializerOptions options,
             ref ReadStack state,
             [MaybeNullWhen(false)] out TCollection value)
        {
            BinaryClassInfo elementClassInfo = state.Current.BinaryClassInfo.ElementClassInfo!;

            if (state.UseFastPath)
            {

                // 刚进入对象读取
                if (reader.CurrentTypeInfo == null || reader.CurrentTypeInfo.SerializeType != ClassType.Enumerable)
                {
                    ThrowHelper.ThrowBinaryException_DeserializeUnableToConvertValue(TypeToConvert);
                }

                // 读取引用标记
                reader.AheadReadStartToken();


                RefState refState = BinarySerializer.ReadReferenceForObject(this, ref state, ref reader, out object refValue);
                if (refState == RefState.None)
                {
                    state.Current.ObjectState = StackFrameObjectState.StartToken;

                    // 读取枚举集合索引的字节长度（根据总长度，可能为1、2、4、8字节长度）

                    // 读取枚举长度
                    reader.AheadReadBytes(1);
                    state.Current.EnumerableIndexBytes = reader.ValueSpan[0];

                    // 读取枚举集合的总长度
                    reader.AheadReadBytes(state.Current.EnumerableIndexBytes);

                    state.Current.EnumerableLength = reader.GetEnumerableLength(reader.ValueSpan);
                    state.Current.EnumerableIndex = 0;


                    CreateCollection(ref reader, ref state, options, state.Current.EnumerableLength);
                    state.Current.BinaryPropertyInfo = state.Current.BinaryClassInfo.ElementClassInfo!.PropertyInfoForClassInfo;

                    BinaryConverter<TElement> elementConverter = GetElementConverter(elementClassInfo);
                    BinaryConverter converter = elementConverter;

                    // Process all elements.
                    while (state.Current.EnumerableIndex < state.Current.EnumerableLength)
                    {
                        // ReadName --> 读取索引

                        reader.AheadReadBytes(state.Current.EnumerableIndexBytes);

                        // 读取项的类型
                        reader.AheadReadTypeSeq();

                        if (reader.TokenType == BinaryTokenType.Null)
                        {
                            Add(default, ref state);
                            state.Current.EndElement();
                            continue;
                        }

                        if (elementConverter.CanBePolymorphic)
                        {
                            Type t = state.TypeMap.GetType(reader.CurrentTypeInfo.Seq);
                            if (state.Current.PropertyPolymorphicConverter != null && t == state.Current.PropertyPolymorphicConverter.TypeToConvert)
                            {
                                converter = state.Current.PropertyPolymorphicConverter;
                            }
                            else if (t != null && t != elementConverter.TypeToConvert && elementConverter.TypeToConvert.IsAssignableFrom(t) )
                            {
                                converter = options.GetConverter(t);
                                state.Current.PropertyPolymorphicConverter = converter;
                                state.Current.PolymorphicBinaryClassInfo = options.GetOrAddClass(t);
                            }
                            else
                            {
                                converter = elementConverter;
                                state.Current.PropertyPolymorphicConverter = null;
                                state.Current.PolymorphicBinaryClassInfo = null;
                            }
                        }




                        TElement element;
                        if (converter is BinaryConverter<TElement> typedConverter)
                        {
                            typedConverter.TryRead(ref reader, typeof(TElement), options, ref state, out _, out element);
                        }
                        else
                        {
                            converter.TryReadAsObject(ref reader, options, ref state, out object ntElement);
                            element = (TElement)ntElement;
                        }

                        Add(element!, ref state);

                        state.Current.EndElement();

                    }

                    state.Current.EndProperty();

                    // 转实际类型
                    ConvertCollection(ref state, options);
                    state.ReferenceResolver.AddReferenceObject(state.Current.RefId, state.Current.ReturnValue);


                }
                else if (refState == RefState.Created)
                {
                    state.Current.ObjectState = StackFrameObjectState.ReadElements;
                    state.Current.ReturnValue = refValue;
                }
                else
                {
                    value = default;
                    return false;
                }

                while (true)
                {
                    // 读取属性索引 
                    reader.AheadReadPropertyName();

                    BinaryPropertyInfo binaryPropertyInfo;

                    if (reader.TokenType == BinaryTokenType.EndObject)
                    {
                        break;
                    }

                    Debug.Assert(reader.TokenType == BinaryTokenType.PropertyName);
                    ushort propertySeq = reader.CurrentPropertySeq;
                    BinaryMemberInfo mi = state.GetMemberInfo(propertySeq);
                    Debug.Assert(mi != null);

                    binaryPropertyInfo = BinarySerializer.LookupProperty(
                        state.Current.ReturnValue,
                        mi.NameAsUtf8Bytes,
                        ref state,
                        out bool useExtensionProperty);

                    state.Current.UseExtensionProperty = useExtensionProperty;

                    // binaryPropertyInfo = state.LookupProperty(mi.NameAsString);
                    state.Current.BinaryPropertyInfo = binaryPropertyInfo;
                    state.Current.PropertyPolymorphicConverter = null;
                    if (binaryPropertyInfo == null)
                    {
                        state.Current.EndProperty();
                        continue;
                    }

                    if (!binaryPropertyInfo.ShouldDeserialize)
                    {
                        if (!reader.TrySkip(options))
                        {
                            value = default;
                            return false;
                        }

                        state.Current.EndProperty();
                        continue;
                    }

                    // Obtain the CLR value from the Binary and set the member.
                    if (!state.Current.UseExtensionProperty)
                    {
                        binaryPropertyInfo.ReadBinaryAndSetMember(state.Current.ReturnValue, ref state, ref reader);
                    }
                    else
                    {
                        // TODO 扩展属性
                    }
                    state.Current.EndProperty();

                }

            }
            else
            {
                // Slower path that supports continuation and preserved references.
                if (state.Current.ObjectState == StackFrameObjectState.None)
                {
                    // 刚进入对象读取
                    if (reader.CurrentTypeInfo == null || reader.CurrentTypeInfo.SerializeType != ClassType.Enumerable)
                    {
                        ThrowHelper.ThrowBinaryException_DeserializeUnableToConvertValue(TypeToConvert);
                    }

                    // 读取引用标记
                    if (!reader.ReadStartToken())
                    {
                        value = default;
                        return false;
                    }


                    RefState refState = BinarySerializer.ReadReferenceForObject(this, ref state, ref reader, out object refValue);
                    if (refState == RefState.None)
                    {
                        state.Current.ObjectState = StackFrameObjectState.StartToken;
                    }
                    else if (refState == RefState.Created)
                    {
                        state.Current.ObjectState = StackFrameObjectState.ReadElements;
                        state.Current.ReturnValue = refValue;
                    }
                    else
                    {
                        value = default;
                        return false;
                    }

                }



                // 读取枚举集合索引的字节长度（根据总长度，可能为1、2、4、8字节长度）
                if (state.Current.ObjectState < StackFrameObjectState.ReadEnumerableLengthBytes)
                {
                    // 读取枚举长度
                    if (!reader.ReadBytes(1))
                    {
                        value = default;
                        return false;
                    }
                    state.Current.EnumerableIndexBytes = reader.ValueSpan[0];
                    state.Current.ObjectState = StackFrameObjectState.ReadEnumerableLengthBytes;
                }

                // 读取枚举集合的总长度
                if (state.Current.ObjectState < StackFrameObjectState.ReadEnumerableLength)
                {
                    if (!reader.ReadBytes(state.Current.EnumerableIndexBytes))
                    {
                        value = default;
                        return false;
                    }

                    state.Current.EnumerableLength = reader.GetEnumerableLength(reader.ValueSpan);
                    state.Current.EnumerableIndex = 0;
                    state.Current.ObjectState = StackFrameObjectState.ReadEnumerableLength;
                }

                if (state.Current.ObjectState < StackFrameObjectState.CreatedObject)
                {
                    CreateCollection(ref reader, ref state, options, state.Current.EnumerableLength);
                    state.Current.BinaryPropertyInfo = state.Current.BinaryClassInfo.ElementClassInfo!.PropertyInfoForClassInfo;
                    state.Current.ObjectState = StackFrameObjectState.CreatedObject;
                }

                if (state.Current.ObjectState < StackFrameObjectState.ReadElements)
                {
                    BinaryConverter<TElement> elementConverter = GetElementConverter(elementClassInfo);
                    BinaryConverter converter = elementConverter;

                    // Process all elements.
                    while (state.Current.EnumerableIndex < state.Current.EnumerableLength)
                    {
                        // ReadName --> 读取索引
                        if (state.Current.PropertyState < StackFramePropertyState.ReadName)
                        {
                            if (!reader.ReadBytes(state.Current.EnumerableIndexBytes))
                            {
                                value = default;
                                return false;
                            }
                            state.Current.PropertyState = StackFramePropertyState.ReadName;
                        }

                        // 读取项的类型
                        if (state.Current.PropertyState < StackFramePropertyState.ReadValue)
                        {
                            // 读取项的类型
                            if (!reader.ReadTypeSeq())
                            {
                                value = default;
                                return false;
                            }
                            state.Current.PropertyState = StackFramePropertyState.ReadValue;

                            if (reader.TokenType == BinaryTokenType.Null)
                            {
                                Add(default, ref state);
                                state.Current.EndElement();
                                continue;
                            }

                            if (elementConverter.CanBePolymorphic)
                            {
                                Type t = state.TypeMap.GetType(reader.CurrentTypeInfo.Seq);
                                if (state.Current.PropertyPolymorphicConverter != null && t == state.Current.PropertyPolymorphicConverter.TypeToConvert)
                                {
                                    converter = state.Current.PropertyPolymorphicConverter;
                                }
                                else if (t != null && t != elementConverter.TypeToConvert && elementConverter.TypeToConvert.IsAssignableFrom(t) )
                                {
                                    converter = options.GetConverter(t);
                                    state.Current.PropertyPolymorphicConverter = converter;
                                    state.Current.PolymorphicBinaryClassInfo = options.GetOrAddClass(t);
                                }
                                else
                                {
                                    converter = elementConverter;
                                    state.Current.PropertyPolymorphicConverter = null;
                                    state.Current.PolymorphicBinaryClassInfo = null;
                                }
                            }
                        }
                        else if (state.Current.PropertyPolymorphicConverter != null)
                        {
                            converter = state.Current.PropertyPolymorphicConverter;
                        }


                        if (state.Current.PropertyState < StackFramePropertyState.TryRead)
                        {
                            TElement element;
                            if (converter is BinaryConverter<TElement> typedConverter)
                            {
                                if (!typedConverter.TryRead(ref reader, typeof(TElement), options, ref state, out _, out element))
                                {
                                    value = default;
                                    return false;
                                }
                            }
                            else
                            {
                                if (!converter.TryReadAsObject(ref reader, options, ref state, out object ntElement))
                                {
                                    value = default;
                                    return false;
                                }
                                element = (TElement)ntElement;
                            }

                            Add(element!, ref state);

                            // No need to set PropertyState to TryRead since we're done with this element now.
                            state.Current.EndElement();
                        }
                    }

                    state.Current.ObjectState = StackFrameObjectState.ReadElements;
                    state.Current.EndProperty();

                    // 转实际类型
                    ConvertCollection(ref state, options);
                    state.ReferenceResolver.AddReferenceObject(state.Current.RefId, state.Current.ReturnValue);
                }

                if (state.Current.ObjectState < StackFrameObjectState.ReadProperties)
                {
                    while (true)
                    {
                        // 读取属性索引 
                        if (state.Current.PropertyState == StackFramePropertyState.None)
                        {
                            if (!reader.ReadPropertyName())
                            {
                                //state.Current.ReturnValue = element;
                                value = default;
                                return false;
                            }

                            state.Current.PropertyState = StackFramePropertyState.ReadName;

                            // 
                        }




                        BinaryPropertyInfo binaryPropertyInfo;
                        if (state.Current.PropertyState <= StackFramePropertyState.ReadName)
                        {
                            state.Current.PropertyState = StackFramePropertyState.Name;

                            if (reader.TokenType == BinaryTokenType.EndObject)
                            {
                                break;
                            }

                            Debug.Assert(reader.TokenType == BinaryTokenType.PropertyName);
                            ushort propertySeq = reader.CurrentPropertySeq;
                            BinaryMemberInfo mi = state.GetMemberInfo(propertySeq);
                            Debug.Assert(mi != null);

                            binaryPropertyInfo = BinarySerializer.LookupProperty(
                                state.Current.ReturnValue,
                                mi.NameAsUtf8Bytes,
                                ref state,
                                out bool useExtensionProperty);

                            state.Current.UseExtensionProperty = useExtensionProperty;

                            // binaryPropertyInfo = state.LookupProperty(mi.NameAsString);
                            state.Current.BinaryPropertyInfo = binaryPropertyInfo;
                            state.Current.PropertyPolymorphicConverter = null;
                            if (binaryPropertyInfo == null)
                            {
                                state.Current.EndProperty();
                                continue;
                            }
                        }
                        else
                        {
                            Debug.Assert(state.Current.BinaryPropertyInfo != null);
                            binaryPropertyInfo = state.Current.BinaryPropertyInfo!;
                        }

                        if (state.Current.PropertyState < StackFramePropertyState.ReadValue)
                        {
                            if (!binaryPropertyInfo.ShouldDeserialize)
                            {
                                if (!reader.TrySkip(options))
                                {
                                    value = default;
                                    return false;
                                }

                                state.Current.EndProperty();
                                continue;
                            }
                        }

                        if (state.Current.PropertyState < StackFramePropertyState.TryRead)
                        {
                            // Obtain the CLR value from the Binary and set the member.
                            if (!state.Current.UseExtensionProperty)
                            {
                                if (!binaryPropertyInfo.ReadBinaryAndSetMember(state.Current.ReturnValue, ref state, ref reader))
                                {
                                    value = default;
                                    return false;
                                }
                            }
                            else
                            {
                                // TODO 扩展属性
                            }

                            state.Current.EndProperty();
                        }


                    }

                    state.Current.ObjectState = StackFrameObjectState.ReadProperties;
                }
            }

            
            value = (TCollection)state.Current.ReturnValue!;
            return true;
        }


        protected abstract long GetLength(
            TCollection value,
            BinarySerializerOptions options,
            ref WriteStack state);

        internal sealed override bool OnTryWrite(
            BinaryWriter writer,
            TCollection value,
            BinarySerializerOptions options,
            ref WriteStack state)
        {
            bool success;

            if (value == null)
            {
            }
            else if (!state.SupportContinuation)
            {
                writer.WriteStartArray();

                if (BinarySerializer.WriteReferenceForObject(this, value, ref state, writer))
                {
                    writer.WriteEndArray();
                    return true;
                }

                state.Current.DeclaredBinaryPropertyInfo = state.Current.BinaryClassInfo.ElementClassInfo!.PropertyInfoForClassInfo;


                long len = GetLength(value, options, ref state);
                state.Current.EnumerableIndexBytes = writer.WriteEnumerableLength(len);


                OnWriteResume(writer, value, options, ref state);


                state.Current.EnumeratorIndex = 0;
                state.Current.EndProperty();

                var binaryClassInfo = state.Current.BinaryClassInfo;

                BinaryPropertyInfo dataExtensionProperty = binaryClassInfo.DataExtensionProperty;

                int propertyCount = 0;
                BinaryPropertyInfo[] propertyCacheArray = binaryClassInfo.PropertyCacheArray;
                if (propertyCacheArray != null)
                {
                    propertyCount = propertyCacheArray.Length;
                }

                for (int i = 0; i < propertyCount; i++)
                {
                    BinaryPropertyInfo binaryPropertyInfo = propertyCacheArray![i];
                    state.Current.DeclaredBinaryPropertyInfo = binaryPropertyInfo;

                    if (binaryPropertyInfo.ShouldSerialize)
                    {
                        if (binaryPropertyInfo == dataExtensionProperty)
                        {
                            // TODO: 扩展属性
                        }
                        else
                        {
                            if (!binaryPropertyInfo.GetMemberAndWriteBinary(value!, ref state, writer))
                            {
                                return false;
                            }
                        }
                    }

                    state.Current.EndProperty();
                }


                writer.WriteEndArray();
            }
            else
            {
                if (state.Current.ObjectState < StackFrameWriteObjectState.WriteStartToken)
                {
                    state.Current.ObjectState = StackFrameWriteObjectState.WriteStartToken;
                    state.Current.ProcessedStartToken = true;

                    if (BinarySerializer.WriteReferenceForObject(this, value, ref state, writer))
                    {
                        writer.WriteEndArray();
                        return true;
                    }


                    state.Current.DeclaredBinaryPropertyInfo = state.Current.BinaryClassInfo.ElementClassInfo!.PropertyInfoForClassInfo;

                    writer.WriteStartArray();
                    long len = GetLength(value, options, ref state);
                    state.Current.EnumerableIndexBytes = writer.WriteEnumerableLength(len);
                }


                if (state.Current.ObjectState < StackFrameWriteObjectState.WriteElements)
                {
                    success = OnWriteResume(writer, value, options, ref state);
                    if (!success)
                    {
                        return false;
                    }
                    state.Current.ObjectState = StackFrameWriteObjectState.WriteElements;
                    state.Current.EnumeratorIndex = 0;
                    state.Current.EndProperty();
                }



                if (state.Current.ObjectState < StackFrameWriteObjectState.WriteProperties)
                {
                    var binaryClassInfo = state.Current.BinaryClassInfo;
                    Debug.Assert(binaryClassInfo != null);

                    BinaryPropertyInfo dataExtensionProperty = binaryClassInfo.DataExtensionProperty;

                    int propertyCount = 0;
                    BinaryPropertyInfo[] propertyCacheArray = binaryClassInfo.PropertyCacheArray;
                    if (propertyCacheArray != null)
                    {
                        propertyCount = propertyCacheArray.Length;
                    }

                    while (propertyCount > state.Current.EnumeratorIndex)
                    {
                        BinaryPropertyInfo binaryPropertyInfo = propertyCacheArray![state.Current.EnumeratorIndex];
                        state.Current.DeclaredBinaryPropertyInfo = binaryPropertyInfo;

                        if (binaryPropertyInfo.ShouldSerialize)
                        {
                            if (binaryPropertyInfo == dataExtensionProperty)
                            {
                                // TODO: 扩展属性
                            }
                            else
                            {
                                if (!binaryPropertyInfo.GetMemberAndWriteBinary(value!, ref state, writer))
                                {
                                    Debug.Assert(binaryPropertyInfo.ConverterBase.ClassType != ClassType.Value);
                                    return false;
                                }
                            }
                        }

                        state.Current.EndProperty();
                        state.Current.EnumeratorIndex++;

                        if (ShouldFlush(writer, ref state))
                        {
                            return false;
                        }
                    }

                    state.Current.ObjectState = StackFrameWriteObjectState.WriteProperties;
                }

                if (state.Current.ObjectState < StackFrameWriteObjectState.WriteEndToken)
                {
                    writer.WriteEndArray();
                    state.Current.ProcessedEndToken = true;
                    state.Current.ObjectState = StackFrameWriteObjectState.WriteEndToken;
                }

            }

            return true;
        }


        protected abstract bool OnWriteResume(BinaryWriter writer, TCollection value, BinarySerializerOptions options, ref WriteStack state);

        internal sealed override void CreateInstanceForReferenceResolver(ref BinaryReader reader, ref ReadStack state, BinarySerializerOptions options)
            => CreateCollection(ref reader, ref state, options, 16);
    }
}
