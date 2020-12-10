using System;

namespace Xfrogcn.BinaryFormatter
{
    public sealed partial class BinaryWriter
    {
        public void WriteInt16Value(short value)
        {
            BitConverter.TryWriteBytes(TryGetWriteSpan(2), value);
        }
    }
}
