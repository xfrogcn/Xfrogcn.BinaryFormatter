using System;
using System.Collections.Generic;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    // IImmutableList<T>
    //      System.Collections.Immutable.ImmutableArray<T>
    //      System.Collections.Immutable.ImmutableList<T>
    // IImmutableQueue<T>
    //      System.Collections.Immutable.ImmutableQueue<T>
    // IImmutableSet<T>
    //      System.Collections.Immutable.ImmutableHashSet<T>
    //      System.Collections.Immutable.ImmutableSortedSet<T>
    // IImmutableStack<T>
    //      System.Collections.Immutable.ImmutableStack<T>
    internal sealed class ImmutableEnumerableOfTConverter<TCollection, TElement>
        : IEnumerableDefaultConverter<TCollection, TElement>
        where TCollection : IEnumerable<TElement>
    {
        protected override void Add(in TElement value, ref ReadStack state)
        {
            throw new NotImplementedException();
        }

        protected override void CreateCollection(ref BinaryReader reader, ref ReadStack state, BinarySerializerOptions options)
        {
            throw new NotImplementedException();
        }

        protected override long GetLength(TCollection value, BinarySerializerOptions options, ref WriteStack state)
        {
            throw new NotImplementedException();
        }

        protected override bool OnWriteResume(BinaryWriter writer, TCollection value, BinarySerializerOptions options, ref WriteStack state)
        {
            throw new NotImplementedException();
        }
    }
}
