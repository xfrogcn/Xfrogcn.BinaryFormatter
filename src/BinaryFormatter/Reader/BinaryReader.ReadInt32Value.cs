using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    public ref partial struct BinaryReader
    {
        

        public int GetInt32()
        {
            Debug.Assert(ValueSpan.Length == 4);

            return BitConverter.ToInt32(ValueSpan);
        }

       
    }
}
