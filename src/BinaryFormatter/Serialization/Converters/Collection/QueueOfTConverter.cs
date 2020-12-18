using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class QueueOfTConverter<TCollection, TElement>
        : IEnumerableDefaultConverter<TCollection, TElement>
        where TCollection : Queue<TElement>
    {
        protected override void Add(in TElement value, ref ReadStack state)
        {
            ((TCollection)state.Current.ReturnValue!).Enqueue(value);
        }

        protected override void CreateCollection(ref BinaryReader reader, ref ReadStack state, BinarySerializerOptions options)
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

        protected override bool OnWriteResume(BinaryWriter writer, TCollection value, BinarySerializerOptions options, ref WriteStack state)
        {
            IEnumerator<TElement> enumerator;
            if (state.Current.CollectionEnumerator == null)
            {
                enumerator = value.GetEnumerator();
                if (!enumerator.MoveNext())
                {
                    return true;
                }
            }
            else
            {
                enumerator = (IEnumerator<TElement>)state.Current.CollectionEnumerator;
            }

            BinaryConverter<TElement> converter = GetElementConverter(ref state);
            int index = state.Current.EnumeratorIndex;
            do
            {
                if (ShouldFlush(writer, ref state))
                {
                    state.Current.CollectionEnumerator = enumerator;
                    return false;
                }

                if (!state.Current.ProcessedEnumerableIndex)
                {
                    state.Current.WriteEnumerableIndex(index, writer);
                    state.Current.ProcessedEnumerableIndex = true;
                }

                TElement element = enumerator.Current;
                if (!converter.TryWrite(writer, element, options, ref state))
                {
                    state.Current.CollectionEnumerator = enumerator;
                    state.Current.EnumeratorIndex = index;
                    return false;
                }

                // 列表状态下，每个写每个元素时独立的，故需清理多态属性
                state.Current.PolymorphicBinaryPropertyInfo = null;
                state.Current.ProcessedEnumerableIndex = false;
                index++;

            } while (enumerator.MoveNext());

            return true;
        }

    }
}
