using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Serialization
{
    public abstract class ReferenceResolver
    {
        public abstract uint GetReference(object value, ulong offset, out bool alreadyExists);

        public abstract void AddReference(uint seq, ulong offset);


        public abstract Dictionary<uint, ulong> GetReferenceOffsetMap();
    }
}
