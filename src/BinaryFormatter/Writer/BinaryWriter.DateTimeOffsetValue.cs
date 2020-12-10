using System;

namespace Xfrogcn.BinaryFormatter
{
    public sealed partial class BinaryWriter
    {
        public void WriteDateTimeOffsetValue(DateTimeOffset value)
        {
            Span<byte> output = stackalloc byte[16];
            
            BitConverter.TryWriteBytes(output, value.Offset.Ticks);
            BitConverter.TryWriteBytes(output.Slice(8, 8), value.Ticks);

            WriteBytes(output);
        }
    }
}
