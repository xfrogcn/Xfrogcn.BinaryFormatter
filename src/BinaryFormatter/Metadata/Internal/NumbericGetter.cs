using System;
using System.Collections.Generic;
using System.Linq;
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
            typeInfo.SerializeType = SerializeTypeEnum.SingleValue;

            if (GetComplexTypeInfo(rt, typeInfo, context) ||
                 GetVectorTypeInfo(rt, typeInfo, context) ||
                 GetVectorTTypeInfo(type, typeInfo, context) ||
                 GetMatrixTypeInfo(rt, typeInfo, context) || 
                 GetPlaneTypeInfo(rt, typeInfo, context) || 
                 GetQuaternionTypeInfo(rt, typeInfo, context))
            {
                return true;
            }
            
            return true;
        }

        protected virtual bool GetComplexTypeInfo(Type type, BinaryTypeInfo typeInfo, MetadataGetterContext context)
        {
            if(type == typeof(Complex))
            {
                typeInfo.Members = new BinaryMemberInfo[2]
                {
                    new BinaryMemberInfo(){ IsField = false, Name = nameof(Complex.Real), Seq = 0, TypeSeq = context.GetTypeSeq(typeof(double), context)  },
                    new BinaryMemberInfo(){ IsField = false, Name = nameof(Complex.Imaginary), Seq = 1, TypeSeq = context.GetTypeSeq(typeof(double), context)  }
                };
                typeInfo.SerializeType = SerializeTypeEnum.KeyValuePair;
                return true;
            }
            return false;
        }

        protected virtual bool GetVectorTypeInfo(Type type, BinaryTypeInfo typeInfo, MetadataGetterContext context)
        {
            List<BinaryMemberInfo> mlist = new List<BinaryMemberInfo>();
            if(type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector4))
            {
                mlist.Add(new BinaryMemberInfo()
                {
                    IsField =true,
                    Name = nameof(Vector2.X),
                    Seq = 0,
                    TypeSeq = context.GetTypeSeq(typeof(float), context)
                });
                mlist.Add(new BinaryMemberInfo()
                {
                    IsField = true,
                    Name = nameof(Vector2.Y),
                    Seq = 1,
                    TypeSeq = context.GetTypeSeq(typeof(float), context)
                });
                Vector3 v2 = new Vector3();
                v2.X = 0;
                v2.Y = 0;
                
            }
            if(type == typeof(Vector3) || type == typeof(Vector4))
            {
                mlist.Add(new BinaryMemberInfo()
                {
                    IsField = true,
                    Name = nameof(Vector3.Z),
                    Seq = 2,
                    TypeSeq = context.GetTypeSeq(typeof(float), context)
                });
            }
            if (type == typeof(Vector4))
            {
                mlist.Add(new BinaryMemberInfo()
                {
                    IsField = true,
                    Name = nameof(Vector4.W),
                    Seq = 3,
                    TypeSeq = context.GetTypeSeq(typeof(float), context)
                });
            }

            if (mlist.Count > 0)
            {
                typeInfo.Members = mlist.ToArray();
                typeInfo.SerializeType = SerializeTypeEnum.KeyValuePair;
                return true;
            }
            return false;
        }


        protected virtual bool GetVectorTTypeInfo(Type type, BinaryTypeInfo typeInfo, MetadataGetterContext context)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Vector<>))
            {
                typeInfo.IsGeneric = true;
                typeInfo.GenericArgumentCount = type.GetGenericArgumentCount();
                typeInfo.GenericArguments = type.GetGenericArguments()
                    .Select(t => context.GetTypeSeq(t, context)).ToArray();
                typeInfo.SerializeType = SerializeTypeEnum.List;
                return true;
            }
          
            return false;
        }

        protected virtual bool GetMatrixTypeInfo(Type type, BinaryTypeInfo typeInfo, MetadataGetterContext context)
        {
            List<BinaryMemberInfo> mlist = new List<BinaryMemberInfo>();
            if(type == typeof(Matrix3x2) || type == typeof(Matrix4x4))
            {
                mlist.Add(new BinaryMemberInfo()
                {
                    Seq = 0,
                    IsField =true,
                    Name = nameof(Matrix3x2.M11),
                    TypeSeq = context.GetTypeSeq(typeof(float), context)
                });
                mlist.Add(new BinaryMemberInfo()
                {
                    Seq = 1,
                    IsField = true,
                    Name = nameof(Matrix3x2.M12),
                    TypeSeq = context.GetTypeSeq(typeof(float), context)
                });
                mlist.Add(new BinaryMemberInfo()
                {
                    Seq = 2,
                    IsField = true,
                    Name = nameof(Matrix3x2.M21),
                    TypeSeq = context.GetTypeSeq(typeof(float), context)
                });
                mlist.Add(new BinaryMemberInfo()
                {
                    Seq = 3,
                    IsField = true,
                    Name = nameof(Matrix3x2.M22),
                    TypeSeq = context.GetTypeSeq(typeof(float), context)
                });
                mlist.Add(new BinaryMemberInfo()
                {
                    Seq = 4,
                    IsField = true,
                    Name = nameof(Matrix3x2.M31),
                    TypeSeq = context.GetTypeSeq(typeof(float), context)
                });
                mlist.Add(new BinaryMemberInfo()
                {
                    Seq = 5,
                    IsField = true,
                    Name = nameof(Matrix3x2.M32),
                    TypeSeq = context.GetTypeSeq(typeof(float), context)
                });
            }
            if(type == typeof(Matrix4x4))
            {
                Matrix4x4 m = new Matrix4x4();
                m.M14 = 0;

                mlist.Add(new BinaryMemberInfo()
                {
                    Seq = 6,
                    IsField = true,
                    Name = nameof(Matrix4x4.M13),
                    TypeSeq = context.GetTypeSeq(typeof(float), context)
                });
                mlist.Add(new BinaryMemberInfo()
                {
                    Seq = 7,
                    IsField = true,
                    Name = nameof(Matrix4x4.M14),
                    TypeSeq = context.GetTypeSeq(typeof(float), context)
                });
                mlist.Add(new BinaryMemberInfo()
                {
                    Seq = 8,
                    IsField = true,
                    Name = nameof(Matrix4x4.M23),
                    TypeSeq = context.GetTypeSeq(typeof(float), context)
                });
                mlist.Add(new BinaryMemberInfo()
                {
                    Seq = 9,
                    IsField = true,
                    Name = nameof(Matrix4x4.M24),
                    TypeSeq = context.GetTypeSeq(typeof(float), context)
                });
                mlist.Add(new BinaryMemberInfo()
                {
                    Seq = 10,
                    IsField = true,
                    Name = nameof(Matrix4x4.M33),
                    TypeSeq = context.GetTypeSeq(typeof(float), context)
                });
                mlist.Add(new BinaryMemberInfo()
                {
                    Seq = 11,
                    IsField = true,
                    Name = nameof(Matrix4x4.M34),
                    TypeSeq = context.GetTypeSeq(typeof(float), context)
                });

                mlist.Add(new BinaryMemberInfo()
                {
                    Seq = 12,
                    IsField = true,
                    Name = nameof(Matrix4x4.M41),
                    TypeSeq = context.GetTypeSeq(typeof(float), context)
                });
                mlist.Add(new BinaryMemberInfo()
                {
                    Seq = 13,
                    IsField = true,
                    Name = nameof(Matrix4x4.M42),
                    TypeSeq = context.GetTypeSeq(typeof(float), context)
                });
                mlist.Add(new BinaryMemberInfo()
                {
                    Seq = 14,
                    IsField = true,
                    Name = nameof(Matrix4x4.M43),
                    TypeSeq = context.GetTypeSeq(typeof(float), context)
                });
                mlist.Add(new BinaryMemberInfo()
                {
                    Seq = 15,
                    IsField = true,
                    Name = nameof(Matrix4x4.M44),
                    TypeSeq = context.GetTypeSeq(typeof(float), context)
                });
            }
            if (mlist.Count > 0)
            {
                typeInfo.SerializeType = SerializeTypeEnum.KeyValuePair;
                typeInfo.Members = mlist.ToArray();
                return true;
            }
            return false;
        }

        protected virtual bool GetPlaneTypeInfo(Type type, BinaryTypeInfo typeInfo, MetadataGetterContext context)
        {
            if(type == typeof(Plane))
            {
                typeInfo.SerializeType = SerializeTypeEnum.KeyValuePair;
                typeInfo.Members = new BinaryMemberInfo[2]
                {
                    new BinaryMemberInfo(){ IsField = true, Name = nameof(Plane.D), Seq = 0, TypeSeq = context.GetTypeSeq(typeof(float), context)},
                    new BinaryMemberInfo(){ IsField = true, Name = nameof(Plane.Normal), Seq = 1, TypeSeq = context.GetTypeSeq(typeof(Vector3), context)},
                };
                return true;
            }
            return false;
        }

        protected virtual bool GetQuaternionTypeInfo(Type type, BinaryTypeInfo typeInfo, MetadataGetterContext context)
        {
            if (type == typeof(Quaternion))
            {
                typeInfo.SerializeType = SerializeTypeEnum.KeyValuePair;
                typeInfo.Members = new BinaryMemberInfo[4]
                {
                    new BinaryMemberInfo(){ IsField = true, Name = nameof(Quaternion.W), Seq = 0, TypeSeq = context.GetTypeSeq(typeof(float), context)},
                    new BinaryMemberInfo(){ IsField = true, Name = nameof(Quaternion.X), Seq = 1, TypeSeq = context.GetTypeSeq(typeof(float), context)},
                    new BinaryMemberInfo(){ IsField = true, Name = nameof(Quaternion.Y), Seq = 2, TypeSeq = context.GetTypeSeq(typeof(float), context)},
                    new BinaryMemberInfo(){ IsField = true, Name = nameof(Quaternion.Z), Seq = 3, TypeSeq = context.GetTypeSeq(typeof(float), context)},
                };
                return true;
            }
            return false;
        }
    }
}
