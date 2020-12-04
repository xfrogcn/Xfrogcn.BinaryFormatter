using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    public sealed partial class BinaryWriter 
    {
        public void WriteBooleanValue(bool value)
        {
            int maxRequired = 2 + 1; //类型索引 2字节+值一个字节
            if (_memory.Length - BytesPending < maxRequired)
            {
                Grow(maxRequired);
            }

            Span<byte> bytes = stackalloc byte[1];
            if (value)
            {
                bytes[0] = 1;
            }
            else
            {
                bytes[0] = 0;
            }
            WriteTypeSeqAndBytes(TypeEnum.Boolean, bytes);

        }
    }
}
