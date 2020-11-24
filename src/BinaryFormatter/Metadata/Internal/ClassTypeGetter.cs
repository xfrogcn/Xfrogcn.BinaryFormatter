using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Metadata.Internal
{
    public class ClassTypeGetter : IMetadataGetter
    {
        public bool CanProcess(Type type)
        {
            return type.IsClass;
        }

        public bool GetTypeInfo(Type type, BinaryTypeInfo typeInfo, MetadataGetterContext context)
        {
            typeInfo.IsGeneric = type.IsGenericType;
            typeInfo.GenericArgumentCount = type.GetGenericArgumentCount();
            typeInfo.SerializeType = SerializeTypeEnum.KeyValuePair;
            typeInfo.GenericArguments = type.GetGenericTypeSeqs(context);
            typeInfo.Members = type.GetMemberInfos(context);
            typeInfo.FullName = type.AssemblyQualifiedName;
            return true;
        }
    }
}
