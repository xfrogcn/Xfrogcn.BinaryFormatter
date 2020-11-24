using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Metadata.Internal
{
    public class DictionaryTypeGetter : IMetadataGetter
    {
        public bool CanProcess(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
        }

        public bool GetTypeInfo(Type type, BinaryTypeInfo typeInfo, MetadataGetterContext context)
        {
            typeInfo.Type = TypeEnum.Dictionary;
            typeInfo.IsGeneric = true;
            typeInfo.GenericArguments = type.GetGenericTypeSeqs(context);
            typeInfo.GenericArgumentCount = (sbyte)typeInfo.GenericArguments.Length;
            typeInfo.SerializeType = SerializeTypeEnum.KeyValuePair;
            typeInfo.Members = new BinaryMemberInfo[]{
                new BinaryMemberInfo(){ IsField =false, Seq = 0, Name = nameof(Dictionary<string,string>.Count), TypeSeq = context.GetTypeSeq(typeof(int), context)}
            };

            return true;
        }
    }
}
