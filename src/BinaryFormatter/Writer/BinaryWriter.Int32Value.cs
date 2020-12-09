using System;

namespace Xfrogcn.BinaryFormatter
{
    public sealed partial class BinaryWriter
    {
        public void WriteInt32Value(int value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }
    }
}
