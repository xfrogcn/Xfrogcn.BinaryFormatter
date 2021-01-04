using System;
using System.Collections.Generic;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal class IDictionaryOfTKeyTValueConverter<TCollection, TKey, TValue>
        : DictionaryEnumeratorConverter<TCollection, TKey, TValue>
        where TCollection : IDictionary<TKey, TValue>
        where TKey : notnull
    {
        private readonly bool _isDictionary = false;

        public IDictionaryOfTKeyTValueConverter()
        {
            Type t = typeof(TCollection);
            if(t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                _isDictionary = true;
            }
        }

        protected override void Add(TKey key, in TValue value, BinarySerializerOptions options, ref ReadStack state)
        {
            TCollection collection = (TCollection)state.Current.ReturnValue!;
            collection[key] = value;
            if (IsValueType)
            {
                state.Current.ReturnValue = collection;
            };
        }

        protected override void CreateCollection(ref BinaryReader reader, ref ReadStack state)
        {
            BinaryClassInfo classInfo = state.Current.BinaryClassInfo;
            TCollection returnValue = (TCollection)classInfo.CreateObject()!;

            if (returnValue.IsReadOnly)
            {
                ThrowHelper.ThrowNotSupportedException_CannotPopulateCollection(TypeToConvert, ref reader, ref state);
            }

            state.Current.ReturnValue = returnValue;
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            base.SetTypeMetadata(typeInfo, typeMap, options);
            if (_isDictionary)
            {
                typeInfo.Type = TypeEnum.Dictionary;
                typeInfo.FullName = null;
            }
        }
    }
}
