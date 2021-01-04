using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class NameValueCollectionConverter : BinaryDictionaryConverter<NameValueCollection>
    {
        private BinaryConverter<string> _keyConverter;
        private BinaryConverter<string> _valueConverter;

        private void Add(string key,
                         in string value,
#pragma warning disable IDE0060 // 删除未使用的参数
                         BinarySerializerOptions options,
#pragma warning restore IDE0060 // 删除未使用的参数
                         ref ReadStack state)
        {
            NameValueCollection collection = (NameValueCollection)state.Current.ReturnValue!;
            collection[key] = value;
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            typeInfo.Type = TypeEnum.NameValueCollection;
            typeInfo.FullName = null;
            typeInfo.SerializeType = ClassType.Dictionary;
        }

        internal override bool OnTryRead(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options, ref ReadStack state, out NameValueCollection value)
        {
          
            _keyConverter ??= (options.GetConverter(typeof(string)) as BinaryConverter<string>);
            _valueConverter ??= (options.GetConverter(typeof(string)) as BinaryConverter<string>);

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
                    state.Current.ReturnValue = new NameValueCollection();

                    while (true)
                    {
                        reader.AheadReadDictionaryKeySeq();

                        if (reader.TokenType == BinaryTokenType.EndDictionaryKey)
                        {
                            break;
                        }

                        reader.AheadReadTypeSeq();

                        if (reader.TokenType == BinaryTokenType.Null)
                        {
                            state.Current.DictionaryKey = default(string);
                        }
                        else
                        {
                            _keyConverter.TryRead(ref reader, typeof(string), options, ref state, out _, out string key);
                            state.Current.DictionaryKey = key;
                        }

                        reader.AheadReadTypeSeq();

                        if (reader.TokenType == BinaryTokenType.Null)
                        {
                            Add((string)state.Current.DictionaryKey, default, options, ref state);
                        }
                        else
                        {
                            _valueConverter.TryRead(ref reader, typeof(string), options, ref state, out _, out string element);
                            Add((string)state.Current.DictionaryKey, element, options, ref state);
                        }

                        state.Current.EndElement();
                    }

                    state.Current.EndProperty();

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

                reader.AheadReadPropertyName();
                Debug.Assert(reader.TokenType == BinaryTokenType.EndObject);
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
                }


                // Create the dictionary.
                if (state.Current.ObjectState < StackFrameObjectState.CreatedObject)
                {
                    state.Current.ReturnValue = new NameValueCollection();
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
                                state.Current.DictionaryKey = default(string);
                                state.Current.PropertyState = StackFramePropertyState.ReadKey;
                            }
                            else
                            {
                                state.Current.PropertyState = StackFramePropertyState.ReadKeyTypeSeq;
                            }

                        }


                        if (state.Current.PropertyState < StackFramePropertyState.ReadKey)
                        {
                            if (!_keyConverter.TryRead(ref reader, typeof(string), options, ref state, out _, out string key))
                            {
                                value = default;
                                return false;
                            }

                            state.Current.DictionaryKey = key;
                            state.Current.PropertyState = StackFramePropertyState.ReadKey;
                            state.Current.PropertyPolymorphicConverter = null;
                        }

                        if (state.Current.PropertyState < StackFramePropertyState.ReadValueTypeSeq)
                        {
                            if (!reader.ReadTypeSeq())
                            {
                                value = default;
                                return false;
                            }

                            if (reader.TokenType == BinaryTokenType.Null)
                            {
                                Add((string)state.Current.DictionaryKey, default, options, ref state);
                                state.Current.PropertyState = StackFramePropertyState.ReadValue;
                            }
                            else
                            {
                                state.Current.PropertyState = StackFramePropertyState.ReadValueTypeSeq;
                            }
                        }


                        if (state.Current.PropertyState < StackFramePropertyState.ReadValue)
                        {
                            if (!_valueConverter.TryRead(ref reader, typeof(string), options, ref state, out _, out string element))
                            {
                                value = default;
                                return false;
                            }

                            state.Current.PropertyState = StackFramePropertyState.ReadValue;
                            state.Current.PropertyPolymorphicConverter = null;

                            string key = (string)state.Current.DictionaryKey!;
                            Add(key, element, options, ref state);
                        }

                        if (state.Current.PropertyState < StackFramePropertyState.ReadValueIsEnd)
                        {
                            state.Current.EndElement();
                        }

                    }

                    state.Current.ObjectState = StackFrameObjectState.ReadElements;
                    state.Current.EndProperty();

                    state.ReferenceResolver.AddReferenceObject(state.Current.RefId, state.Current.ReturnValue);
                }

                if (state.Current.ObjectState < StackFrameObjectState.ReadProperties)
                {
                    if (!reader.ReadPropertyName())
                    {
                        //state.Current.ReturnValue = element;
                        value = default;
                        return false;
                    }
                    Debug.Assert(reader.TokenType == BinaryTokenType.EndObject);
                    state.Current.ObjectState = StackFrameObjectState.ReadProperties;
                }

            }

            value = (NameValueCollection)state.Current.ReturnValue!;
            return true;
        }


        internal override bool OnTryWrite(BinaryWriter writer, NameValueCollection value, BinarySerializerOptions options, ref WriteStack state)
        {
            if (value == null)
            {
                //writer.WriteNullValue();
                return true;
            }

            _keyConverter ??= (options.GetConverter(typeof(string)) as BinaryConverter<string>);
            _valueConverter ??= (options.GetConverter(typeof(string)) as BinaryConverter<string>);


            if (!state.SupportContinuation)
            {
                writer.WriteStartDictionary();

                if (BinarySerializer.WriteReferenceForObject(this, value, ref state, writer))
                {
                    writer.WriteEndDictionary();
                    return true;
                }

                OnWriteResume(writer, value, options, ref state);


                state.Current.EnumeratorIndex = 0;
                state.Current.EndProperty();

                writer.WriteKeyEnd();

                writer.WriteEndDictionary();

            }
            else
            {
                if (state.Current.ObjectState < StackFrameWriteObjectState.WriteStartToken)
                {
                    state.Current.ObjectState = StackFrameWriteObjectState.WriteStartToken;
                    state.Current.ProcessedStartToken = true;

                    writer.WriteStartDictionary();

                    if (BinarySerializer.WriteReferenceForObject(this, value, ref state, writer))
                    {
                        writer.WriteEndDictionary();
                        return true;
                    }

                }

                if (state.Current.ObjectState < StackFrameWriteObjectState.WriteElements)
                {
                    bool success = OnWriteResume(writer, value, options, ref state);
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

                if (state.Current.ObjectState < StackFrameWriteObjectState.WriteEndToken)
                {
                    writer.WriteEndDictionary();
                    state.Current.ProcessedEndToken = true;
                    state.Current.ObjectState = StackFrameWriteObjectState.WriteEndToken;
                }

            }


            return true;
        }

        protected internal override bool OnWriteResume(BinaryWriter writer, NameValueCollection value, BinarySerializerOptions options, ref WriteStack state)
        {
            IEnumerator enumerator;
            if (state.Current.CollectionEnumerator == null)
            {
                enumerator = value.AllKeys.GetEnumerator();
                if (!enumerator.MoveNext())
                {
                    return true;
                }
            }
            else
            {
                enumerator = state.Current.CollectionEnumerator;
            }

            _keyConverter ??= (options.GetConverter(typeof(string)) as BinaryConverter<string>);
            _valueConverter ??= (options.GetConverter(typeof(string)) as BinaryConverter<string>);

            if (!state.SupportContinuation)
            {
                do
                {
                    writer.WritePropertyStringSeq();
                    string key = (string)enumerator.Current;
                    _keyConverter.TryWrite(writer, key, options, ref state);
                    _valueConverter.TryWrite(writer, value[key], options, ref state);
                    state.Current.EndDictionaryElement();
                } while (enumerator.MoveNext());
            }
            else
            {
                do
                {
                    if (ShouldFlush(writer, ref state))
                    {
                        state.Current.CollectionEnumerator = enumerator;
                        return false;
                    }

                    writer.WritePropertyStringSeq();
                    string key = (string)enumerator.Current;
                    _keyConverter.TryWrite(writer, key, options, ref state);
                    _valueConverter.TryWrite(writer, value[key], options, ref state);

                    state.Current.EndDictionaryElement();
                } while (enumerator.MoveNext());

            }

            return true;
        }

    }
}
