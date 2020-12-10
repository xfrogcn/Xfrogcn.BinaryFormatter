using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    public ref partial struct BinaryReader
    {
        public Complex GetComplex()
        {
            Debug.Assert(ValueSpan.Length == 8*2);

            double[] data = new double[2];

            data[0] = BitConverter.ToDouble(ValueSpan.Slice(0, 8));
            data[1] = BitConverter.ToDouble(ValueSpan.Slice(8, 8));


            return new Complex(data[0], data[1]);
        }
    }
}
