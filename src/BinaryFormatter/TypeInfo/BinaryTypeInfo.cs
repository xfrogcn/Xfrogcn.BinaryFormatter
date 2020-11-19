using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    public struct BinaryTypeInfo
    {
        public ushort Seq { get; set; }

        public bool IsGeneric { get; set; }

        public sbyte GenericArgumentCount { get; set; }

        public TypeEnum Type { get; set; }
        public byte[] FullName { get; set; }

        public ushort[] GenericArguments { get; set; }

        public BinaryMemberInfo[] Members { get; set; }
    }
}
