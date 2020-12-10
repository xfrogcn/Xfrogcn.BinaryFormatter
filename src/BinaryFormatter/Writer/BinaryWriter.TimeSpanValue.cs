using System;

namespace Xfrogcn.BinaryFormatter
{
    public sealed partial class BinaryWriter
    {
        public void WriteTimeSpanValue(TimeSpan value)
        {
            BitConverter.TryWriteBytes(TryGetWriteSpan(8), value.Ticks);
        }
    }
}
