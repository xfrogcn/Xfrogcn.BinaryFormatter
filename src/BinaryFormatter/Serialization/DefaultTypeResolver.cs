using System;
using System.Collections.Generic;
using System.Numerics;
using static System.TimeZoneInfo;

namespace Xfrogcn.BinaryFormatter
{
    internal sealed class DefaultTypeResolver : TypeResolver
    {
        readonly Dictionary<TypeEnum, Type> _internalTypeMaps =
            new Dictionary<TypeEnum, Type>()
            {
                { TypeEnum.Byte, typeof(Byte)  },
                { TypeEnum.Int16, typeof(Int16)  },
                { TypeEnum.Int32, typeof(Int32)  },
                { TypeEnum.Int64, typeof(Int64)  },
                { TypeEnum.SByte, typeof(SByte)  },
                { TypeEnum.UInt16, typeof(UInt16)  },
                { TypeEnum.UInt32, typeof(UInt32)  },
                { TypeEnum.UInt64, typeof(UInt64)  },
                { TypeEnum.Single, typeof(Single)  },
                { TypeEnum.Double, typeof(Double)  },
                { TypeEnum.Decimal, typeof(Decimal)  },
                { TypeEnum.BigInteger, typeof(BigInteger)  },
                { TypeEnum.Complex, typeof(Complex)  },
                { TypeEnum.Vector2, typeof(Vector2)  },
                { TypeEnum.Vector3, typeof(Vector3)  },
                { TypeEnum.Vector4, typeof(Vector4)  },
                { TypeEnum.VectorT, typeof(Vector<>)  },

                { TypeEnum.Matrix3x2, typeof(Matrix3x2)  },
                { TypeEnum.Matrix4x4, typeof(Matrix4x4)  },
                { TypeEnum.Plane, typeof(Plane)  },
                { TypeEnum.Quaternion, typeof(Quaternion)  },
                { TypeEnum.IntPtr, typeof(IntPtr)  },

                { TypeEnum.Guid, typeof(Guid)  },
                { TypeEnum.DateTime, typeof(DateTime)  },
                { TypeEnum.DateTimeOffset, typeof(DateTimeOffset)  },
                { TypeEnum.TimeZoneInfo, typeof(TimeZoneInfo)  },
                { TypeEnum.AdjustmentRule, typeof(AdjustmentRule)  },
                { TypeEnum.TransitionTime, typeof(TransitionTime)  },
                { TypeEnum.TimeSpan, typeof(TimeSpan)  },

                { TypeEnum.Boolean, typeof(bool)  },
                { TypeEnum.Char, typeof(char)  },
                { TypeEnum.String, typeof(string)  },
                { TypeEnum.Uri, typeof(Uri)  },
                { TypeEnum.Version, typeof(Version)  },
                { TypeEnum.DBNull, typeof(DBNull)  },

                { TypeEnum.Nullable, typeof(Nullable<>)  },
                { TypeEnum.Object, typeof(object)  },
            };


        public override bool TryResolveType(TypeMap typeMap, BinaryTypeInfo typeInfo, out Type type)
        {
            type = null;
            if( typeInfo.Type == TypeEnum.None)
            {
                return true;
            }
            if(_internalTypeMaps.ContainsKey(typeInfo.Type))
            {
                type = _internalTypeMaps[typeInfo.Type];
            }

            if (type != null)
            {
                type = CreateGenericType(typeMap, type, typeInfo);
                return true;
            }

            return false;
        }

        private Type CreateGenericType(TypeMap typeMap, Type type, BinaryTypeInfo typeInfo)
        {
            if (!typeInfo.IsGeneric)
            {
                return type;
            }

            Type[] genericArguments = new Type[typeInfo.GenericArgumentCount];
            for (int i = 0; i < typeInfo.GenericArgumentCount; i++)
            {
                var ti = typeMap.GetTypeInfo(typeInfo.GenericArguments[i]);
                if (ti == null)
                {
                    throw new Exception();
                }
                if (TryResolveType(typeMap, ti, out Type t))
                {
                    genericArguments[i] = t;
                }
                else
                {
                    throw new Exception();
                }
            }

            return type.MakeGenericType(genericArguments);

        }
    }
}
