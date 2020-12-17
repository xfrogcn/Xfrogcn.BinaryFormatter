using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal class ObjectDefaultConverter<T> : BinaryObjectConverter<T> where T : notnull
    {
        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            typeInfo.SerializeType = ClassType.Object;
            if (typeof(T) == typeof(object))
            {
                typeInfo.Type = TypeEnum.Object;
            }
            else
            {
                typeInfo.FullName = options.GetTypeFullName(typeof(T));
                typeInfo.Type = TypeEnum.Class;
            }

        }


        internal override bool OnTryRead(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options, ref ReadStack state, [MaybeNullWhen(false)] out T value)
        {
            object obj;

            //if (state.UseFastPath)
            //{
            //    // Fast path that avoids maintaining state variables and dealing with preserved references.

            //    if (reader.TokenType != BinaryTokenType.StartObject)
            //    {
            //        ThrowHelper.ThrowBinaryException_DeserializeUnableToConvertValue(TypeToConvert);
            //    }

            //    if (state.Current.BinaryClassInfo.CreateObject == null)
            //    {
            //        ThrowHelper.ThrowNotSupportedException_DeserializeNoConstructor(state.Current.BinaryClassInfo.Type, ref reader, ref state);
            //    }

            //    obj = state.Current.BinaryClassInfo.CreateObject!()!;

            //    // Process all properties.
            //    while (true)
            //    {
            //        // Read the property name or EndObject.
            //        // reader.ReadWithVerify();

            //        BinaryTokenType tokenType = reader.TokenType;

            //        if (tokenType == BinaryTokenType.EndObject)
            //        {
            //            break;
            //        }

            //        // Read method would have thrown if otherwise.
            //        Debug.Assert(tokenType == BinaryTokenType.PropertyName);

            //        ReadOnlySpan<byte> unescapedPropertyName = BinarySerializer.GetPropertyName(ref state, ref reader, options);
            //        BinaryPropertyInfo jsonPropertyInfo = BinarySerializer.LookupProperty(
            //            obj,
            //            unescapedPropertyName,
            //            ref state,
            //            out bool useExtensionProperty);

            //        ReadPropertyValue(obj, ref state, ref reader, jsonPropertyInfo, useExtensionProperty);
            //    }
            //}
            //else
            {
                // Slower path that supports continuation and preserved references.
                if (state.Current.ObjectState == StackFrameObjectState.None)
                {
                    // 刚进入对象读取
                    if (reader.CurrentTypeInfo == null || reader.CurrentTypeInfo.SerializeType != ClassType.Object)
                    {
                        ThrowHelper.ThrowBinaryException_DeserializeUnableToConvertValue(TypeToConvert);
                    }

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




                //// Handle the metadata properties.
                //if (state.Current.ObjectState < StackFrameObjectState.PropertyValue)
                //{
                //    if (options.ReferenceHandler != null)
                //    {
                //        if (JsonSerializer.ResolveMetadataForJsonObject<T>(ref reader, ref state, options))
                //        {
                //            if (state.Current.ObjectState == StackFrameObjectState.ReadRefEndObject)
                //            {
                //                // This will never throw since it was previously validated in ResolveMetadataForJsonObject.
                //                value = (T)state.Current.ReturnValue!;
                //                return true;
                //            }
                //        }
                //        else
                //        {
                //            value = default;
                //            return false;
                //        }
                //    }
                //}

                // 创建对象
                if (state.Current.ObjectState < StackFrameObjectState.CreatedObject)
                {
                    if (state.Current.BinaryClassInfo.CreateObject == null)
                    {
                       // ThrowHelper.ThrowNotSupportedException_DeserializeNoConstructor(state.Current.BinaryClassInfo.Type, ref reader, ref state);
                    }
                    obj = state.Current.BinaryClassInfo.CreateObject();
                    //if( state.TypeMap.GetType( state.Current.BinaryTypeInfo.Seq) == state.Current.BinaryPropertyInfo.ConverterBase.TypeToConvert)
                    //{
                    //    obj = state.Current.BinaryPropertyInfo.RuntimeClassInfo.CreateObject();
                    //}
                    //else if(state.Current.PolymorphicBinaryClassInfo!= null )
                    //{
                    //    obj = state.Current.PolymorphicBinaryClassInfo.CreateObject();
                    //}
                    //else
                    //{
                    //    obj = null;
                    //    ThrowHelper.ThrowBinaryException();
                    //}



                    state.Current.ReturnValue = obj;
                    state.Current.ObjectState = StackFrameObjectState.CreatedObject;
                }
                else
                {
                    obj = state.Current.ReturnValue!;
                    Debug.Assert(obj != null);
                }

                // Process all properties.
                while (true)
                {
                    // Determine the property.

                    // 读取属性索引 
                    if (state.Current.PropertyState == StackFramePropertyState.None)
                    {
                        if (!reader.ReadPropertyName())
                        {
                            state.Current.ReturnValue = obj;
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
                            obj,
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
                                state.Current.ReturnValue = obj;
                                value = default;
                                return false;
                            }

                            state.Current.EndProperty();
                            continue;
                        }

                        //if (!ReadAheadPropertyValue(ref state, ref reader, binaryPropertyInfo))
                        //{
                        //    state.Current.ReturnValue = obj;
                        //    value = default;
                        //    return false;
                        //}
                    }

                    if (state.Current.PropertyState < StackFramePropertyState.TryRead)
                    {
                        // Obtain the CLR value from the Binary and set the member.
                        if (!state.Current.UseExtensionProperty)
                        {
                            if (!binaryPropertyInfo.ReadBinaryAndSetMember(obj, ref state, ref reader))
                            {
                                state.Current.ReturnValue = obj;
                                value = default;
                                return false;
                            }
                        }
                        else
                        {
                            //if (!binaryPropertyInfo.ReadBinaryAndAddExtensionProperty(obj, ref state, ref reader))
                            //{
                            //    // No need to set 'value' here since JsonElement must be read in full.
                            //    state.Current.ReturnValue = obj;
                            //    value = default;
                            //    return false;
                            //}
                        }

                        state.Current.EndProperty();
                    }

                    //if (state.Current.PropertyState < StackFramePropertyState.Name)
                    //{
                    //    state.Current.PropertyState = StackFramePropertyState.Name;

                    //    BinaryTokenType tokenType = reader.TokenType;
                    //    if (tokenType == BinaryTokenType.EndObject)
                    //    {
                    //        break;
                    //    }

                    //    // Read method would have thrown if otherwise.
                    //    Debug.Assert(tokenType == BinaryTokenType.PropertyName);

                    //    ReadOnlySpan<byte> unescapedPropertyName = BinarySerializer.GetPropertyName(ref state, ref reader, options);
                    //    jsonPropertyInfo = BinarySerializer.LookupProperty(
                    //        obj,
                    //        unescapedPropertyName,
                    //        ref state,
                    //        out bool useExtensionProperty);

                    //    state.Current.UseExtensionProperty = useExtensionProperty;
                    //}
                    //else
                    //{
                    //    Debug.Assert(state.Current.BinaryPropertyInfo != null);
                    //    jsonPropertyInfo = state.Current.BinaryPropertyInfo!;
                    //}

                    //if (state.Current.PropertyState < StackFramePropertyState.ReadValue)
                    //{
                    //    if (!jsonPropertyInfo.ShouldDeserialize)
                    //    {
                    //        if (!reader.TrySkip())
                    //        {
                    //            state.Current.ReturnValue = obj;
                    //            value = default;
                    //            return false;
                    //        }

                    //        state.Current.EndProperty();
                    //        continue;
                    //    }

                    //    if (!ReadAheadPropertyValue(ref state, ref reader, jsonPropertyInfo))
                    //    {
                    //        state.Current.ReturnValue = obj;
                    //        value = default;
                    //        return false;
                    //    }
                    //}

                    //if (state.Current.PropertyState < StackFramePropertyState.TryRead)
                    //{
                    //    // Obtain the CLR value from the JSON and set the member.
                    //    if (!state.Current.UseExtensionProperty)
                    //    {
                    //        if (!jsonPropertyInfo.ReadBinaryAndSetMember(obj, ref state, ref reader))
                    //        {
                    //            state.Current.ReturnValue = obj;
                    //            value = default;
                    //            return false;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        if (!jsonPropertyInfo.ReadBinaryAndAddExtensionProperty(obj, ref state, ref reader))
                    //        {
                    //            // No need to set 'value' here since JsonElement must be read in full.
                    //            state.Current.ReturnValue = obj;
                    //            value = default;
                    //            return false;
                    //        }
                    //    }

                    //    state.Current.EndProperty();
                    //}
                }
            }

            // Check if we are trying to build the sorted cache.
            if (state.Current.PropertyRefCache != null)
            {
                state.Current.BinaryClassInfo.UpdateSortedPropertyCache(ref state.Current);
            }

            value = (T)obj;

            return true;
        }

        internal override bool OnTryWrite(BinaryWriter writer, T value, BinarySerializerOptions options, ref WriteStack state)
        {
            // Minimize boxing for structs by only boxing once here
            object objectValue = value!;

            if (!state.SupportContinuation)
            {

            }
            else
            {
                if(!state.Current.ProcessedStartToken)
                {
                    state.Current.ProcessedStartToken = true;
                    writer.WriteStartObject();
                    if (BinarySerializer.WriteReferenceForObject(this, objectValue, ref state, writer))
                    {
                        writer.WriteEndObject();
                        return true;
                    }
                }

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
                            //if (!binaryPropertyInfo.GetMemberAndWriteJsonExtensionData(objectValue!, ref state, writer))
                            //{
                            //    return false;
                            //}
                        }
                        else
                        {
                           
                            if (!binaryPropertyInfo.GetMemberAndWriteBinary(objectValue!, ref state, writer))
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

                if (!state.Current.ProcessedEndToken)
                {
                    state.Current.ProcessedEndToken = true;
                    writer.WriteEndObject();
                }
            }

            return true;
            // return base.OnTryWrite(writer, value, options, ref state);
        }
    }
}
