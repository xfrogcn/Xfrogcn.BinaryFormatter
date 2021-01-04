using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    public ref partial struct BinaryReader
    {
        
        public DateTime GetDateTime()
        {
            Debug.Assert(ValueSpan.Length == 9);

            return new DateTime(BitConverter.ToInt64(ValueSpan[1..]), (DateTimeKind)ValueSpan[0]);

        }
    }
}
