using System.Collections;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal class IDictionaryConverter<TCollection>
        : DictionaryDefaultConverter<TCollection, object, object>
        where TCollection : IDictionary
    {
        protected override void Add(object key, in object value, BinarySerializerOptions options, ref ReadStack state)
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

        protected internal override bool OnWriteResume(BinaryWriter writer, TCollection dictionary, BinarySerializerOptions options, ref WriteStack state)
        {
            IDictionaryEnumerator enumerator;
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
                enumerator = (IDictionaryEnumerator)state.Current.CollectionEnumerator;
            }

            if (!state.SupportContinuation)
            {
                do
                {
                    WriteKey(writer, enumerator.Key, options, ref state);
                    WriteValue(writer, enumerator.Value, options, ref state);

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

                    if (!WriteKey(writer, enumerator.Key, options, ref state))
                    {
                        state.Current.CollectionEnumerator = enumerator;
                        return false;
                    }

                    if (!WriteValue(writer, enumerator.Value, options, ref state))
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
