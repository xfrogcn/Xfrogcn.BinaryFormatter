using System;

namespace Xfrogcn.BinaryFormatter
{
    public sealed partial class BinaryWriter
    {
        public void WriteUInt64Value(ulong value)
        {
            BitConverter.TryWriteBytes(TryGetWriteSpan(8), value);
        }
    }
}
