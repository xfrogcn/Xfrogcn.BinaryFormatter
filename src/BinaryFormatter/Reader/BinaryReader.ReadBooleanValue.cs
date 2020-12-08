﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    public ref partial struct BinaryReader
    {
        public bool ReadBooleanValue()
        {
            if (ReadBytes(1, out ReadOnlySpan<byte> val))
            {
                return val[0] == 0 ? false : true;
            }

            throw new InvalidOperationException();

        }
    }
}
