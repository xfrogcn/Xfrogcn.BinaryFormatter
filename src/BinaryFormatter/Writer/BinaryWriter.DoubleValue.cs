using System;

namespace Xfrogcn.BinaryFormatter
{
    public sealed partial class BinaryWriter
    {
        public void WriteDoubleValue(double value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }
    }
}
