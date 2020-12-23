using System;
using System.Collections.Generic;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal class IDictionaryOfTKeyTValueConverter<TCollection, TKey, TValue>
        : DictionaryEnumeratorConverter<TCollection, TKey, TValue>
        where TCollection : IDictionary<TKey, TValue>
        where TKey : notnull
    {
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
    }
}
