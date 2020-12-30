using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Serialization
{
    public enum RefState
    {
        None,
        Start,
        Created
    }

    public abstract class ReferenceResolver
    {
        public abstract uint GetReference(object value, ulong offset, out bool alreadyExists);

        public abstract void AddReference(uint seq);

        public abstract void AddReferenceObject(uint seq, object value);

        public abstract bool AddReferenceCallback(object instance, object propertyValue, Func<object, object, bool> action);

        public abstract RefState TryGetReference(uint seq, out object value);

        public abstract Dictionary<uint, ulong> GetReferenceOffsetMap();
    }
}
