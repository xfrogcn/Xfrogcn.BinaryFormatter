using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static Xfrogcn.BinaryFormatter.BinaryClassInfo;

namespace Xfrogcn.BinaryFormatter.Serialization
{
    internal sealed class ReflectionEmitMemberAccessor : MemberAccessor
    {
        const int MaxParameterCount = 64;
        const int UnboxedParameterCountThreshold = 4;
       

        public override ConstructorDelegate CreateConstructor(Type type)
        {
            Debug.Assert(type != null);
            ConstructorInfo realMethod = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, binder: null, Type.EmptyTypes, modifiers: null);

            if (type.IsAbstract)
            {
                return null;
            }

            if (realMethod == null && !type.IsValueType)
            {
                return null;
            }

            var dynamicMethod = new DynamicMethod(
                ConstructorInfo.ConstructorName,
                TypeMap.ObjectType,
                Type.EmptyTypes,
                typeof(ReflectionEmitMemberAccessor).Module,
                skipVisibility: true);

            ILGenerator generator = dynamicMethod.GetILGenerator();

            if (realMethod == null)
            {
                LocalBuilder local = generator.DeclareLocal(type);

                generator.Emit(OpCodes.Ldloca_S, local);
                generator.Emit(OpCodes.Initobj, type);
                generator.Emit(OpCodes.Ldloc, local);
                generator.Emit(OpCodes.Box, type);
            }
            else
            {
                generator.Emit(OpCodes.Newobj, realMethod);
            }

            generator.Emit(OpCodes.Ret);

            return (ConstructorDelegate)dynamicMethod.CreateDelegate(typeof(ConstructorDelegate));
        }

        public override ParameterizedConstructorDelegate<T> CreateParameterizedConstructor<T>(ConstructorInfo constructor) =>
            CreateDelegate<ParameterizedConstructorDelegate<T>>(CreateParameterizedConstructor(constructor));

        private static DynamicMethod CreateParameterizedConstructor(ConstructorInfo constructor)
        {
            Type type = constructor.DeclaringType;

            Debug.Assert(type != null);
            Debug.Assert(!type.IsAbstract);
            Debug.Assert(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Contains(constructor));

            ParameterInfo[] parameters = constructor.GetParameters();
            int parameterCount = parameters.Length;

            if (parameterCount > MaxParameterCount)
            {
                return null;
            }

            var dynamicMethod = new DynamicMethod(
                ConstructorInfo.ConstructorName,
                type,
                new[] { typeof(object[]) },
                typeof(ReflectionEmitMemberAccessor).Module,
                skipVisibility: true);

            ILGenerator generator = dynamicMethod.GetILGenerator();

            for (int i = 0; i < parameterCount; i++)
            {
                Type paramType = parameters[i].ParameterType;

                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldc_I4_S, i);
                generator.Emit(OpCodes.Ldelem_Ref);
                generator.Emit(OpCodes.Unbox_Any, paramType);
            }

            generator.Emit(OpCodes.Newobj, constructor);
            generator.Emit(OpCodes.Ret);

            return dynamicMethod;
        }

        public override ParameterizedConstructorDelegate<T, TArg0, TArg1, TArg2, TArg3>
            CreateParameterizedConstructor<T, TArg0, TArg1, TArg2, TArg3>(ConstructorInfo constructor) =>
            CreateDelegate<ParameterizedConstructorDelegate<T, TArg0, TArg1, TArg2, TArg3>>(
                CreateParameterizedConstructor(constructor, typeof(TArg0), typeof(TArg1), typeof(TArg2), typeof(TArg3)));

        private static DynamicMethod CreateParameterizedConstructor(ConstructorInfo constructor, Type parameterType1, Type parameterType2, Type parameterType3, Type parameterType4)
        {
            Type type = constructor.DeclaringType;

            Debug.Assert(type != null);
            Debug.Assert(!type.IsAbstract);
            Debug.Assert(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Contains(constructor));

            ParameterInfo[] parameters = constructor.GetParameters();
            int parameterCount = parameters.Length;

            var dynamicMethod = new DynamicMethod(
                ConstructorInfo.ConstructorName,
                type,
                new[] { parameterType1, parameterType2, parameterType3, parameterType4 },
                typeof(ReflectionEmitMemberAccessor).Module,
                skipVisibility: true);

            ILGenerator generator = dynamicMethod.GetILGenerator();

            for (int index = 0; index < parameterCount; index++)
            {
                Debug.Assert(index <= UnboxedParameterCountThreshold);

                generator.Emit(
                    index switch
                    {
                        0 => OpCodes.Ldarg_0,
                        1 => OpCodes.Ldarg_1,
                        2 => OpCodes.Ldarg_2,
                        3 => OpCodes.Ldarg_3,
                        _ => throw new InvalidOperationException()
                    });
            }

            generator.Emit(OpCodes.Newobj, constructor);
            generator.Emit(OpCodes.Ret);

            return dynamicMethod;
        }

        public override Action<TCollection, object> CreateAddMethodDelegate<TCollection>() =>
            CreateDelegate<Action<TCollection, object>>(CreateAddMethodDelegate(typeof(TCollection)));

        private static DynamicMethod CreateAddMethodDelegate(Type collectionType)
        {
            // We verified this won't be null when we created the converter that calls this method.
            MethodInfo realMethod = (collectionType.GetMethod("Push") ?? collectionType.GetMethod("Enqueue"))!;

            var dynamicMethod = new DynamicMethod(
                realMethod.Name,
                typeof(void),
                new[] { collectionType, TypeMap.ObjectType },
                typeof(ReflectionEmitMemberAccessor).Module,
                skipVisibility: true);

            ILGenerator generator = dynamicMethod.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Callvirt, realMethod);
            generator.Emit(OpCodes.Ret);

            return dynamicMethod;
        }


        public override Action<TCollection, TElement> CreateIEnumerableAddMethod<TCollection, TElement>()
        {
            Type collectionType = typeof(TCollection);
            Type elementType = typeof(TElement);
            MethodInfo realMethod = collectionType.GetMethod("Add") ?? collectionType.GetMethod("Push") ?? collectionType.GetMethod("Enqueue");

            if (realMethod == null)
            {
                return null;
            }

            return CreateDelegate<Action<TCollection, TElement>>(CreateIEnumerableAddMethodDelegate(collectionType, elementType, realMethod));
        }

        private static DynamicMethod CreateIEnumerableAddMethodDelegate(Type collectionType, Type elementType, MethodInfo realMethod)
        {
           
            var dynamicMethod = new DynamicMethod(
                realMethod.Name,
                typeof(void),
                new[] { collectionType, elementType },
                typeof(ReflectionEmitMemberAccessor).Module,
                skipVisibility: true);

            ILGenerator generator = dynamicMethod.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Callvirt, realMethod);
            generator.Emit(OpCodes.Ret);

            return dynamicMethod;
        }

        public override Action<TCollection, TKey, TValue> CreateDictionaryAddMethod<TCollection, TKey, TValue>()
        {
            Type collectionType = typeof(TCollection);
            Type keyType = typeof(TKey);
            Type valueType = typeof(TValue);
            MethodInfo realMethod = collectionType.GetMethod(
                "Add",
                new Type[] { keyType, valueType });
            if (realMethod != null)
            {
                var dynamicMethod = new DynamicMethod(
                realMethod.Name,
                typeof(void),
                new[] { collectionType, keyType, valueType },
                typeof(ReflectionEmitMemberAccessor).Module,
                skipVisibility: true);

                ILGenerator generator = dynamicMethod.GetILGenerator();

                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Ldarg_2);
                generator.Emit(OpCodes.Callvirt, realMethod);
                generator.Emit(OpCodes.Ret);

                return CreateDelegate<Action<TCollection, TKey, TValue>>(dynamicMethod);
            }

            realMethod = collectionType.GetMethod(
                "Add",
                new Type[] { typeof(KeyValuePair<TKey,TValue>) });
            if (realMethod != null)
            {
                var dynamicMethod = new DynamicMethod(
               realMethod.Name,
               typeof(void),
               new[] { collectionType, typeof(KeyValuePair<TKey,TValue>) },
               typeof(ReflectionEmitMemberAccessor).Module,
               skipVisibility: true);

                ILGenerator generator = dynamicMethod.GetILGenerator();

                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Callvirt, realMethod);
                generator.Emit(OpCodes.Ret);

                var addMethod = CreateDelegate<Action<TCollection, KeyValuePair<TKey,TValue>>>(dynamicMethod);

                return (dic, key, value) =>
                {
                    addMethod(dic, new KeyValuePair<TKey, TValue>(key, value));
                };
            }

            return null;
        }

        /// <summary>
        /// 创建不可变集合类型对应的CreateRange构造方法
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <typeparam name="TCollection"></typeparam>
        /// <returns></returns>
        public override Func<IEnumerable<TElement>, TCollection> CreateImmutableEnumerableCreateRangeDelegate<TElement, TCollection>() =>
            CreateDelegate<Func<IEnumerable<TElement>, TCollection>>(
                CreateImmutableEnumerableCreateRangeDelegate(typeof(TCollection), typeof(TElement), typeof(IEnumerable<TElement>)));


        private static DynamicMethod CreateImmutableEnumerableCreateRangeDelegate(Type collectionType, Type elementType, Type enumerableType)
        {
            MethodInfo realMethod = collectionType.GetImmutableEnumerableCreateRangeMethod(elementType);

            var dynamicMethod = new DynamicMethod(
                realMethod.Name,
                collectionType,
                new[] { enumerableType },
                typeof(ReflectionEmitMemberAccessor).Module,
                skipVisibility: true);

            ILGenerator generator = dynamicMethod.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Call, realMethod);
            generator.Emit(OpCodes.Ret);

            return dynamicMethod;
        }

        public override Func<IEnumerable<KeyValuePair<TKey, TElement>>, TCollection> CreateImmutableDictionaryCreateRangeDelegate<TKey, TElement, TCollection>() =>
            CreateDelegate<Func<IEnumerable<KeyValuePair<TKey, TElement>>, TCollection>>(
                CreateImmutableDictionaryCreateRangeDelegate(typeof(TCollection), typeof(TKey), typeof(TElement), typeof(IEnumerable<KeyValuePair<TKey, TElement>>)));

        private static DynamicMethod CreateImmutableDictionaryCreateRangeDelegate(Type collectionType, Type keyType, Type elementType, Type enumerableType)
        {
            MethodInfo realMethod = collectionType.GetImmutableDictionaryCreateRangeMethod(keyType, elementType);

            var dynamicMethod = new DynamicMethod(
                realMethod.Name,
                collectionType,
                new[] { enumerableType },
                typeof(ReflectionEmitMemberAccessor).Module,
                skipVisibility: true);

            ILGenerator generator = dynamicMethod.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Call, realMethod);
            generator.Emit(OpCodes.Ret);

            return dynamicMethod;
        }

        public override Func<TCollection, int> CreateIEnumerableCountMethod<TCollection>()
        {
            Type collectionType = typeof(TCollection);
            MemberInfo mi = collectionType.GetProperty("Count", BindingFlags.Public | BindingFlags.Instance);
            if (mi == null)
            {
                mi = collectionType.GetField("Count", BindingFlags.Public | BindingFlags.Instance);
            }
            if (mi == null)
            {
                mi = collectionType.GetProperty("Length", BindingFlags.Public | BindingFlags.Instance);
                if (mi == null)
                {
                    mi = collectionType.GetField("Length", BindingFlags.Public | BindingFlags.Instance);
                }
            }

            if (mi == null )
            {
                return null;
            }

            if (mi is PropertyInfo pi && pi.PropertyType == typeof(int))
            {
                return CreateDelegate<Func<TCollection, int>>(CreatePropertyGetter(pi, pi.PropertyType));
            }
            else if (mi is FieldInfo fi && fi.FieldType == typeof(int))
            {
                return CreateDelegate<Func<TCollection, int>>(CreateFieldGetter(fi, fi.FieldType));
            }

            return null;
        }

        public override Func<object, TProperty> CreatePropertyGetter<TProperty>(PropertyInfo propertyInfo) =>
            CreateDelegate<Func<object, TProperty>>(CreatePropertyGetter(propertyInfo, typeof(TProperty)));

        private static DynamicMethod CreatePropertyGetter(PropertyInfo propertyInfo, Type runtimePropertyType)
        {
            MethodInfo realMethod = propertyInfo.GetMethod;
            Debug.Assert(realMethod != null);

            Type declaringType = propertyInfo.DeclaringType;
            Debug.Assert(declaringType != null);

            Type declaredPropertyType = propertyInfo.PropertyType;

            DynamicMethod dynamicMethod = CreateGetterMethod(propertyInfo.Name, runtimePropertyType);
            ILGenerator generator = dynamicMethod.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);

            if (declaringType.IsValueType)
            {
                generator.Emit(OpCodes.Unbox, declaringType);
                generator.Emit(OpCodes.Call, realMethod);
            }
            else
            {
                generator.Emit(OpCodes.Castclass, declaringType);
                generator.Emit(OpCodes.Callvirt, realMethod);
            }


            if (declaredPropertyType != runtimePropertyType && declaredPropertyType.IsValueType)
            {
                generator.Emit(OpCodes.Box, declaredPropertyType);
            }

            generator.Emit(OpCodes.Ret);

            return dynamicMethod;
        }

        public override Action<object, TProperty> CreatePropertySetter<TProperty>(PropertyInfo propertyInfo) =>
            CreateDelegate<Action<object, TProperty>>(CreatePropertySetter(propertyInfo, typeof(TProperty)));

        private static DynamicMethod CreatePropertySetter(PropertyInfo propertyInfo, Type runtimePropertyType)
        {
            MethodInfo realMethod = propertyInfo.SetMethod;
            Debug.Assert(realMethod != null);

            Type declaringType = propertyInfo.DeclaringType;
            Debug.Assert(declaringType != null);

            Type declaredPropertyType = propertyInfo.PropertyType;

            DynamicMethod dynamicMethod = CreateSetterMethod(propertyInfo.Name, runtimePropertyType);
            ILGenerator generator = dynamicMethod.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(declaringType.IsValueType ? OpCodes.Unbox : OpCodes.Castclass, declaringType);
            generator.Emit(OpCodes.Ldarg_1);

         
            if (declaredPropertyType != runtimePropertyType && declaredPropertyType.IsValueType)
            {
                generator.Emit(OpCodes.Unbox_Any, declaredPropertyType);
            }

            generator.Emit(declaringType.IsValueType ? OpCodes.Call : OpCodes.Callvirt, realMethod);
            generator.Emit(OpCodes.Ret);

            return dynamicMethod;
        }

        public override Func<object, TProperty> CreateFieldGetter<TProperty>(FieldInfo fieldInfo) =>
            CreateDelegate<Func<object, TProperty>>(CreateFieldGetter(fieldInfo, typeof(TProperty)));

        private static DynamicMethod CreateFieldGetter(FieldInfo fieldInfo, Type runtimeFieldType)
        {
            Type declaringType = fieldInfo.DeclaringType;
            Debug.Assert(declaringType != null);

            Type declaredFieldType = fieldInfo.FieldType;

            DynamicMethod dynamicMethod = CreateGetterMethod(fieldInfo.Name, runtimeFieldType);
            ILGenerator generator = dynamicMethod.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(
                declaringType.IsValueType
                    ? OpCodes.Unbox
                    : OpCodes.Castclass,
                declaringType);
            generator.Emit(OpCodes.Ldfld, fieldInfo);


            if (declaredFieldType.IsValueType && declaredFieldType != runtimeFieldType)
            {
                generator.Emit(OpCodes.Box, declaredFieldType);
            }

            generator.Emit(OpCodes.Ret);

            return dynamicMethod;
        }

        public override Action<object, TProperty> CreateFieldSetter<TProperty>(FieldInfo fieldInfo) =>
            CreateDelegate<Action<object, TProperty>>(CreateFieldSetter(fieldInfo, typeof(TProperty)));


        public override Func<object, TProperty> CreateMethodGetter<TProperty>(MethodInfo methodInfo)
        {
            Type declaringType = methodInfo.DeclaringType;
            Debug.Assert(declaringType != null);

            Type declaredFieldType = methodInfo.ReturnType;
            Type runtimeFieldType = typeof(TProperty);

            var dynamicMethod = new DynamicMethod(
                methodInfo.Name,
                methodInfo.ReturnType,
                new[] { TypeMap.ObjectType },
                typeof(ReflectionEmitMemberAccessor).Module,
                skipVisibility: true);

            ILGenerator generator = dynamicMethod.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(
                declaringType.IsValueType
                    ? OpCodes.Unbox
                    : OpCodes.Castclass,
                declaringType);
            if (declaringType.IsValueType)
            {
                generator.Emit(OpCodes.Call, methodInfo);
            }
            else
            {
                generator.Emit(OpCodes.Callvirt, methodInfo);
            }


            if (declaredFieldType.IsValueType && declaredFieldType != runtimeFieldType)
            {
                generator.Emit(OpCodes.Box, declaredFieldType);
            }

            generator.Emit(OpCodes.Ret);

            return CreateDelegate<Func<object, TProperty>>(dynamicMethod);

        }
        

        private static DynamicMethod CreateFieldSetter(FieldInfo fieldInfo, Type runtimeFieldType)
        {
            Type declaringType = fieldInfo.DeclaringType;
            Debug.Assert(declaringType != null);

            Type declaredFieldType = fieldInfo.FieldType;

            DynamicMethod dynamicMethod = CreateSetterMethod(fieldInfo.Name, runtimeFieldType);
            ILGenerator generator = dynamicMethod.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(declaringType.IsValueType ? OpCodes.Unbox : OpCodes.Castclass, declaringType);
            generator.Emit(OpCodes.Ldarg_1);

            if (declaredFieldType != runtimeFieldType && declaredFieldType.IsValueType)
            {
                generator.Emit(OpCodes.Unbox_Any, declaredFieldType);
            }

            generator.Emit(OpCodes.Stfld, fieldInfo);
            generator.Emit(OpCodes.Ret);

            return dynamicMethod;
        }



        private static DynamicMethod CreateGetterMethod(string memberName, Type memberType) =>
            new DynamicMethod(
                memberName + "Getter",
                memberType,
                new[] { TypeMap.ObjectType },
                typeof(ReflectionEmitMemberAccessor).Module,
                skipVisibility: true);

        private static DynamicMethod CreateSetterMethod(string memberName, Type memberType) =>
            new DynamicMethod(
                memberName + "Setter",
                typeof(void),
                new[] { TypeMap.ObjectType, memberType },
                typeof(ReflectionEmitMemberAccessor).Module,
                skipVisibility: true);

        [return: NotNullIfNotNull("method")]
        private static T CreateDelegate<T>(DynamicMethod method) where T : Delegate =>
            (T)method?.CreateDelegate(typeof(T));
    }
}
