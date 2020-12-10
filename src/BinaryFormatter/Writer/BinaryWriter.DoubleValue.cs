using System;

namespace Xfrogcn.BinaryFormatter
{
    public sealed partial class BinaryWriter
    {
        public void WriteDoubleValue(double value)
        {
            BitConverter.TryWriteBytes(TryGetWriteSpan(8), value);
        }
    }
}
