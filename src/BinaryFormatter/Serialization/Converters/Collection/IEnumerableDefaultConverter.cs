﻿using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal abstract class IEnumerableDefaultConverter<TCollection, TElement>
        : BinaryCollectionConverter<TCollection, TElement>
    {
        protected abstract void Add(in TElement value, ref ReadStack state);
        protected abstract void CreateCollection(ref BinaryReader reader, ref ReadStack state, BinarySerializerOptions options);

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
                //// Fast path that avoids maintaining state variables and dealing with preserved references.

                //if (reader.TokenType != JsonTokenType.StartArray)
                //{
                //    ThrowHelper.ThrowJsonException_DeserializeUnableToConvertValue(TypeToConvert);
                //}

                //CreateCollection(ref reader, ref state, options);

                //JsonConverter<TElement> elementConverter = GetElementConverter(elementClassInfo);
                //if (elementConverter.CanUseDirectReadOrWrite && state.Current.NumberHandling == null)
                //{
                //    // Fast path that avoids validation and extra indirection.
                //    while (true)
                //    {
                //        reader.ReadWithVerify();
                //        if (reader.TokenType == JsonTokenType.EndArray)
                //        {
                //            break;
                //        }

                //        // Obtain the CLR value from the JSON and apply to the object.
                //        TElement element = elementConverter.Read(ref reader, elementConverter.TypeToConvert, options);
                //        Add(element!, ref state);
                //    }
                //}
                //else
                //{
                //    // Process all elements.
                //    while (true)
                //    {
                //        reader.ReadWithVerify();
                //        if (reader.TokenType == JsonTokenType.EndArray)
                //        {
                //            break;
                //        }

                //        // Get the value from the converter and add it.
                //        elementConverter.TryRead(ref reader, typeof(TElement), options, ref state, out TElement element);
                //        Add(element!, ref state);
                //    }
                //}
            }
            else
            {
                // Slower path that supports continuation and preserved references.

                bool preserveReferences = options.ReferenceHandler != null;
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
                }

                if (state.Current.ObjectState < StackFrameObjectState.CreatedObject)
                {
                    CreateCollection(ref reader, ref state, options);
                    state.Current.BinaryPropertyInfo = state.Current.BinaryClassInfo.ElementClassInfo!.PropertyInfoForClassInfo;
                    state.Current.ObjectState = StackFrameObjectState.CreatedObject;
                }

                // 读取枚举集合索引的字节长度（根据总长度，可能为1、2、4、8字节长度）
                if(state.Current.ObjectState < StackFrameObjectState.ReadEnumerableLengthBytes)
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
                if(state.Current.ObjectState< StackFrameObjectState.ReadEnumerableLength)
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



                if (state.Current.ObjectState < StackFrameObjectState.ReadElements)
                {
                    BinaryConverter<TElement> elementConverter = GetElementConverter(elementClassInfo);
                    BinaryConverter converter = elementConverter;

                    // Process all elements.
                    while (state.Current.EnumerableIndex < state.Current.EnumerableLength)
                    {
                        // ReadName --> 读取索引
                        if(state.Current.PropertyState < StackFramePropertyState.ReadName)
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

                            if(reader.TokenType == BinaryTokenType.Null)
                            {
                                Add(default, ref state);
                                state.Current.EndElement();
                                continue;
                            }
                            
                            if (elementConverter.CanBePolymorphic)
                            {
                                Type t = state.TypeMap.GetType(reader.CurrentTypeInfo.Seq);
                                if (state.Current.PropertyPolymorphicConverter!=null && t == state.Current.PropertyPolymorphicConverter.TypeToConvert)
                                {
                                    converter = state.Current.PropertyPolymorphicConverter;
                                }
                                else if (t != elementConverter.TypeToConvert)
                                {
                                    converter = options.GetConverter(t);
                                    state.Current.PropertyPolymorphicConverter = converter;
                                    state.Current.PolymorphicBinaryClassInfo = options.GetOrAddClass(t);
                                }
                            }
                        }


                        if (state.Current.PropertyState < StackFramePropertyState.TryRead)
                        {
                            TElement element = default;
                            if(converter is BinaryConverter<TElement> typedConverter)
                            {
                                if (!typedConverter.TryRead(ref reader, typeof(TElement), options, ref state, out element))
                                {
                                    value = default;
                                    return false;
                                }
                            }
                            else
                            {
                                if(!converter.TryReadAsObject(ref reader, options, ref state, out object ntElement))
                                {
                                    value = default;
                                    return false;
                                }
                                element =(TElement)ntElement;
                            }

                            Add(element!, ref state);

                            // No need to set PropertyState to TryRead since we're done with this element now.
                            state.Current.EndElement();
                        }
                    }

                    state.Current.ObjectState = StackFrameObjectState.ReadElements;

                }

                if (state.Current.ObjectState < StackFrameObjectState.EndToken)
                {
                    state.Current.ObjectState = StackFrameObjectState.EndToken;

                    // 读取对象结束标记用于校验
                    if (!reader.ReadEndArrayToken())
                    {
                        value = default;
                        return false;
                    }
                }

                if (state.Current.ObjectState < StackFrameObjectState.EndTokenValidation)
                {
                    state.Current.ObjectState = StackFrameObjectState.EndTokenValidation;
                    if(reader.TokenType != BinaryTokenType.EndObject)
                    {
                        ThrowHelper.ThrowBinaryException();
                    }
                }
            }

            ConvertCollection(ref state, options);
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
                success = true;
            }
            else
            {
                if (!state.Current.ProcessedStartToken)
                {
                    state.Current.ProcessedStartToken = true;

                    if (BinarySerializer.WriteReferenceForObject(this, value, ref state, writer))
                    {
                        writer.WriteEndArray();
                        return true;
                    }

                    //if (options.ReferenceHandler == null)
                    //{
                    //    writer.WriteStartArray();
                    //}
                    //else
                    //{
                    //    MetadataPropertyName metadata = JsonSerializer.WriteReferenceForCollection(this, value, ref state, writer);
                    //    if (metadata == MetadataPropertyName.Ref)
                    //    {
                    //        return true;
                    //    }

                    //    state.Current.MetadataPropertyName = metadata;
                    //}

                    state.Current.DeclaredBinaryPropertyInfo = state.Current.BinaryClassInfo.ElementClassInfo!.PropertyInfoForClassInfo;

                    writer.WriteStartArray();
                    long len = GetLength(value, options, ref state);
                    state.Current.EnumerableIndexBytes = writer.WriteEnumerableLength(len);
                }


                success = OnWriteResume(writer, value, options, ref state);
                if (success)
                {
                    if (!state.Current.ProcessedEndToken)
                    {
                        state.Current.ProcessedEndToken = true;
                        writer.WriteEndArray();
                    }
                }
            }

            return success;
        }


        protected abstract bool OnWriteResume(BinaryWriter writer, TCollection value, BinarySerializerOptions options, ref WriteStack state);

        internal sealed override void CreateInstanceForReferenceResolver(ref BinaryReader reader, ref ReadStack state, BinarySerializerOptions options)
            => CreateCollection(ref reader, ref state, options);
    }
}