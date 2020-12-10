using System;

namespace Xfrogcn.BinaryFormatter
{
    public sealed partial class BinaryWriter
    {
        public void WriteDateTimeValue(DateTime value)
        {
            Span<byte> output = stackalloc byte[9];
            output[0] = (byte)value.Kind;

            BitConverter.TryWriteBytes(output.Slice(1), value.Ticks);

            WriteBytes(output);
        }
    }
}
