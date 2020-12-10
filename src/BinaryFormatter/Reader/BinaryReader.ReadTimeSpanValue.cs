using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    public ref partial struct BinaryReader
    {
        
        public TimeSpan GetTimeSpan()
        {
            Debug.Assert(ValueSpan.Length == 8);

            return new TimeSpan(BitConverter.ToInt64(ValueSpan));

        }
    }
}
