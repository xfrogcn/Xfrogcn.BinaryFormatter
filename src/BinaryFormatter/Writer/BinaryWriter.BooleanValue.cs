using System;

namespace Xfrogcn.BinaryFormatter
{
    public sealed partial class BinaryWriter 
    {
        public void WriteBooleanValue(bool value)
        {
            WriteByteValue(value ? 1 : 0);
        }
    }
}
