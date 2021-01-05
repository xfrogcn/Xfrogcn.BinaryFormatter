using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Numerics;
using System.Reflection;
using static System.TimeZoneInfo;

namespace Xfrogcn.BinaryFormatter
{
    public class DefaultTypeResolver : TypeResolver
    {
        private readonly Dictionary<AssemblyName, Assembly> _assemblyCache
            = new Dictionary<AssemblyName, Assembly>();

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
                { TypeEnum.ByteArray, typeof(byte[])  },
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

                { TypeEnum.KeyValuePair, typeof(KeyValuePair<,>) },
                { TypeEnum.Dictionary, typeof(Dictionary<,>) },
                { TypeEnum.NameValueCollection, typeof(NameValueCollection) },

                { TypeEnum.Nullable, typeof(Nullable<>)  },
                { TypeEnum.List, typeof(List<>) },
                { TypeEnum.Object, typeof(object)  },
            };

        readonly Dictionary<int, Type> _tupleTypeMaps =
            new Dictionary<int, Type>()
            {
                { 1, typeof(Tuple<>) },
                { 2, typeof(Tuple<,>) },
                { 3, typeof(Tuple<,,>) },
                { 4, typeof(Tuple<,,,>) },
                { 5, typeof(Tuple<,,,,>) },
                { 6, typeof(Tuple<,,,,,>) },
                { 7, typeof(Tuple<,,,,,,>) },
                { 8, typeof(Tuple<,,,,,,,>) },
            };

        readonly IDictionary<int, Type> _valueTupleTypeMaps =
            new Dictionary<int, Type>()
            {
                { 1, typeof(ValueTuple<>) },
                { 2, typeof(ValueTuple<,>) },
                { 3, typeof(ValueTuple<,,>) },
                { 4, typeof(ValueTuple<,,,>) },
                { 5, typeof(ValueTuple<,,,,>) },
                { 6, typeof(ValueTuple<,,,,,>) },
                { 7, typeof(ValueTuple<,,,,,,>) },
                { 8, typeof(ValueTuple<,,,,,,,>) },
            };

        readonly ConcurrentDictionary<string, Type> _typeNameMaps =
            new ConcurrentDictionary<string, Type>();

        public override bool TryResolveType(TypeMap typeMap, BinaryTypeInfo typeInfo, out Type type)
        {
            type = null;
            if( typeInfo.Type == TypeEnum.None)
            {
                return true;
            }
            if(string.IsNullOrEmpty(typeInfo.FullName) && _internalTypeMaps.ContainsKey(typeInfo.Type))
            {
                type = _internalTypeMaps[typeInfo.Type];
            }

            if (type == null || type.IsGenericType)
            {
                string typeName = typeInfo.GetFullName(typeMap);
                Type tmp = type;
                type = _typeNameMaps.GetOrAdd(typeName, (tn) =>
                {
                    if (typeInfo.Type == TypeEnum.Array)
                    {
                        TryResolveType(typeMap, typeMap.GetTypeInfo(typeInfo.GenericArguments[0]), out Type elementType);
                        if (elementType != null)
                        {
                            return elementType.MakeArrayType();
                        }
                        return null;
                    }
                    else if (typeInfo.Type == TypeEnum.Tuple ||
                   typeInfo.Type == TypeEnum.ValueTuple)
                    {
                        tmp = GetTupleType(typeInfo);
                    }

                    // 从fullName获取Type
                    Type t = tmp ?? ParseType(typeInfo.FullName);
                    if (t != null)
                    {
                        t = CreateGenericType(typeMap, t, typeInfo);
                    }
                    return t;
                });
            }
            

            if (type != null)
            {
                return true;
            }


            return false;
        }

        private Type GetTupleType(BinaryTypeInfo ti)
        {
            if (!ti.IsGeneric)
            {
                return null;
            }
            if (ti.Type == TypeEnum.Tuple)
            {
                return _tupleTypeMaps[ti.GenericArgumentCount];
            }
            else if (ti.Type == TypeEnum.ValueTuple)
            {
                return _valueTupleTypeMaps[ti.GenericArgumentCount];
            }
            return null;
        }

        public virtual Type ParseType(string fullName)
        {
            var type = Type.GetType(fullName, AssemblyResolver, TypeResolver, false);
            return type;
        }

        protected virtual Assembly AssemblyResolver(AssemblyName assemblyName)
        {
            if (_assemblyCache.ContainsKey(assemblyName))
            {
                return _assemblyCache[assemblyName];
            }

            Assembly actualAssembly = null;
            var assembly = Assembly.GetEntryAssembly(); 
            if(assembly.GetName() == assemblyName)
            {
                actualAssembly = assembly;
            }
            
            if( actualAssembly == null)
            {
                assembly = Assembly.GetCallingAssembly();
                if (assembly.GetName() == assemblyName)
                {
                    actualAssembly = assembly;
                }

            }
            if (actualAssembly == null)
            {
                assembly = Assembly.GetExecutingAssembly();
                if (assembly.GetName() == assemblyName)
                {
                    actualAssembly = assembly;
                }
            }

            try
            {
                actualAssembly = Assembly.Load(assemblyName);
            }
            catch (System.IO.FileNotFoundException)
            {
                Assembly[] ais = AppDomain.CurrentDomain.GetAssemblies();
                actualAssembly = ais.FirstOrDefault(a => a.GetName() == assemblyName);
                if(actualAssembly == null)
                {
                    actualAssembly = ais.FirstOrDefault(a => a.GetName().Name == assemblyName.Name);
                }
                if (actualAssembly == null)
                {
                    ThrowHelper.ThrowBinaryException_DeserializeCannotFindType(assemblyName.FullName);
                }
            }

            _assemblyCache[assemblyName] = actualAssembly;

            return actualAssembly;
        }

        protected virtual Type TypeResolver(Assembly assembly, string typeName, bool ignoreCase)
        {
            Type t =  assembly.GetType(typeName,false,true);
            if(t == null)
            {
                t = assembly.GetType(typeName, false, false);
            }

            if (t == null)
            {
                t = assembly.GetTypes()
                    .FirstOrDefault(t => t.FullName == typeName);
            }
            return t;
        } 

        private Type CreateGenericType(TypeMap typeMap, Type type, BinaryTypeInfo typeInfo)
        {
            if (!typeInfo.IsGeneric || typeInfo.Type == TypeEnum.Array)
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

        public override string TryGetTypeFullName(Type type)
        {
            if (type.IsGenericType)
            {
                return type.GetGenericTypeDefinition().AssemblyQualifiedName;
            }
            return type.AssemblyQualifiedName;
        }
    }
}
