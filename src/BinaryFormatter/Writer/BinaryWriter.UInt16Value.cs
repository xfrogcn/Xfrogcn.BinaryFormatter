using System;

namespace Xfrogcn.BinaryFormatter
{
    public sealed partial class BinaryWriter
    {
        public void WriteUInt16Value(ushort value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }
    }
}
