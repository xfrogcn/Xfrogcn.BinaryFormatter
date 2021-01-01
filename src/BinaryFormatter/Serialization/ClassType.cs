using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    /// <summary>
    /// Determines how a given class is treated when it is (de)serialized.
    /// </summary>
    /// <remarks>
    /// Although bit flags are used, a given ClassType can only be one value.
    /// Bit flags are used to efficiently compare against more than one value.
    /// </remarks>
    internal enum ClassType : byte
    {
        // Default - no class type.
        None = 0x0,
        // BinaryObjectConverter<> - objects with properties.
        Object = 0x1,
        // BinaryConverter<> - simple values.
        Value = 0x2,
        // BinaryValueConverter<> - simple values that need to re-enter the serializer such as KeyValuePair<TKey, TValue>.
        NewValue = 0x4,
        // BinaryIEnumerbleConverter<> - all enumerable collections except dictionaries.
        Enumerable = 0x8,
        // BinaryDictionaryConverter<,> - dictionary types.
        Dictionary = 0x10,
    }
}
