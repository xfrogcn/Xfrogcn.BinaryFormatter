using System;

namespace Xfrogcn.BinaryFormatter
{
    public sealed partial class BinaryWriter
    {
        public void WriteUInt32Value(uint value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }
    }
}
