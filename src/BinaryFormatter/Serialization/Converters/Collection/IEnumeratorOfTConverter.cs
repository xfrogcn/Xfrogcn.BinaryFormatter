using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal abstract class IEnumeratorOfTConverter<TCollection, TElement>
        : IEnumerableDefaultConverter<TCollection, TElement>
        where TCollection : IEnumerable<TElement>
    {
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
            if (!state.SupportContinuation)
            {
                do
                {
                    state.Current.WriteEnumerableIndex(index, writer);
                    TElement element = enumerator.Current;
                    converter.TryWrite(writer, element, options, ref state);
                    // 列表状态下，每个写每个元素时独立的，故需清理多态属性
                    state.Current.PolymorphicBinaryPropertyInfo = null;
                    index++;
                } while (enumerator.MoveNext());

            }
            else
            {
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

            }

            return true;
        }
    }
}
