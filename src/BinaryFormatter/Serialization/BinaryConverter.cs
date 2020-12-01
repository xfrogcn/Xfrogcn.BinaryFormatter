using System;

namespace Xfrogcn.BinaryFormatter.Serialization
{
    public abstract partial class BinaryConverter
    {
        internal bool IsInternalConverter { get; set; }

        internal abstract Type TypeToConvert { get; }

        internal bool CanBeNull { get; }

        internal BinaryConverter() { }

        public abstract bool CanConvert(Type typeToConvert);
    }
}
