using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class IEnumerableOfTWithAddMethodConverter<TCollection, TElement>
        : IEnumeratorOfTConverter<TCollection, TElement>
        where TCollection : IEnumerable<TElement>

    {
  
        protected override void Add(in TElement value, ref ReadStack state)
        {
            var addMethodDelegate = ((Action<TCollection, TElement>)state.Current.BinaryClassInfo.AddMethodDelegate);
            Debug.Assert(addMethodDelegate != null);
            addMethodDelegate((TCollection)state.Current.ReturnValue!, value);
        }

        protected override void CreateCollection(ref BinaryReader reader, ref ReadStack state, BinarySerializerOptions options, ulong len)
        {
            BinaryClassInfo classInfo = state.Current.BinaryClassInfo;
            BinaryClassInfo.ConstructorDelegate constructorDelegate = classInfo.CreateObject;

            if (constructorDelegate == null)
            {
                ThrowHelper.ThrowNotSupportedException_CannotPopulateCollection(TypeToConvert, ref reader, ref state);
            }

            if( classInfo.AddMethodDelegate == null)
            {
                Action<TCollection, TElement> addDelegate = options.MemberAccessorStrategy.CreateIEnumerableAddMethod<TCollection, TElement>();
                if (addDelegate == null)
                {
                    ThrowHelper.ThrowNotSupportedException_CannotPopulateCollection(TypeToConvert, ref reader, ref state);
                }

                classInfo.AddMethodDelegate = addDelegate;
            }

            state.Current.ReturnValue = constructorDelegate();

           
        }

        protected override long GetLength(TCollection value, BinarySerializerOptions options, ref WriteStack state)
        {
            BinaryClassInfo classInfo = state.Current.BinaryClassInfo;
            if (classInfo.CountMethodDelegate == null)
            {
                classInfo.CountMethodDelegate = options.MemberAccessorStrategy.CreateIEnumerableCountMethod<TCollection>();
                if (classInfo.CountMethodDelegate == null)
                {
                    Func<TCollection, int> linqCountDelegate = (collection) => collection.Count();
                    classInfo.CountMethodDelegate = linqCountDelegate;
                }
            }
            Func<TCollection, int> countDelegate = state.Current.BinaryClassInfo.CountMethodDelegate as Func<TCollection, int>;
            return countDelegate(value);
        }
    }
}
