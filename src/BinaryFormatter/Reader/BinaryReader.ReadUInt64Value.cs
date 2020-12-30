using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    public ref partial struct BinaryReader
    {
        public ulong ReadUInt64Value()
        {
            if (ReadBytes(8, out ReadOnlySpan<byte> val))
            {
                return BitConverter.ToUInt64(val);
            }

            throw new InvalidOperationException();

        }

        public ulong GetUInt64()
        {
            Debug.Assert(ValueSpan.Length == 8);

            return BitConverter.ToUInt64(ValueSpan);
        }
    }
}
