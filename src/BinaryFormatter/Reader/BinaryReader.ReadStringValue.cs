using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    public ref partial struct BinaryReader
    {
        public string ReadStringValue(int bytes)
        {
            if (ReadBytes(bytes, out ReadOnlySpan<byte> val))
            {
                return Encoding.UTF8.GetString(val);
            }

            throw new InvalidOperationException();

        }
    }
}
