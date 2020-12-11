using System;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal abstract class BinaryObjectConverter<T> : BinaryResumableConverter<T>
    {
        internal sealed override ClassType ClassType => ClassType.Object;
        internal sealed override Type ElementType => null;
    }
}
