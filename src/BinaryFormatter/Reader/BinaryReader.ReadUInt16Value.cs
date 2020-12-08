﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    public ref partial struct BinaryReader
    {
        public ushort ReadUInt16Value()
        {
            if (ReadBytes(2, out ReadOnlySpan<byte> val))
            {
                return BitConverter.ToUInt16(val);
            }

            throw new InvalidOperationException();

        }
    }
}
