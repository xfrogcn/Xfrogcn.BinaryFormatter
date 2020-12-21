using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class QueueOfTConverter<TCollection, TElement>
        : IEnumeratorOfTConverter<TCollection, TElement>
        where TCollection : Queue<TElement>
    {
        protected override void Add(in TElement value, ref ReadStack state)
        {
            ((TCollection)state.Current.ReturnValue!).Enqueue(value);
        }

        protected override void CreateCollection(ref BinaryReader reader, ref ReadStack state, BinarySerializerOptions options, ulong len)
        {
            if (state.Current.BinaryClassInfo.CreateObject == null)
            {
                ThrowHelper.ThrowNotSupportedException_SerializationNotSupported(state.Current.BinaryClassInfo.Type);
            }

            state.Current.ReturnValue = state.Current.BinaryClassInfo.CreateObject();
        }

        protected override long GetLength(TCollection value, BinarySerializerOptions options, ref WriteStack state)
        {
            return value.Count;
        }

    }
}
