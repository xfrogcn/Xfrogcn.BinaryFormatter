using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    internal enum StackFramePropertyState : byte
    {
        None = 0,

        ReadName,   // Read the name of the property.
        ReadKeySeq,
        ReadKeyTypeSeq,
        ReadKey,
        Name,   // Verify or process the name.
        ReadValueTypeSeq,
        ReadValue,  // Read the value of the property.
        ReadValueIsEnd, // Determine if we are done reading.
        TryReadTypeSeq,
        TryRead,    // Perform the actual call to the converter's TryRead().
        WriteKeySeq,
        WriteKey,
        WriteValue
    }
}
