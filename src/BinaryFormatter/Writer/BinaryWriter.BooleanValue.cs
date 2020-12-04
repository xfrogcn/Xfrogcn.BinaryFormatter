using System;

namespace Xfrogcn.BinaryFormatter
{
    public sealed partial class BinaryWriter 
    {
        public void WriteBooleanValue(bool value)
        {
            Span<byte> bytes = stackalloc byte[1];
            if (value)
            {
                bytes[0] = 1;
            }
            else
            {
                bytes[0] = 0;
            }
            WriteBytes(bytes);

        }
    }
}
