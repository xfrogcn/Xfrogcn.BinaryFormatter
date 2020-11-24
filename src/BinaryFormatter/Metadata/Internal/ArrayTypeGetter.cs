using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Metadata.Internal
{
    public class ArrayTypeGetter : IMetadataGetter
    {
        public bool CanProcess(Type type)
        {
            return type.IsArray;
        }

        public bool GetTypeInfo(Type type, BinaryTypeInfo typeInfo, MetadataGetterContext context)
        {
            var eleType = type.GetElementType();
            typeInfo.Type = TypeEnum.Array;
            typeInfo.IsGeneric = true;
            typeInfo.GenericArgumentCount = 1;
            typeInfo.GenericArguments = new ushort[] { context.GetTypeSeq(eleType, context) }; 
            typeInfo.SerializeType = SerializeTypeEnum.List;
            typeInfo.Members = new BinaryMemberInfo[]{
                new BinaryMemberInfo(){ IsField =false, Seq = 0, Name = nameof(Array.Length), TypeSeq = context.GetTypeSeq(typeof(long), context)}
            };
            
            return true;
        }
    }
}
