using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal class ObjectConverterFactory : BinaryConverterFactory
    {
        readonly HashSet<Type> _tupleTypes =
            new HashSet<Type>()
            {
                typeof(Tuple<>),
                typeof(Tuple<,>),
                typeof(Tuple<,,>),
                typeof(Tuple<,,,>),
                typeof(Tuple<,,,,>),
                typeof(Tuple<,,,,,>),
                typeof(Tuple<,,,,,,>),
                typeof(Tuple<,,,,,,,>),
            };

        readonly HashSet<Type> _valueTupleTypes =
            new HashSet<Type>()
            {
 
                typeof(ValueTuple<>),
                typeof(ValueTuple<,>),
                typeof(ValueTuple<,,>),
                typeof(ValueTuple<,,,>),
                typeof(ValueTuple<,,,,>),
                typeof(ValueTuple<,,,,,>),
                typeof(ValueTuple<,,,,,,>),
                typeof(ValueTuple<,,,,,,,>),
            };

        public override bool CanConvert(Type typeToConvert)
        {
            // This is the last built-in factory converter, so if the IEnumerableConverterFactory doesn't
            // support it, then it is not IEnumerable.
            Debug.Assert(!typeof(IEnumerable).IsAssignableFrom(typeToConvert));
            return true;
        }

        public override BinaryConverter CreateConverter(Type typeToConvert, BinarySerializerOptions options)
        {
            if (IsKeyValuePair(typeToConvert))
            {
                return CreateKeyValuePairConverter(typeToConvert, options);
            }

            BinaryConverter converter;
            ConstructorInfo constructor = GetDeserializationConstructor(typeToConvert);
            ParameterInfo[] parameters = constructor?.GetParameters();

            Type converterType;
            if (typeToConvert.IsGenericType && _valueTupleTypes.Contains(typeToConvert.GetGenericTypeDefinition()))
            {
                converterType = typeof(ValueTupleConverter<>).MakeGenericType(typeToConvert);
            }
            else if (constructor == null || typeToConvert.IsAbstract || parameters!.Length == 0)
            {
                converterType = typeof(ObjectDefaultConverter<>).MakeGenericType(typeToConvert);
            }
            else if (typeToConvert.IsGenericType && _tupleTypes.Contains(typeToConvert.GetGenericTypeDefinition()))
            {
                converterType = typeof(TupleConverter<>).MakeGenericType(typeToConvert);
            }
            else
            {
                int parameterCount = parameters.Length;

                if (!options.IgnoreCtorParameterCountThreshold && parameterCount <= BinarySerializerConstants.UnboxedParameterCountThreshold)
                {
                    Type placeHolderType = BinaryClassInfo.ObjectType;
                    Type[] typeArguments = new Type[BinarySerializerConstants.UnboxedParameterCountThreshold + 1];

                    typeArguments[0] = typeToConvert;
                    for (int i = 0; i < BinarySerializerConstants.UnboxedParameterCountThreshold; i++)
                    {
                        if (i < parameterCount)
                        {
                            typeArguments[i + 1] = parameters[i].ParameterType;
                        }
                        else
                        {
                            // Use placeholder arguments if there are less args than the threshold.
                            typeArguments[i + 1] = placeHolderType;
                        }
                    }

                    converterType = typeof(SmallObjectWithParameterizedConstructorConverter<,,,,>).MakeGenericType(typeArguments);
                }
                else
                {
                    converterType = typeof(LargeObjectWithParameterizedConstructorConverter<>).MakeGenericType(typeToConvert);
                }
            }

            converter = (BinaryConverter)Activator.CreateInstance(
                    converterType,
                    BindingFlags.Instance | BindingFlags.Public,
                    binder: null,
                    args: null,
                    culture: null)!;

            converter.ConstructorInfo = constructor!;
            return converter;
        }

        private bool IsKeyValuePair(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType)
                return false;

            Type generic = typeToConvert.GetGenericTypeDefinition();
            return (generic == typeof(KeyValuePair<,>));
        }

        private BinaryConverter CreateKeyValuePairConverter(Type type, BinarySerializerOptions options)
        {
            Debug.Assert(IsKeyValuePair(type));

            Type keyType = type.GetGenericArguments()[0];
            Type valueType = type.GetGenericArguments()[1];

            BinaryConverter converter = (BinaryConverter)Activator.CreateInstance(
                typeof(KeyValuePairConverter<,>).MakeGenericType(new Type[] { keyType, valueType }),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: null,
                culture: null)!;

            converter.Initialize(options);

            return converter;
        }

        private ConstructorInfo GetDeserializationConstructor(Type type)
        {
            ConstructorInfo ctorWithAttribute = null;
            ConstructorInfo publicParameterlessCtor = null;
            ConstructorInfo lonePublicCtor = null;

            ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            if (constructors.Length == 1)
            {
                lonePublicCtor = constructors[0];
            }

            foreach (ConstructorInfo constructor in constructors)
            {
                if (constructor.GetCustomAttribute<BinaryConstructorAttribute>() != null)
                {
                    if (ctorWithAttribute != null)
                    {
                        ThrowHelper.ThrowInvalidOperationException_SerializationDuplicateTypeAttribute<BinaryConstructorAttribute>(type);
                    }

                    ctorWithAttribute = constructor;
                }
                else if (constructor.GetParameters().Length == 0)
                {
                    publicParameterlessCtor = constructor;
                }
            }

            // For correctness, throw if multiple ctors have [BinaryConstructor], even if one or more are non-public.
            ConstructorInfo dummyCtorWithAttribute = ctorWithAttribute;

            constructors = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (ConstructorInfo constructor in constructors)
            {
                if (constructor.GetCustomAttribute<BinaryConstructorAttribute>() != null)
                {
                    if (dummyCtorWithAttribute != null)
                    {
                        ThrowHelper.ThrowInvalidOperationException_SerializationDuplicateTypeAttribute<BinaryConstructorAttribute>(type);
                    }

                    dummyCtorWithAttribute = constructor;
                }
            }

            // Structs will use default constructor if attribute isn't used.
            if (type.IsValueType && ctorWithAttribute == null)
            {
                return null;
            }

            return ctorWithAttribute ?? publicParameterlessCtor ?? lonePublicCtor;
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            
        }
    }
}
