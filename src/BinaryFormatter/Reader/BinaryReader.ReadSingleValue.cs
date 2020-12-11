using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    public ref partial struct BinaryReader
    {
        public float GetSingle()
        {
            Debug.Assert(ValueSpan.Length == 4);

            return BitConverter.ToSingle(ValueSpan);
        }

    }
}
