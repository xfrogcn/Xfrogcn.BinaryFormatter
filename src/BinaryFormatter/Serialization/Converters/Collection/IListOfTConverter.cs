using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class IListOfTConverter<TCollection, TElement>
       : IEnumerableDefaultConverter<TCollection, TElement>
       where TCollection : IList<TElement>
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
