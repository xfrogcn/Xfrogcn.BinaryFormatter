using System;

namespace Xfrogcn.BinaryFormatter
{
    public sealed partial class BinaryWriter
    {
        public void WriteSingleValue(float value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }
    }
}
