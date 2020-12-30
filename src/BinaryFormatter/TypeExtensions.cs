using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Xfrogcn.BinaryFormatter;
using Xfrogcn.BinaryFormatter.Serialization;

namespace System
{
    public static class TypeExtensions
    {
        private static readonly Type s_nullableType = typeof(Nullable<>);

        public static bool IsNullableValueType(this Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        public static bool IsNullableType(this Type type)
        {
            return !type.IsValueType || IsNullableValueType(type);
        }

        public static Type GetSerializeType([NotNull] this Type type)
        {
            if(type == null)
            {
                return null;
            }

            Type rt = type;
            if (type.IsGenericType)
            {
                rt = type.GetGenericTypeDefinition();
            }
            return rt;
        }

        public static byte GetGenericArgumentCount([NotNull]this Type type)
        {
            if(type ==null)
            {
                return 0;
            }
            if (type.IsGenericType)
            {
                return (byte)type.GetGenericArguments().Length;
            }
            return 0;
        }

        public static Type[] GetTypeGenericArguments([NotNull] this Type type)
        {
            if (type == null)
            {
                return new Type[0];
            }
            if (type.IsGenericType)
            {
                return type.GetGenericArguments();
            }
            return new Type[0];
        }

        public static ushort[] GetGenericTypeSeqs([NotNull]this Type type, [NotNull] TypeMap typeMap)
        {
            if (!type.IsGenericType)
            {
                return new ushort[0];
            }

            return type.GetGenericArguments()
                .Select(t =>
                {
                    typeMap.TryAdd(t, out BinaryTypeInfo ti);
                    return ti.Seq;
                })
                .ToArray();
        }

        //public static BinaryMemberInfo[] GetMemberInfos([NotNull]this Type type, [NotNull] MetadataGetterContext context)
        //{
        //    // 公共可读属性 公共字段
        //    ushort seq = 0;
        //    List<BinaryMemberInfo> members = new List<BinaryMemberInfo>();

        //    var pis = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        //    foreach (var p in pis)
        //    {
        //        members.Add(new BinaryMemberInfo()
        //        {
        //            Seq = seq,
        //            IsField = false,
        //            TypeSeq = context.GetTypeSeq(p.PropertyType, context),
        //            Name = p.Name
        //        });
                
        //        seq++;
        //    }
        //    var fis = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        //    foreach (var f in fis)
        //    {
        //        members.Add(new BinaryMemberInfo()
        //        {
        //            Seq = seq,
        //            IsField = false,
        //            TypeSeq = context.GetTypeSeq(f.FieldType, context),
        //            Name = f.Name
        //        });
        //        seq++;
        //    }
        //    return members.ToArray();
        //}

        /// <summary>
        /// Returns <see langword="true" /> when the given type is of type <see cref="Nullable{T}"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullableOfT(this Type type) =>
            type.IsGenericType && type.GetGenericTypeDefinition() == s_nullableType;

        public static bool IsAssignableFromInternal(this Type type, Type from)
        {
            if (IsNullableValueType(from) && type.IsInterface)
            {
                return type.IsAssignableFrom(from.GetGenericArguments()[0]);
            }

            return type.IsAssignableFrom(from);
        }

        public static bool IsRefId(this object instance)
        {
            if(instance == null)
            {
                return false;
            }

            return instance is ReferenceID;
        }
    }
}
