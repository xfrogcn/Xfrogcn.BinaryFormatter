using System;

namespace Xfrogcn.BinaryFormatter
{
    public ref partial struct BinaryReader
    {
        public ulong GetEnumerableLength(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length == 1)
            {
                return bytes[0];
            }
            else if (bytes.Length == 2)
            {
                return BitConverter.ToUInt16(bytes);
            }
            else if (bytes.Length == 4)
            {
                return BitConverter.ToUInt32(bytes);
            }
            else if (bytes.Length == 8)
            {
                return BitConverter.ToUInt64(bytes);
            }
            else
            {
                ThrowHelper.ThrowBinaryException();
            }

            return 0;
        }
    }
}
