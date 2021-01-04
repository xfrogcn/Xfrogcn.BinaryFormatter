using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal class IEnumerableConverterFactory : BinaryConverterFactory
    {
       // private static readonly IDictionaryConverter<IDictionary> s_converterForIDictionary = new IDictionaryConverter<IDictionary>();
       private static readonly IListConverter<IList> s_converterForIList = new IListConverter<IList>();
        private static readonly BitArrayConverter s_converterForBitArray = new BitArrayConverter();

        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(IEnumerable).IsAssignableFrom(typeToConvert);
        }

        public override BinaryConverter CreateConverter(Type typeToConvert, BinarySerializerOptions options)
        {
            Type[] genericArgs;
            Type elementType = null;
            Type dictionaryKeyType = null;
            Type actualTypeToConvert;

            Type converterType;
            // Array
            if (typeToConvert.IsArray)
            {
                // Verify that we don't have a multidimensional array.
                if (typeToConvert.GetArrayRank() > 1)
                {
                    ThrowHelper.ThrowNotSupportedException_SerializationNotSupported(typeToConvert);
                }

                converterType = typeof(ArrayConverter<,>);
                elementType = typeToConvert.GetElementType();
            }
            // Immutable dictionaries from System.Collections.Immutable, e.g. ImmutableDictionary<TKey, TValue>
            else if (typeToConvert.IsImmutableDictionaryType())
            {
                genericArgs = typeToConvert.GetGenericArguments();
                converterType = typeof(ImmutableDictionaryOfTKeyTValueConverter<,,>);
                dictionaryKeyType = genericArgs[0];
                elementType = genericArgs[1];
            }
            // IDictionary<TKey, TValue> or deriving from IDictionary<TKey, TValue>
            else if ((actualTypeToConvert = typeToConvert.GetCompatibleGenericInterface(typeof(IDictionary<,>))) != null)
            {
                genericArgs = actualTypeToConvert.GetGenericArguments();
                converterType = typeof(IDictionaryOfTKeyTValueConverter<,,>);
                dictionaryKeyType = genericArgs[0];
                elementType = genericArgs[1];
            }
            // Immutable non-dictionaries from System.Collections.Immutable, e.g. ImmutableStack<T>
            else if (typeToConvert.IsImmutableEnumerableType())
            {
                converterType = typeof(ImmutableEnumerableOfTConverter<,>);
                elementType = typeToConvert.GetGenericArguments()[0];
            }
            // IList<>
            else if ((actualTypeToConvert = typeToConvert.GetCompatibleGenericInterface(typeof(IList<>))) != null)
            {
                converterType = typeof(IListOfTConverter<,>);
                elementType = actualTypeToConvert.GetGenericArguments()[0];
            }
            // ISet<>
            else if ((actualTypeToConvert = typeToConvert.GetCompatibleGenericInterface(typeof(ISet<>))) != null)
            {
                converterType = typeof(ISetOfTConverter<,>);
                elementType = actualTypeToConvert.GetGenericArguments()[0];
            }
            // ICollection<>
            else if ((actualTypeToConvert = typeToConvert.GetCompatibleGenericInterface(typeof(ICollection<>))) != null)
            {
                converterType = typeof(ICollectionOfTConverter<,>);
                elementType = actualTypeToConvert.GetGenericArguments()[0];
            }
            // Stack<> or deriving from Stack<>
            else if ((actualTypeToConvert = typeToConvert.GetCompatibleGenericBaseClass(typeof(Stack<>))) != null)
            {
                converterType = typeof(StackOfTConverter<,>);
                elementType = actualTypeToConvert.GetGenericArguments()[0];
            }
            // Queue<> or deriving from Queue<>
            else if ((actualTypeToConvert = typeToConvert.GetCompatibleGenericBaseClass(typeof(Queue<>))) != null)
            {
                converterType = typeof(QueueOfTConverter<,>);
                elementType = actualTypeToConvert.GetGenericArguments()[0];
            }
            // ConcurrentStack<> or deriving from ConcurrentStack<>
            else if ((actualTypeToConvert = typeToConvert.GetCompatibleGenericBaseClass(typeof(ConcurrentStack<>))) != null)
            {
                converterType = typeof(ConcurrentStackOfTConverter<,>);
                elementType = actualTypeToConvert.GetGenericArguments()[0];
            }
            // ConcurrentQueue<> or deriving from ConcurrentQueue<>
            else if ((actualTypeToConvert = typeToConvert.GetCompatibleGenericBaseClass(typeof(ConcurrentQueue<>))) != null)
            {
                converterType = typeof(ConcurrentQueueOfTConverter<,>);
                elementType = actualTypeToConvert.GetGenericArguments()[0];
            }
            // ConcurrentBag<> or deriving from ConcurrentBag<>
            else if ((actualTypeToConvert = typeToConvert.GetCompatibleGenericBaseClass(typeof(ConcurrentBag<>))) != null)
            {
                converterType = typeof(ConcurrentBagOfTConverter<,>);
                elementType = actualTypeToConvert.GetGenericArguments()[0];
            }
            // IEnumerable<>, types assignable from List<>
            else if ((actualTypeToConvert = typeToConvert.GetCompatibleGenericInterface(typeof(IEnumerable<>))) != null)
            {
                converterType = typeof(IEnumerableOfTWithAddMethodConverter<,>);
                elementType = actualTypeToConvert.GetGenericArguments()[0];
                if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                {
                    converterType = typeof(IEnumerableKeyValuePairConverter<,,>);
                    dictionaryKeyType = elementType.GetTypeGenericArguments()[0];
                    elementType = elementType.GetTypeGenericArguments()[1];
                }
            }
            // Check for non-generics after checking for generics.
            else if (typeof(IDictionary).IsAssignableFrom(typeToConvert))
            {
                converterType = typeof(IDictionaryConverter<>);
            }
            else if (typeof(IList).IsAssignableFrom(typeToConvert))
            {
                if (typeToConvert == typeof(IList))
                {
                    return s_converterForIList;
                }

                converterType = typeof(IListConverter<>);
            }
            else if (typeToConvert == typeof(BitArray))
            {
                return s_converterForBitArray;
            }
            else
            {
                converterType = typeof(IEnumerableWithAddMethodConverter<>);
            }

            Type genericType;
            int numberOfGenericArgs = converterType.GetGenericArguments().Length;
            if (numberOfGenericArgs == 1)
            {
                genericType = converterType.MakeGenericType(typeToConvert);
            }
            else if (numberOfGenericArgs == 2)
            {
                genericType = converterType.MakeGenericType(typeToConvert, elementType!);
            }
            else
            {
                Debug.Assert(numberOfGenericArgs == 3);
                genericType = converterType.MakeGenericType(typeToConvert, dictionaryKeyType!, elementType!);
            }

            BinaryConverter converter = (BinaryConverter)Activator.CreateInstance(
                genericType,
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: null,
                culture: null)!;

            return converter;
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            
        }
    }
}
