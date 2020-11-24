using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Metadata.Internal
{
    public class ObjectTypeGetter : IMetadataGetter
    {
        public bool CanProcess(Type type)
        {
            return type == typeof(object);
        }

        public bool GetTypeInfo(Type type, BinaryTypeInfo typeInfo, MetadataGetterContext context)
        {
            typeInfo.Type = TypeEnum.Object;
            typeInfo.IsGeneric = false;
            typeInfo.GenericArgumentCount = 0;
            typeInfo.GenericArguments = new ushort[0];
            typeInfo.SerializeType = SerializeTypeEnum.KeyValuePair;
            typeInfo.Members = new BinaryMemberInfo[0];

            return true;
        }
    }
}
