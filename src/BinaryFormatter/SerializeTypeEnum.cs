using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    public enum SerializeTypeEnum : sbyte
    {
        Null = 0,
        SingleValue = 1,
        KeyValuePair = 2,
        List = 3
    }
}
