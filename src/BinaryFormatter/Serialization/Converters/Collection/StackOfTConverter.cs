﻿using System.Collections.Generic;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class StackOfTConverter<TCollection, TElement>
        : IEnumeratorOfTConverter<TCollection, TElement>
        where TCollection : Stack<TElement>
    {
        protected override void Add(in TElement value, ref ReadStack state)
        {
            ((List<TElement>)state.Current.ReturnValue!).Add(value);
        }

        protected override void CreateCollection(ref BinaryReader reader, ref ReadStack state, BinarySerializerOptions options)
        {
            state.Current.ReturnValue = new List<TElement>();
        }

        protected override void ConvertCollection(ref ReadStack state, BinarySerializerOptions options)
        {
            if (state.Current.BinaryClassInfo.CreateObject == null)
            {
                ThrowHelper.ThrowNotSupportedException_SerializationNotSupported(state.Current.BinaryClassInfo.Type);
            }
            List<TElement> list = (List < TElement > )state.Current.ReturnValue;
            state.Current.ReturnValue = state.Current.BinaryClassInfo.CreateObject();
            int index = list.Count - 1;
            for (; index >= 0; index--)
            {
                ((Stack<TElement>)state.Current.ReturnValue).Push(list[index]);
            }
        }

        protected override long GetLength(TCollection value, BinarySerializerOptions options, ref WriteStack state)
        {
            return value.Count;
        }

       
    }
}
