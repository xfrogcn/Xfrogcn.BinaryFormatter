using System;
using System.Collections.Generic;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class ICollectionOfTConverter<TCollection, TElement>
        : IEnumeratorOfTConverter<TCollection, TElement>
        where TCollection : ICollection<TElement>
    {
        protected override void Add(in TElement value, ref ReadStack state)
        {
            TCollection collection = (TCollection)state.Current.ReturnValue!;
            collection.Add(value);
            if(typeof(TCollection).IsValueType)
            {
                state.Current.ReturnValue = collection;
            };
        }

        protected override void CreateCollection(ref BinaryReader reader, ref ReadStack state, BinarySerializerOptions options, ulong len)
        {
            BinaryClassInfo classInfo = state.Current.BinaryClassInfo;

            TCollection returnValue = (TCollection)classInfo.CreateObject()!;
            state.Current.ReturnValue = returnValue;
        }

        protected override long GetLength(TCollection value, BinarySerializerOptions options, ref WriteStack state)
        {
            return value.Count;
        }

        
    }
}
