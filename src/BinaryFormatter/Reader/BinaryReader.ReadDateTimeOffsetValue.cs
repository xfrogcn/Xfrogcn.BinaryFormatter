using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    public ref partial struct BinaryReader
    {
        
        public DateTimeOffset GetDateTimeOffset()
        {
            Debug.Assert(ValueSpan.Length == 16);

            return new DateTimeOffset(BitConverter.ToInt64(ValueSpan.Slice(8,8)), new TimeSpan(BitConverter.ToInt64(ValueSpan.Slice(0,8))));

        }
    }
}
