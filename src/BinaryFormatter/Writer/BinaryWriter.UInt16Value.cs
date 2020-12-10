using System;

namespace Xfrogcn.BinaryFormatter
{
    public sealed partial class BinaryWriter
    {
        public void WriteUInt16Value(ushort value)
        {
            BitConverter.TryWriteBytes(TryGetWriteSpan(2), value);
        }
    }
}
