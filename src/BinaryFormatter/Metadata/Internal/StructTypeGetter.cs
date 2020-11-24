using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Metadata.Internal
{
    public class StructTypeGetter : IMetadataGetter
    {
        public bool CanProcess(Type type)
        {
            if (type.IsValueType)
            {
                return true;
            }
            return false;
        }

        public bool GetTypeInfo(Type type, BinaryTypeInfo typeInfo, MetadataGetterContext context)
        {
            typeInfo.Type = TypeEnum.Struct;
            typeInfo.IsGeneric = type.IsGenericType;
            typeInfo.GenericArgumentCount = type.GetGenericArgumentCount();
            typeInfo.SerializeType = SerializeTypeEnum.KeyValuePair;
            typeInfo.GenericArguments = type.GetTypeGenericArguments()
                .Select(t => context.GetTypeSeq(t, context))
                .ToArray();
            typeInfo.Members = type.GetMemberInfos(context);
            return true;
        }
    }
}
