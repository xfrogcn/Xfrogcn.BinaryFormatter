using System;
using System.Diagnostics;

namespace Xfrogcn.BinaryFormatter
{
    public sealed partial class BinaryWriter
    {
        public void WriteDecimalValue(decimal value)
        {
            int[] values = decimal.GetBits(value);

            Debug.Assert(values.Length == 4);

            Span<byte> data = stackalloc byte[4 * 4];

            BitConverter.GetBytes(values[0]).CopyTo(data);
            BitConverter.GetBytes(values[1]).CopyTo(data.Slice(4));
            BitConverter.GetBytes(values[2]).CopyTo(data.Slice(8));
            BitConverter.GetBytes(values[3]).CopyTo(data.Slice(12));

            WriteBytes(data);
        }
    }
}
