using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    public ref partial struct BinaryReader
    {
        public decimal GetDecimal()
        {
            Debug.Assert(ValueSpan.Length == 4*4);

            int[] intData = new int[4];

            intData[0] = BitConverter.ToInt32(ValueSpan.Slice(0, 4));
            intData[1] = BitConverter.ToInt32(ValueSpan.Slice(4, 4));
            intData[2] = BitConverter.ToInt32(ValueSpan.Slice(8, 4));
            intData[3] = BitConverter.ToInt32(ValueSpan.Slice(12, 4));

            return new decimal(intData);
        }
    }
}
