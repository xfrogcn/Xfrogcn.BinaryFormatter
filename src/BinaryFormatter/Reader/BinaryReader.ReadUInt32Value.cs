using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    public ref partial struct BinaryReader
    {
        public uint GetUInt32()
        {
            Debug.Assert(ValueSpan.Length == 4);

            return BitConverter.ToUInt32(ValueSpan);
        }
    }
}
