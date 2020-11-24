using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using Xfrogcn.BinaryFormatter;

namespace System
{
    public static class TypeExtensions
    {
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

        public static sbyte GetGenericArgumentCount([NotNull]this Type type)
        {
            if(type ==null)
            {
                return 0;
            }
            if (type.IsGenericType)
            {
                return (sbyte)type.GetGenericArguments().Length;
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

        public static ushort[] GetGenericTypeSeqs([NotNull]this Type type, [NotNull] MetadataGetterContext context)
        {
            if (!type.IsGenericType)
            {
                return new ushort[0];
            }

            return type.GetGenericArguments()
                .Select(t => context.GetTypeSeq(t, context))
                .ToArray();
        }

        public static BinaryMemberInfo[] GetMemberInfos([NotNull]this Type type, [NotNull] MetadataGetterContext context)
        {
            // 公共可读属性 公共字段
            ushort seq = 0;
            List<BinaryMemberInfo> members = new List<BinaryMemberInfo>();

            var pis = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var p in pis)
            {
                members.Add(new BinaryMemberInfo()
                {
                    Seq = seq,
                    IsField = false,
                    TypeSeq = context.GetTypeSeq(p.PropertyType, context),
                    Name = p.Name
                });
                seq++;
            }
            var fis = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var f in fis)
            {
                members.Add(new BinaryMemberInfo()
                {
                    Seq = seq,
                    IsField = false,
                    TypeSeq = context.GetTypeSeq(f.FieldType, context),
                    Name = f.Name
                });
                seq++;
            }
            return members.ToArray();
        }
    }
}
