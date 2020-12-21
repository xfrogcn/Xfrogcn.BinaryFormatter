using System.Collections.Generic;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class ISetOfTConverter<TCollection, TElement>
        : IEnumeratorOfTConverter<TCollection, TElement>
        where TCollection : ISet<TElement>
    {
        protected override void Add(in TElement value, ref ReadStack state)
        {
            ((TCollection)state.Current.ReturnValue!).Add(value);
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
