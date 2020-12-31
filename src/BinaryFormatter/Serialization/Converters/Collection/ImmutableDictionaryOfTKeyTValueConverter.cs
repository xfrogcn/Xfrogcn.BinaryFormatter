using System;
using System.Collections.Generic;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class ImmutableDictionaryOfTKeyTValueConverter<TCollection, TKey, TValue>
        : DictionaryEnumeratorConverter<TCollection, TKey, TValue>
        where TCollection : IReadOnlyDictionary<TKey, TValue>
        where TKey : notnull
    {
        protected override void Add(TKey key, in TValue value, BinarySerializerOptions options, ref ReadStack state)
        {
            ((Dictionary<TKey, TValue>)state.Current.ReturnValue!)[key] = value;
        }

        protected override void CreateCollection(ref BinaryReader reader, ref ReadStack state)
        {
            state.Current.ReturnValue = new Dictionary<TKey, TValue>();
        }

        protected override void ConvertCollection(ref ReadStack state, BinarySerializerOptions options)
        {
            BinaryClassInfo classInfo = state.Current.BinaryClassInfo;

            Func<IEnumerable<KeyValuePair<TKey, TValue>>, TCollection> creator = (Func<IEnumerable<KeyValuePair<TKey, TValue>>, TCollection>)classInfo.CreateObjectWithArgs;
            if (creator == null)
            {
                creator = options.MemberAccessorStrategy.CreateImmutableDictionaryCreateRangeDelegate<TKey,TValue, TCollection>();
                classInfo.CreateObjectWithArgs = creator;
            }

            state.Current.ReturnValue = creator((Dictionary<TKey, TValue>)state.Current.ReturnValue!);
        }
    }
}
