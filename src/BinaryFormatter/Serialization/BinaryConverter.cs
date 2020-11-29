using System;

namespace Xfrogcn.BinaryFormatter.Serialization
{
    public abstract partial class JsonConverter
    {
        internal JsonConverter() { }

        public abstract bool CanConvert(Type typeToConvert);
    }
}
