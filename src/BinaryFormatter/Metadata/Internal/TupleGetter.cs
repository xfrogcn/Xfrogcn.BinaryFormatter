using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Metadata.Internal
{
    public class TupleGetter : IMetadataGetter
    {
        static readonly Dictionary<Type, TypeEnum> _tupleTypeMap = new Dictionary<Type, TypeEnum>()
        {
            { typeof(Tuple),  TypeEnum.Tuple },
            { typeof(Tuple<>), TypeEnum.TupleT },
            { typeof(Tuple<,>), TypeEnum.TupleT },
            { typeof(Tuple<,,>), TypeEnum.TupleT },
            { typeof(Tuple<,,,>), TypeEnum.TupleT },
            { typeof(Tuple<,,,,>), TypeEnum.TupleT },
            { typeof(Tuple<,,,,,>), TypeEnum.TupleT },
            { typeof(Tuple<,,,,,,>), TypeEnum.TupleT },
            { typeof(Tuple<,,,,,,,>), TypeEnum.TupleT }
        };

        public bool CanProcess(Type type)
        {
            Type rt = type.GetSerializeType();

            return _tupleTypeMap.ContainsKey(rt);
        }

        public bool GetTypeInfo(Type type, BinaryTypeInfo typeInfo, MetadataGetterContext context)
        {
            Type rt = type.GetSerializeType();
            TypeEnum te = _tupleTypeMap[rt];
            typeInfo.Type = te;
            typeInfo.IsGeneric = type.IsGenericType;
            typeInfo.GenericArgumentCount = type.GetGenericArgumentCount();
            typeInfo.SerializeType = SerializeTypeEnum.KeyValuePair;
            typeInfo.GenericArguments = type.GetTypeGenericArguments()
                .Select(t => context.GetTypeSeq(t, context))
                .ToArray();

            typeInfo.Members = new BinaryMemberInfo[typeInfo.GenericArguments.Length];
            for(ushort i = 0; i < typeInfo.GenericArguments.Length; i++)
            {
                BinaryMemberInfo mi = new BinaryMemberInfo()
                {
                    IsField = false,
                    Seq = i,
                    TypeSeq = typeInfo.GenericArguments[i]
                };
                if( i == 7)
                {
                    mi.Name = "Rest";
                }
                else
                {
                    mi.Name = $"Item{i + 1}";
                }
                typeInfo.Members[i] = mi;
            }

            return true;
        }
    }
}
