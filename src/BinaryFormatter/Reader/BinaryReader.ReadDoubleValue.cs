using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    public ref partial struct BinaryReader
    {
        public double GetDouble()
        {
            Debug.Assert(ValueSpan.Length == 8);

            return BitConverter.ToDouble(ValueSpan);
        }
    }
}
