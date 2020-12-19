using System;
using System.Collections;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    //internal sealed class IEnumerableWithAddMethodConverter<TCollection>
    //    : IEnumeratorOfTConverter<TCollection, object>
    //    where TCollection : IEnumerable
    //{
    //    protected override void Add(in object value, ref ReadStack state)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    protected override void CreateCollection(ref BinaryReader reader, ref ReadStack state, BinarySerializerOptions options)
    //    {
    //        BinaryClassInfo classInfo = state.Current.BinaryClassInfo;
    //        BinaryClassInfo.ConstructorDelegate constructorDelegate = classInfo.CreateObject;

    //        //if (constructorDelegate == null)
    //        //{
    //        //    ThrowHelper.ThrowNotSupportedException_CannotPopulateCollection(TypeToConvert, ref reader, ref state);
    //        //}

    //        state.Current.ReturnValue = constructorDelegate();

    //        // Initialize add method used to populate the collection.
    //        if (classInfo.AddMethodDelegate == null)
    //        {
    //            // We verified this exists when we created the converter in the enumerable converter factory.
    //            classInfo.AddMethodDelegate = options.MemberAccessorStrategy.CreateAddMethodDelegate<TCollection>();
    //        }
    //    }

    //    protected override long GetLength(TCollection value, BinarySerializerOptions options, ref WriteStack state)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
