using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    public ref partial struct BinaryReader
    {
        public sbyte GetSByte()
        {
            if(ValueSpan.Length != 1)
            {
                throw new Exception();
            }

            return unchecked((sbyte)ValueSpan[0]);
        }
    }
}
