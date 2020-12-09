using System;
using System.Runtime.InteropServices;

namespace Xfrogcn.BinaryFormatter
{
    public sealed partial class BinaryWriter
    {
        public void WriteStringValue(ReadOnlySpan<byte> value)
        {
            WriteBytesValue(value);
        }
    }
}
