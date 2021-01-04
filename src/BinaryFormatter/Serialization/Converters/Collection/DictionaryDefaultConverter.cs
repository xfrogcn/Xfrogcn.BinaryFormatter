using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal abstract class DictionaryDefaultConverter<TCollection, TKey, TValue>
        : BinaryDictionaryConverter<TCollection>
        where TKey : notnull
    {
        /// <summary>
        /// When overridden, adds the value to the collection.
        /// </summary>
        protected abstract void Add(TKey key, in TValue value, BinarySerializerOptions options, ref ReadStack state);

        /// <summary>
        /// When overridden, converts the temporary collection held in state.Current.ReturnValue to the final collection.
        /// This is used with immutable collections.
        /// </summary>
        protected virtual void ConvertCollection(ref ReadStack state, BinarySerializerOptions options) { }

        /// <summary>
        /// When overridden, create the collection. It may be a temporary collection or the final collection.
        /// </summary>
        protected virtual void CreateCollection(ref BinaryReader reader, ref ReadStack state) { }

        private static readonly Type s_valueType = typeof(TValue);

        internal override Type ElementType => s_valueType;

        protected Type KeyType = typeof(TKey);
        // For string keys we don't use a key converter
        // in order to avoid performance regression on already supported types.
        protected bool IsStringKey = typeof(TKey) == typeof(string);

        protected BinaryConverter<TKey> _keyConverter;
        protected BinaryConverter<TValue> _valueConverter;

        protected static BinaryConverter<TValue> GetValueConverter(BinaryClassInfo elementClassInfo)
        {
            BinaryConverter<TValue> converter = (BinaryConverter<TValue>)elementClassInfo.PropertyInfoForClassInfo.ConverterBase;
            Debug.Assert(converter != null); // It should not be possible to have a null converter at this point.

            return converter;
        }

        protected static BinaryConverter<TKey> GetKeyConverter(Type keyType, BinarySerializerOptions options)
        {
            return (BinaryConverter<TKey>)options.GetConverter(keyType);
        }

        internal sealed override bool OnTryRead(
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
                if (reader.CurrentTypeInfo == null || reader.CurrentTypeInfo.SerializeType != ClassType.Dictionary)
                {
                    ThrowHelper.ThrowBinaryException_DeserializeUnableToConvertValue(TypeToConvert);
                }

                // 读取引用标记
                reader.ReadStartToken();

                RefState refState = BinarySerializer.ReadReferenceForObject(this, ref state, ref reader, out object refValue);
                if (refState == RefState.None)
                {
                    state.Current.ObjectState = StackFrameObjectState.StartToken;
                    CreateCollection(ref reader, ref state);
                  
                    while (true)
                    {
                        reader.AheadReadDictionaryKeySeq();

                        if (reader.TokenType == BinaryTokenType.EndDictionaryKey)
                        {
                            break;
                        }


                        BinaryConverter converter = _keyConverter;
                        reader.AheadReadTypeSeq();

                        if (reader.TokenType == BinaryTokenType.Null)
                        {
                            state.Current.DictionaryKey = default(TKey);
                        }
                        else
                        {
                            _keyConverter ??= GetKeyConverter(typeof(TKey), options);
                            if (_keyConverter.CanBePolymorphic)
                            {
                                Type t = state.TypeMap.GetType(reader.CurrentTypeInfo.Seq);
                                if (state.Current.PropertyPolymorphicConverter != null && t == state.Current.PropertyPolymorphicConverter.TypeToConvert)
                                {
                                    converter = state.Current.PropertyPolymorphicConverter;
                                }
                                else if (t != null && t != _keyConverter.TypeToConvert && _keyConverter.TypeToConvert.IsAssignableFrom(t))
                                {
                                    converter = options.GetConverter(t);
                                    state.Current.PropertyPolymorphicConverter = converter;
                                    state.Current.PolymorphicBinaryClassInfo = options.GetOrAddClass(t);
                                }
                                else
                                {
                                    converter = _keyConverter;
                                    state.Current.PropertyPolymorphicConverter = null;
                                    state.Current.PolymorphicBinaryClassInfo = null;
                                }
                            }

                        }



                        TKey key;
                        if (converter is BinaryConverter<TKey> typedConverter)
                        {
                            typedConverter.TryRead(ref reader, typeof(TKey), options, ref state, out _, out key);

                        }
                        else
                        {
                            converter.TryReadAsObject(ref reader, options, ref state, out object ntElement);
                            key = (TKey)ntElement;
                        }

                        state.Current.DictionaryKey = key;
                        state.Current.PropertyPolymorphicConverter = null;


                        converter = _valueConverter;
                        reader.AheadReadTypeSeq();

                        if (reader.TokenType == BinaryTokenType.Null)
                        {
                            Add((TKey)state.Current.DictionaryKey, default, options, ref state);
                        }
                        else
                        {
                            _valueConverter ??= GetValueConverter(elementClassInfo);
                            if (_valueConverter.CanBePolymorphic)
                            {
                                Type t = state.TypeMap.GetType(reader.CurrentTypeInfo.Seq);
                                if (state.Current.PropertyPolymorphicConverter != null && t == state.Current.PropertyPolymorphicConverter.TypeToConvert)
                                {
                                    converter = state.Current.PropertyPolymorphicConverter;
                                }
                                else if (t != null && t != _valueConverter.TypeToConvert && _valueConverter.TypeToConvert.IsAssignableFrom(t))
                                {
                                    converter = options.GetConverter(t);
                                    state.Current.PropertyPolymorphicConverter = converter;
                                    state.Current.PolymorphicBinaryClassInfo = options.GetOrAddClass(t);
                                }
                                else
                                {
                                    converter = _valueConverter;
                                    state.Current.PropertyPolymorphicConverter = null;
                                    state.Current.PolymorphicBinaryClassInfo = null;
                                }
                            }

                        }



                        TValue element;
                        if (converter is BinaryConverter<TValue> valueTypedConverter)
                        {
                            valueTypedConverter.TryRead(ref reader, typeof(TValue), options, ref state, out _, out element);
                        }
                        else
                        {
                            converter.TryReadAsObject(ref reader, options, ref state, out object ntElement);
                            element = (TValue)ntElement;
                        }

                        state.Current.PropertyPolymorphicConverter = null;

                        Add(key, element, options, ref state);

                        state.Current.EndElement();
                    }


                    state.Current.EndProperty();

                    // 转实际类型
                    ConvertCollection(ref state, options);
                    state.ReferenceResolver.AddReferenceObject(state.Current.RefId, state.Current.ReturnValue);
                }
                else if (refState == RefState.Created)
                {
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
                        state.Current.EndProperty();
                    }
                }

                state.Current.ObjectState = StackFrameObjectState.ReadProperties;

            }
            else
            {
                // Slower path that supports continuation and preserved references.

                if (state.Current.ObjectState == StackFrameObjectState.None)
                {
                    // 刚进入对象读取
                    if (reader.CurrentTypeInfo == null || reader.CurrentTypeInfo.SerializeType != ClassType.Dictionary)
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


                // Create the dictionary.
                if (state.Current.ObjectState < StackFrameObjectState.CreatedObject)
                {
                    CreateCollection(ref reader, ref state);
                    state.Current.ObjectState = StackFrameObjectState.CreatedObject;
                }



                // Process all elements.
                if (state.Current.ObjectState < StackFrameObjectState.ReadElements)
                {
                    while (true)
                    {
                        if (state.Current.PropertyState < StackFramePropertyState.ReadKeySeq)
                        {

                            // Read the key name.
                            if (!reader.ReadDictionaryKeySeq())
                            {
                                value = default;
                                return false;
                            }

                            if (reader.TokenType == BinaryTokenType.EndDictionaryKey)
                            {
                                break;
                            }

                            state.Current.PropertyState = StackFramePropertyState.ReadKeySeq;
                        }

                        BinaryConverter converter = _keyConverter;
                        // Determine the property.
                        if (state.Current.PropertyState < StackFramePropertyState.ReadKeyTypeSeq)
                        {
                            if (!reader.ReadTypeSeq())
                            {
                                value = default;
                                return false;
                            }

                            if (reader.TokenType == BinaryTokenType.Null)
                            {
                                state.Current.DictionaryKey = default(TKey);
                                state.Current.PropertyState = StackFramePropertyState.ReadKey;
                            }
                            else
                            {
                                state.Current.PropertyState = StackFramePropertyState.ReadKeyTypeSeq;
                                _keyConverter ??= GetKeyConverter(typeof(TKey), options);
                                if (_keyConverter.CanBePolymorphic)
                                {
                                    Type t = state.TypeMap.GetType(reader.CurrentTypeInfo.Seq);
                                    if (state.Current.PropertyPolymorphicConverter != null && t == state.Current.PropertyPolymorphicConverter.TypeToConvert)
                                    {
                                        converter = state.Current.PropertyPolymorphicConverter;
                                    }
                                    else if (t != null && t != _keyConverter.TypeToConvert && _keyConverter.TypeToConvert.IsAssignableFrom(t))
                                    {
                                        converter = options.GetConverter(t);
                                        state.Current.PropertyPolymorphicConverter = converter;
                                        state.Current.PolymorphicBinaryClassInfo = options.GetOrAddClass(t);
                                    }
                                    else
                                    {
                                        converter = _keyConverter;
                                        state.Current.PropertyPolymorphicConverter = null;
                                        state.Current.PolymorphicBinaryClassInfo = null;
                                    }
                                }

                            }

                        }
                        else if (state.Current.PropertyPolymorphicConverter != null)
                        {
                            converter = state.Current.PropertyPolymorphicConverter;
                        }

                        if (state.Current.PropertyState < StackFramePropertyState.ReadKey)
                        {
                            TKey key;
                            if (converter is BinaryConverter<TKey> typedConverter)
                            {
                                if (!typedConverter.TryRead(ref reader, typeof(TKey), options, ref state, out _, out key))
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
                                key = (TKey)ntElement;
                            }

                            state.Current.DictionaryKey = key;
                            state.Current.PropertyState = StackFramePropertyState.ReadKey;
                            state.Current.PropertyPolymorphicConverter = null;
                        }

                        converter = _valueConverter;
                        if (state.Current.PropertyState < StackFramePropertyState.ReadValueTypeSeq)
                        {
                            if (!reader.ReadTypeSeq())
                            {
                                value = default;
                                return false;
                            }

                            if (reader.TokenType == BinaryTokenType.Null)
                            {
                                Add((TKey)state.Current.DictionaryKey, default, options, ref state);
                                state.Current.PropertyState = StackFramePropertyState.ReadValue;
                            }
                            else
                            {
                                state.Current.PropertyState = StackFramePropertyState.ReadValueTypeSeq;
                                _valueConverter ??= GetValueConverter(elementClassInfo);
                                if (_valueConverter.CanBePolymorphic)
                                {
                                    Type t = state.TypeMap.GetType(reader.CurrentTypeInfo.Seq);
                                    if (state.Current.PropertyPolymorphicConverter != null && t == state.Current.PropertyPolymorphicConverter.TypeToConvert)
                                    {
                                        converter = state.Current.PropertyPolymorphicConverter;
                                    }
                                    else if (t != null && t != _valueConverter.TypeToConvert && _valueConverter.TypeToConvert.IsAssignableFrom(t))
                                    {
                                        converter = options.GetConverter(t);
                                        state.Current.PropertyPolymorphicConverter = converter;
                                        state.Current.PolymorphicBinaryClassInfo = options.GetOrAddClass(t);
                                    }
                                    else
                                    {
                                        converter = _valueConverter;
                                        state.Current.PropertyPolymorphicConverter = null;
                                        state.Current.PolymorphicBinaryClassInfo = null;
                                    }
                                }

                            }
                        }
                        else if (state.Current.PropertyPolymorphicConverter != null)
                        {
                            converter = state.Current.PropertyPolymorphicConverter;
                        }

                        if (state.Current.PropertyState < StackFramePropertyState.ReadValue)
                        {
                            TValue element;
                            if (converter is BinaryConverter<TValue> typedConverter)
                            {
                                if (!typedConverter.TryRead(ref reader, typeof(TKey), options, ref state, out _, out element))
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
                                element = (TValue)ntElement;
                            }

                            state.Current.PropertyState = StackFramePropertyState.ReadValue;
                            state.Current.PropertyPolymorphicConverter = null;

                            TKey key = (TKey)state.Current.DictionaryKey!;
                            Add(key, element, options, ref state);

                        }

                        if (state.Current.PropertyState < StackFramePropertyState.ReadValueIsEnd)
                        {
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

        internal sealed override bool OnTryWrite(
            BinaryWriter writer,
            TCollection dictionary,
            BinarySerializerOptions options,
            ref WriteStack state)
        {
            if (dictionary == null)
            {
                //writer.WriteNullValue();
                return true;
            }

            if (!state.SupportContinuation)
            {
                writer.WriteStartDictionary();

                if (BinarySerializer.WriteReferenceForObject(this, dictionary, ref state, writer))
                {
                    writer.WriteEndDictionary();
                    return true;
                }

                state.Current.DeclaredBinaryPropertyInfo = state.Current.BinaryClassInfo.ElementClassInfo!.PropertyInfoForClassInfo;

                OnWriteResume(writer, dictionary, options, ref state);


                state.Current.EnumeratorIndex = 0;
                state.Current.EndProperty();

                writer.WriteKeyEnd();

                var binaryClassInfo = state.Current.BinaryClassInfo;
                Debug.Assert(binaryClassInfo != null);

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
                            if (!binaryPropertyInfo.GetMemberAndWriteBinary(dictionary!, ref state, writer))
                            {
                                return false;
                            }
                        }
                    }

                    state.Current.EndProperty();
                }

                writer.WriteEndDictionary();

            }
            else
            {
                if (state.Current.ObjectState < StackFrameWriteObjectState.WriteStartToken)
                {
                    state.Current.ObjectState = StackFrameWriteObjectState.WriteStartToken;
                    state.Current.ProcessedStartToken = true;

                    writer.WriteStartDictionary();

                    if (BinarySerializer.WriteReferenceForObject(this, dictionary, ref state, writer))
                    {
                        writer.WriteEndDictionary();
                        return true;
                    }


                    state.Current.DeclaredBinaryPropertyInfo = state.Current.BinaryClassInfo.ElementClassInfo!.PropertyInfoForClassInfo;

                }

                if (state.Current.ObjectState < StackFrameWriteObjectState.WriteElements)
                {
                    bool success = OnWriteResume(writer, dictionary, options, ref state);
                    if (!success)
                    {
                        return false;
                    }
                    state.Current.ObjectState = StackFrameWriteObjectState.WriteElements;
                    state.Current.EnumeratorIndex = 0;
                    state.Current.EndProperty();
                }

                if (state.Current.ObjectState < StackFrameWriteObjectState.WriteElementsEnd)
                {
                    state.Current.ObjectState = StackFrameWriteObjectState.WriteElementsEnd;
                    writer.WriteKeyEnd();
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
                                if (!binaryPropertyInfo.GetMemberAndWriteBinary(dictionary!, ref state, writer))
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
                    writer.WriteEndDictionary();
                    state.Current.ProcessedEndToken = true;
                    state.Current.ObjectState = StackFrameWriteObjectState.WriteEndToken;
                }

            }



            return true;
        }

        internal virtual bool WriteKey(BinaryWriter writer, TKey key, BinarySerializerOptions options, ref WriteStack state)
        {
            _keyConverter ??= GetKeyConverter(typeof(TKey), options);
            if (!state.SupportContinuation)
            {
                writer.WritePropertyStringSeq();
                _keyConverter.TryWriteAsObject(writer, key, options, ref state);
            }
            else
            {
                if (state.Current.PropertyState < StackFramePropertyState.WriteKeySeq)
                {
                    writer.WritePropertyStringSeq();
                    state.Current.PropertyState = StackFramePropertyState.WriteKeySeq;
                }
                if (state.Current.PropertyState < StackFramePropertyState.WriteKey)
                {
                    bool success = _keyConverter.TryWriteAsObject(writer, key, options, ref state);
                    if (success)
                    {
                        state.Current.PropertyState = StackFramePropertyState.WriteKey;
                    }

                    return success;
                }
            }
            

            return true;
        }

        internal virtual bool WriteValue(BinaryWriter writer, TValue value, BinarySerializerOptions options, ref WriteStack state)
        {
            _valueConverter ??= GetValueConverter(state.Current.BinaryClassInfo.ElementClassInfo);
            if (!state.SupportContinuation)
            {
                _valueConverter.TryWriteAsObject(writer, value, options, ref state);
            }
            else
            {
                if (state.Current.PropertyState < StackFramePropertyState.WriteValue)
                {
                    bool success = _valueConverter.TryWriteAsObject(writer, value, options, ref state);
                    if (success)
                    {
                        state.Current.PropertyState = StackFramePropertyState.WriteValue;
                    }
                    return success;
                }
            }
            
            return true;
        }

        internal sealed override void CreateInstanceForReferenceResolver(ref BinaryReader reader, ref ReadStack state, BinarySerializerOptions options)
            => CreateCollection(ref reader, ref state);

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            typeInfo.Type = TypeEnum.Class;
            typeInfo.SerializeType = ClassType.Dictionary;
            typeInfo.FullName = options.GetTypeFullName(typeof(TCollection));
        }
    }
}
