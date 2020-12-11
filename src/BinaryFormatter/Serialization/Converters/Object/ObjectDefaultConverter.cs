using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal class ObjectDefaultConverter<T> : BinaryObjectConverter<T> where T : notnull
    {
        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            typeInfo.FullName = options.GetTypeFullName(typeof(T));
            typeInfo.SerializeType = ClassType.Object;
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
                if(BinarySerializer.WriteReferenceForObject(this, objectValue, ref state, writer))
                {
                    return true;
                }

                BinaryPropertyInfo dataExtensionProperty = state.Current.BinaryClassInfo.DataExtensionProperty;

                int propertyCount = 0;
                BinaryPropertyInfo[] propertyCacheArray = state.Current.BinaryClassInfo.PropertyCacheArray;
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
                 //   state.Current.ProcessedEndToken = true;
                  //  writer.WriteEndObject();
                }
            }

            return base.OnTryWrite(writer, value, options, ref state);
        }
    }
}
