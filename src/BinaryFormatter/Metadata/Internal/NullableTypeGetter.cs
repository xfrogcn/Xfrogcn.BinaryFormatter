using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Metadata.Internal
{
    public class NullableTypeGetter : IMetadataGetter
    {
        public bool CanProcess(Type type)
        {
            if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return true;
            }
            return false;
        }

        public bool GetTypeInfo(Type type, BinaryTypeInfo typeInfo, MetadataGetterContext context)
        {
            TypeEnum te = TypeEnum.Nullable;
            typeInfo.Type = te;
            typeInfo.IsGeneric = true;
            typeInfo.SerializeType = SerializeTypeEnum.KeyValuePair;
            typeInfo.GenericArguments = type.GetGenericTypeSeqs(context);
            typeInfo.GenericArgumentCount = (sbyte)typeInfo.GenericArguments.Length;
            typeInfo.Members = new BinaryMemberInfo[0];
            return true;
        }
    }
}
