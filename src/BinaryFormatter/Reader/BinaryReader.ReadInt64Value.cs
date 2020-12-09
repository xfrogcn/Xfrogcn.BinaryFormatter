using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    public ref partial struct BinaryReader
    {
        public long GetInt64()
        {
            Debug.Assert(ValueSpan.Length == 8);

            return BitConverter.ToInt64(ValueSpan);
        }
    }
}
