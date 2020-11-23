using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Metadata.Internal
{
    public class BaseTypeGetter : IMetadataGetter
    {
        static readonly Dictionary<Type, TypeEnum> _baseTypeMap = new Dictionary<Type, TypeEnum>()
        {
            { typeof(Boolean),  TypeEnum.Boolean },
            { typeof(Char), TypeEnum.Char },
            { typeof(String), TypeEnum.String },
            { typeof(Uri), TypeEnum.Uri },
            { typeof(Version), TypeEnum.Version },
            { typeof(DBNull), TypeEnum.DBNull },
        };

        public bool CanProcess(Type type)
        {
            return _baseTypeMap.ContainsKey(type);
        }

        public bool GetTypeInfo(Type type, BinaryTypeInfo typeInfo, MetadataGetterContext context)
        {
            TypeEnum te = _baseTypeMap[type];
            typeInfo.Type = te;
            typeInfo.IsGeneric = false;
            typeInfo.GenericArgumentCount = 0;
            typeInfo.SerializeType = SerializeTypeEnum.SingleValue;
            typeInfo.Members = new BinaryMemberInfo[0];
            return true;
        }
    }
}
