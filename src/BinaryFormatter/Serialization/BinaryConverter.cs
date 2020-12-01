using System;

namespace Xfrogcn.BinaryFormatter.Serialization
{
    public abstract partial class BinaryConverter
    {
        internal BinaryConverter() { }

        public abstract bool CanConvert(Type typeToConvert);
    }
}
