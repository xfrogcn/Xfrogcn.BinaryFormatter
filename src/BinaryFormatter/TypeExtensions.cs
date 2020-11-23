using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

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
    }
}
