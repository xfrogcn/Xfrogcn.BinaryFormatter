﻿using System;
namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal abstract class BinaryCollectionConverter<TCollection, TElement> : BinaryResumableConverter<TCollection>
    {
        internal sealed override ClassType ClassType => ClassType.Enumerable;
        internal override Type ElementType => typeof(TElement);
    }
}
