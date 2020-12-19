﻿using System;
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

        protected override void CreateCollection(ref BinaryReader reader, ref ReadStack state, BinarySerializerOptions options)
        {
            BinaryClassInfo classInfo = state.Current.BinaryClassInfo;


            //if (classInfo.CreateObject == null)
            //{
            //    ThrowHelper.ThrowNotSupportedException_DeserializeNoConstructor(TypeToConvert, ref reader, ref state);
            //}

            TCollection returnValue = (TCollection)classInfo.CreateObject()!;

            //if (returnValue.IsReadOnly)
            //{
            //    ThrowHelper.ThrowNotSupportedException_CannotPopulateCollection(TypeToConvert, ref reader, ref state);
            //}

            state.Current.ReturnValue = returnValue;
        }

        protected override long GetLength(TCollection value, BinarySerializerOptions options, ref WriteStack state)
        {
            return value.Count;
        }

        
    }
}
