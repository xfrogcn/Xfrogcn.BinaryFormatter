using System;
using System.Numerics;

namespace Xfrogcn.BinaryFormatter
{
    public sealed partial class BinaryWriter
    {
        public void WriteComplexValue(Complex value)
        {
            Span<byte> data = stackalloc byte[8 * 2];

            BitConverter.GetBytes(value.Real).CopyTo(data);
            BitConverter.GetBytes(value.Imaginary).CopyTo(data.Slice(8));
            
            WriteBytesValue(data);
        }
    }
}
