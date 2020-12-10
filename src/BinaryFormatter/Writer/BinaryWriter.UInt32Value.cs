using System;

namespace Xfrogcn.BinaryFormatter
{
    public sealed partial class BinaryWriter
    {
        public void WriteUInt32Value(uint value)
        {
            BitConverter.TryWriteBytes(TryGetWriteSpan(4), value);
        }
    }
}
