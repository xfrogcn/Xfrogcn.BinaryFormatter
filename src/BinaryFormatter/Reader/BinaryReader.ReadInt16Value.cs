using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    public ref partial struct BinaryReader
    {
        public short GetInt16()
        {
            Debug.Assert(ValueSpan.Length == 2);

            return BitConverter.ToInt16(ValueSpan);
        }
    }
}
