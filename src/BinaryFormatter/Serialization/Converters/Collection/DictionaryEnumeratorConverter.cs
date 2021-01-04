using System;
using System.Collections.Generic;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal abstract class DictionaryEnumeratorConverter<TCollection, TKey, TValue> : DictionaryDefaultConverter<TCollection, TKey, TValue>
        where TCollection : IEnumerable<KeyValuePair<TKey, TValue>>
        where TKey : notnull
    {

        protected internal override bool OnWriteResume(BinaryWriter writer, TCollection dictionary, BinarySerializerOptions options, ref WriteStack state)
        {
            IEnumerator<KeyValuePair<TKey, TValue>> enumerator;
            if (state.Current.CollectionEnumerator == null)
            {
                enumerator = dictionary.GetEnumerator();
                if (!enumerator.MoveNext())
                {
                    return true;
                }
            }
            else
            {
                enumerator = (IEnumerator<KeyValuePair<TKey, TValue>>)state.Current.CollectionEnumerator;
            }

            if (!state.SupportContinuation)
            {
                do
                {
                    WriteKey(writer, enumerator.Current.Key, options, ref state);
                    WriteValue(writer, enumerator.Current.Value, options, ref state);

                    state.Current.EndDictionaryElement();
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

                    if (!WriteKey(writer, enumerator.Current.Key, options, ref state))
                    {
                        state.Current.CollectionEnumerator = enumerator;
                        return false;
                    }

                    if (!WriteValue(writer, enumerator.Current.Value, options, ref state))
                    {
                        state.Current.CollectionEnumerator = enumerator;
                        return false;
                    }

                    state.Current.EndDictionaryElement();
                } while (enumerator.MoveNext());

            }

            return true;
        }
    }
}
