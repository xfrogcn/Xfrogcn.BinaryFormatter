using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Metadata.Internal
{
    public class NumbericGetter : IMetadataGetter
    {
        static readonly Dictionary<Type, TypeEnum> _nmbericTypeMap = new Dictionary<Type, TypeEnum>()
        {
            { typeof(Byte),  TypeEnum.Byte },
            { typeof(Int16), TypeEnum.Int16 },
            { typeof(Int32), TypeEnum.Int32 },
            { typeof(Int64), TypeEnum.Int64 },
            { typeof(SByte),  TypeEnum.SByte },
            { typeof(UInt16), TypeEnum.UInt16 },
            { typeof(UInt32), TypeEnum.UInt32 },
            { typeof(UInt64), TypeEnum.UInt64 },
            { typeof(Single), TypeEnum.Single },
            { typeof(Double), TypeEnum.Double },
            { typeof(Decimal), TypeEnum.Decimal },
            { typeof(BigInteger), TypeEnum.BigInteger },
            { typeof(Complex), TypeEnum.Complex },
            { typeof(Vector2), TypeEnum.Vector2 },
            { typeof(Vector3), TypeEnum.Vector3 },
            { typeof(Vector4), TypeEnum.Vector4 },
            { typeof(Vector<>), TypeEnum.VectorT },
            { typeof(Matrix3x2), TypeEnum.Matrix3x2 },
            { typeof(Matrix4x4), TypeEnum.Matrix4x4 },
            { typeof(Plane), TypeEnum.Plane },
            { typeof(Quaternion), TypeEnum.Quaternion },
        };

        public bool CanProcess(Type type)
        {
            if(type == null)
            {
                return false;
            }
            Type rt = type.GetSerializeType() ;
            return _nmbericTypeMap.ContainsKey(rt);
        }


        public bool GetTypeInfo(Type type, BinaryTypeInfo typeInfo, MetadataGetterContext context)
        {
            Type rt = type.GetSerializeType();
            TypeEnum te = _nmbericTypeMap[rt];
            typeInfo.Type = te;
            typeInfo.IsGeneric = false;
            typeInfo.GenericArgumentCount = 0;
            
            
            
            return true;
        }


    }
}
