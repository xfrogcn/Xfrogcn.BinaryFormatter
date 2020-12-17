using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    public sealed partial class BinaryWriter
    {
        public void WriteBytesValue(ReadOnlySpan<byte> value)
        {
            int len = value.Length;
            if(len > BinarySerializerConstants.EndObjectSeq)
            {
                ReadOnlySpan<byte> lenBytes = BitConverter.GetBytes(len);
                Span<byte> bytes = stackalloc byte[4];
                bytes[0] = (byte)(len>>24 | (byte)0x80);
                bytes[1] = (byte)(len >> 16 & 0xFF);
                bytes[2] = (byte)(len >> 8 & 0xFF);
                bytes[3] = (byte)(len & 0xFF);

                WriteBytes(bytes);
            }
            else
            {
                Span<byte> bytes = stackalloc byte[2];
                bytes[0] = (byte)(len >> 8);
                bytes[1] = (byte)(len & 0xFF);
                WriteBytes(bytes);
            }

            if (len > 0)
            {
                WriteBytes(value);
            }
        }
    }
}
