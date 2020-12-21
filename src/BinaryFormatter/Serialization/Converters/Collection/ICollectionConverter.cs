//using System;
//using System.Collections;
//using System.Collections.Generic;

//namespace Xfrogcn.BinaryFormatter.Serialization.Converters
//{
//    internal sealed class ICollectionConverter<TCollection>
//        : IEnumeratorOfTConverter<TCollection, object>
//        where TCollection : ICollection
//    {
//        protected override void Add(in object value, ref ReadStack state)
//        {
            
//            TCollection collection = (TCollection)state.Current.ReturnValue!;

//            collection.Add(value);
//            if(typeof(TCollection).IsValueType)
//            {
//                state.Current.ReturnValue = collection;
//            };
//        }

//        protected override void CreateCollection(ref BinaryReader reader, ref ReadStack state, BinarySerializerOptions options)
//        {
//            BinaryClassInfo classInfo = state.Current.BinaryClassInfo;

//            TCollection returnValue = (TCollection)classInfo.CreateObject()!;
//            state.Current.ReturnValue = returnValue;
//        }

//        protected override long GetLength(TCollection value, BinarySerializerOptions options, ref WriteStack state)
//        {
//            return value.Count;
//        }

        
//    }
//}
