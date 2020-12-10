using System;

namespace Xfrogcn.BinaryFormatter
{
    public sealed partial class BinaryWriter
    {
        public void WriteSingleValue(float value)
        {
            BitConverter.TryWriteBytes(TryGetWriteSpan(4), value);
        }
    }
}
