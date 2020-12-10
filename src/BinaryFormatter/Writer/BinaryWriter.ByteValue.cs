using System;

namespace Xfrogcn.BinaryFormatter
{
    public sealed partial class BinaryWriter
    {
        public void WriteByteValue(byte value)
        {
            if (_memory.Length - BytesPending < 1)
            {
                Grow(1);
            }
            
            Span<byte> output = _memory.Span;
            output[BytesPending++] = value;
        }
    }
}
