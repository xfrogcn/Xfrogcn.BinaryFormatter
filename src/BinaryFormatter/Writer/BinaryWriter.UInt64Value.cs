using System;

namespace Xfrogcn.BinaryFormatter
{
    public sealed partial class BinaryWriter
    {
        public void WriteUInt64Value(ulong value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }
    }
}
