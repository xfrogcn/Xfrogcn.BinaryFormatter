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

            if (state.UseFastPath)
            {

                // 刚进入对象读取
                if (reader.CurrentTypeInfo == null || reader.CurrentTypeInfo.SerializeType != ClassType.Object)
                {
                    ThrowHelper.ThrowBinaryException_DeserializeUnableToConvertValue(TypeToConvert);
                }

                reader.AheadReadStartToken();

                if (state.Current.BinaryClassInfo.CreateObject == null)
                {
                    ThrowHelper.ThrowNotSupportedException_DeserializeNoConstructor(state.Current.BinaryClassInfo.Type, ref reader, ref state);
                }

                RefState refState = BinarySerializer.ReadReferenceForObject(this, ref state, ref reader, out object refValue);
                if (refState == RefState.None)
                {
                    obj = state.Current.BinaryClassInfo.CreateObject();
                    state.ReferenceResolver.AddReferenceObject(state.Current.RefId, obj);
                }
                else
                {
                    obj = refValue;
                }


                // Process all properties.
                while (true)
                {
                    reader.AheadReadPropertyName();

                    BinaryPropertyInfo binaryPropertyInfo = null;

                    state.Current.PropertyState = StackFramePropertyState.Name;

                    if (reader.TokenType == BinaryTokenType.EndObject)
                    {
                        break;
                    }

                    Debug.Assert(reader.TokenType == BinaryTokenType.PropertyName);
                    ushort propertySeq = reader.CurrentPropertySeq;
                    BinaryMemberInfo mi = state.GetMemberInfo(propertySeq);
                    // Debug.Assert(mi != null);

                    
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
                        if (!reader.TrySkip(options))
                        {
                            value = default;
                            return false;
                        }
                        state.Current.EndProperty();
                        continue;
                    }

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


                    // Obtain the CLR value from the Binary and set the member.
                    if (!state.Current.UseExtensionProperty)
                    {
                        binaryPropertyInfo.ReadBinaryAndSetMember(obj, ref state, ref reader);
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
                    if (reader.CurrentTypeInfo == null || reader.CurrentTypeInfo.SerializeType != ClassType.Object)
                    {
                        ThrowHelper.ThrowBinaryException_DeserializeUnableToConvertValue(TypeToConvert);
                    }

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
                        state.Current.ObjectState = StackFrameObjectState.CreatedObject;
                        state.Current.ReturnValue = refValue;
                    }

                }


                // 创建对象
                if (state.Current.ObjectState < StackFrameObjectState.CreatedObject)
                {
                    if (state.Current.BinaryClassInfo.CreateObject == null)
                    {
                        ThrowHelper.ThrowNotSupportedException_DeserializeNoConstructor(state.Current.BinaryClassInfo.Type, ref reader, ref state);
                    }
                    obj = state.Current.BinaryClassInfo.CreateObject();
                    state.ReferenceResolver.AddReferenceObject(state.Current.RefId, obj);

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
                            // TODO 扩展属性
                        }

                        state.Current.EndProperty();
                    }
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

                writer.WriteStartObject();

                if (BinarySerializer.WriteReferenceForObject(this, objectValue, ref state, writer))
                {
                    writer.WriteEndObject();
                    return true;
                }


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

                            if (!binaryPropertyInfo.GetMemberAndWriteBinary(objectValue!, ref state, writer))
                            {
                                return false;
                            }
                        }
                    }

                    state.Current.EndProperty();
                }

                writer.WriteEndObject();
            }
            else
            {
                if (!state.Current.ProcessedStartToken)
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
                            // TODO: 扩展属性
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
