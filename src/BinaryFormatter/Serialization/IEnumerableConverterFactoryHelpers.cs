using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Serialization
{
    internal static class IEnumerableConverterFactoryHelpers
    {
        // Immutable collection types.
        private const string ImmutableArrayGenericTypeName = "System.Collections.Immutable.ImmutableArray`1";
        private const string ImmutableListGenericTypeName = "System.Collections.Immutable.ImmutableList`1";
        private const string ImmutableListGenericInterfaceTypeName = "System.Collections.Immutable.IImmutableList`1";
        private const string ImmutableStackGenericTypeName = "System.Collections.Immutable.ImmutableStack`1";
        private const string ImmutableStackGenericInterfaceTypeName = "System.Collections.Immutable.IImmutableStack`1";
        private const string ImmutableQueueGenericTypeName = "System.Collections.Immutable.ImmutableQueue`1";
        private const string ImmutableQueueGenericInterfaceTypeName = "System.Collections.Immutable.IImmutableQueue`1";
        private const string ImmutableSortedSetGenericTypeName = "System.Collections.Immutable.ImmutableSortedSet`1";
        private const string ImmutableHashSetGenericTypeName = "System.Collections.Immutable.ImmutableHashSet`1";
        private const string ImmutableSetGenericInterfaceTypeName = "System.Collections.Immutable.IImmutableSet`1";
        private const string ImmutableDictionaryGenericTypeName = "System.Collections.Immutable.ImmutableDictionary`2";
        private const string ImmutableDictionaryGenericInterfaceTypeName = "System.Collections.Immutable.IImmutableDictionary`2";
        private const string ImmutableSortedDictionaryGenericTypeName = "System.Collections.Immutable.ImmutableSortedDictionary`2";

        // Immutable collection builder types.
        private const string ImmutableArrayTypeName = "System.Collections.Immutable.ImmutableArray";
        private const string ImmutableListTypeName = "System.Collections.Immutable.ImmutableList";
        private const string ImmutableStackTypeName = "System.Collections.Immutable.ImmutableStack";
        private const string ImmutableQueueTypeName = "System.Collections.Immutable.ImmutableQueue";
        private const string ImmutableSortedSetTypeName = "System.Collections.Immutable.ImmutableSortedSet";
        private const string ImmutableHashSetTypeName = "System.Collections.Immutable.ImmutableHashSet";
        private const string ImmutableDictionaryTypeName = "System.Collections.Immutable.ImmutableDictionary";
        private const string ImmutableSortedDictionaryTypeName = "System.Collections.Immutable.ImmutableSortedDictionary";

        private const string CreateRangeMethodName = "CreateRange";
        //private const string CreateRangeMethodNameForEnumerable = "CreateRange`1";
        //private const string CreateRangeMethodNameForDictionary = "CreateRange`2";

        //private const string ImmutableCollectionsAssembly = "System.Collections.Immutable";

        internal static Type GetCompatibleGenericBaseClass(this Type type, Type baseType)
        {
            Debug.Assert(baseType.IsGenericType);
            Debug.Assert(!baseType.IsInterface);
            Debug.Assert(baseType == baseType.GetGenericTypeDefinition());

            Type baseTypeToCheck = type;

            while (baseTypeToCheck != null && baseTypeToCheck != TypeMap.ObjectType)
            {
                if (baseTypeToCheck.IsGenericType)
                {
                    Type genericTypeToCheck = baseTypeToCheck.GetGenericTypeDefinition();
                    if (genericTypeToCheck == baseType)
                    {
                        return baseTypeToCheck;
                    }
                }

                baseTypeToCheck = baseTypeToCheck.BaseType;
            }

            return null;
        }

        /// <summary>
        /// 获取兼容的接口类型
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="interfaceType">接口类型</param>
        /// <returns>如果存在指定接口返回接口类型，否则返回null</returns>
        internal static Type GetCompatibleGenericInterface(this Type type, Type interfaceType)
        {
            Debug.Assert(interfaceType.IsGenericType);
            Debug.Assert(interfaceType.IsInterface);
            Debug.Assert(interfaceType == interfaceType.GetGenericTypeDefinition());

            Type interfaceToCheck = type;

            if (interfaceToCheck.IsGenericType)
            {
                interfaceToCheck = interfaceToCheck.GetGenericTypeDefinition();
            }

            if (interfaceToCheck == interfaceType)
            {
                return type;
            }

            foreach (Type typeToCheck in type.GetInterfaces())
            {
                if (typeToCheck.IsGenericType)
                {
                    Type genericInterfaceToCheck = typeToCheck.GetGenericTypeDefinition();
                    if (genericInterfaceToCheck == interfaceType)
                    {
                        return typeToCheck;
                    }
                }
            }

            return null;
        }

        public static bool IsImmutableDictionaryType(this Type type)
        {
            if (!type.IsGenericType || !type.Assembly.FullName!.StartsWith("System.Collections.Immutable,", StringComparison.Ordinal))
            {
                return false;
            }

            return type.GetGenericTypeDefinition().FullName switch
            {
                ImmutableDictionaryGenericTypeName or ImmutableDictionaryGenericInterfaceTypeName or ImmutableSortedDictionaryGenericTypeName => true,
                _ => false,
            };
        }

        /// <summary>
        /// 检查类型是否为不可变集合类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsImmutableEnumerableType(this Type type)
        {
            if (!type.IsGenericType || !type.Assembly.FullName!.StartsWith("System.Collections.Immutable,", StringComparison.Ordinal))
            {
                return false;
            }

            return type.GetGenericTypeDefinition().FullName switch
            {
                ImmutableArrayGenericTypeName or ImmutableListGenericTypeName or ImmutableListGenericInterfaceTypeName or ImmutableStackGenericTypeName or ImmutableStackGenericInterfaceTypeName or ImmutableQueueGenericTypeName or ImmutableQueueGenericInterfaceTypeName or ImmutableSortedSetGenericTypeName or ImmutableHashSetGenericTypeName or ImmutableSetGenericInterfaceTypeName => true,
                _ => false,
            };
        }

        public static bool IsImmutableStackType(this Type type)
        {
            string fullName = type.GetGenericTypeDefinition().FullName;
            if(fullName == ImmutableStackGenericTypeName || fullName == ImmutableStackGenericInterfaceTypeName)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取不可变集合的CreateRange方法，用于创建对应的不可变集合实例
        /// </summary>
        /// <param name="type">集合类型</param>
        /// <param name="elementType">元素类型</param>
        /// <returns>对应的创建方法</returns>
        public static MethodInfo GetImmutableEnumerableCreateRangeMethod(this Type type, Type elementType)
        {
            // 获取构造类型
            Type constructingType = GetImmutableEnumerableConstructingType(type);
            if (constructingType != null)
            {

                MethodInfo[] constructingTypeMethods = constructingType.GetMethods();
                foreach (MethodInfo method in constructingTypeMethods)
                {
                    // CreateRange<T>(IEnumerable<T>)
                    if (method.Name == CreateRangeMethodName &&
                        method.GetParameters().Length == 1 &&
                        method.IsGenericMethod &&
                        method.GetGenericArguments().Length == 1)
                    {
                        return method.MakeGenericMethod(elementType);
                    }
                }
            }
            throw new NotSupportedException($"类型不支持序列化：{type.FullName}");
        }

           public static MethodInfo GetImmutableDictionaryCreateRangeMethod(this Type type, Type keyType, Type elementType)
        {
            Type constructingType = GetImmutableDictionaryConstructingType(type);
            if (constructingType != null)
            {
                MethodInfo[] constructingTypeMethods = constructingType.GetMethods();
                foreach (MethodInfo method in constructingTypeMethods)
                {
                    if (method.Name == CreateRangeMethodName &&
                        method.GetParameters().Length == 1 &&
                        method.IsGenericMethod &&
                        method.GetGenericArguments().Length == 2)
                    {
                        return method.MakeGenericMethod(keyType, elementType);
                    }
                }
            }

            throw new NotSupportedException($"类型不支持序列化：{type.FullName}");
        }

        /// <summary>
        /// 获取不可变集合对应的构造类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Type GetImmutableEnumerableConstructingType(Type type)
        {
            Debug.Assert(type.IsImmutableEnumerableType());

            // Use the generic type definition of the immutable collection to determine
            // an appropriate constructing type, i.e. a type that we can invoke the
            // `CreateRange<T>` method on, which returns the desired immutable collection.
            Type underlyingType = type.GetGenericTypeDefinition();
            string constructingTypeName;

            switch (underlyingType.FullName)
            {
                case ImmutableArrayGenericTypeName:
                    constructingTypeName = ImmutableArrayTypeName;
                    break;
                case ImmutableListGenericTypeName:
                case ImmutableListGenericInterfaceTypeName:
                    constructingTypeName = ImmutableListTypeName;
                    break;
                case ImmutableStackGenericTypeName:
                case ImmutableStackGenericInterfaceTypeName:
                    constructingTypeName = ImmutableStackTypeName;
                    break;
                case ImmutableQueueGenericTypeName:
                case ImmutableQueueGenericInterfaceTypeName:
                    constructingTypeName = ImmutableQueueTypeName;
                    break;
                case ImmutableSortedSetGenericTypeName:
                    constructingTypeName = ImmutableSortedSetTypeName;
                    break;
                case ImmutableHashSetGenericTypeName:
                case ImmutableSetGenericInterfaceTypeName:
                    constructingTypeName = ImmutableHashSetTypeName;
                    break;
                default:
                    // We verified that the type is an immutable collection, so the
                    // generic definition is one of the above.
                    return null;
            }

            return underlyingType.Assembly.GetType(constructingTypeName);
        }

        private static Type GetImmutableDictionaryConstructingType(Type type)
        {
            Debug.Assert(type.IsImmutableDictionaryType());

            // Use the generic type definition of the immutable collection to determine
            // an appropriate constructing type, i.e. a type that we can invoke the
            // `CreateRange<T>` method on, which returns the desired immutable collection.
            Type underlyingType = type.GetGenericTypeDefinition();
            string constructingTypeName;

            switch (underlyingType.FullName)
            {
                case ImmutableDictionaryGenericTypeName:
                case ImmutableDictionaryGenericInterfaceTypeName:
                    constructingTypeName = ImmutableDictionaryTypeName;
                    break;
                case ImmutableSortedDictionaryGenericTypeName:
                    constructingTypeName = ImmutableSortedDictionaryTypeName;
                    break;
                default:
                    // We verified that the type is an immutable collection, so the
                    // generic definition is one of the above.
                    return null;
            }

            return underlyingType.Assembly.GetType(constructingTypeName);
        }

        public static bool IsNonGenericStackOrQueue(this Type type)
        {
            const string stackTypeName = "System.Collections.Stack, System.Collections.NonGeneric";
            const string queueTypeName = "System.Collections.Queue, System.Collections.NonGeneric";

            Type stackType = GetTypeIfExists(stackTypeName);
            if (stackType?.IsAssignableFrom(type) == true)
            {
                return true;
            }

            Type queueType = GetTypeIfExists(queueTypeName);
            if (queueType?.IsAssignableFrom(type) == true)
            {
                return true;
            }

            return false;
        }

        // This method takes an unannotated string which makes linker reflection analysis lose track of the type we are
        // looking for. This indirection allows the removal of the type if it is not used in the calling application.
        private static Type GetTypeIfExists(string name) => Type.GetType(name, false);
    }
}
