using System;
namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal abstract class IEnumerableDefaultConverter<TCollection, TElement>
        : BinaryCollectionConverter<TCollection, TElement>
    {
    }
}
