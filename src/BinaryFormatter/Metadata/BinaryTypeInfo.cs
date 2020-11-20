using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    public class BinaryTypeInfo
    {
        public ushort Seq { get; internal set; }

        public SerializeTypeEnum SerializeType { get; set; }

        public bool IsGeneric { get; set; }

        public sbyte GenericArgumentCount { get; set; }

        public TypeEnum Type { get; set; }
        public string FullName { get; set; }

        public ushort[] GenericArguments { get; set; }

        public BinaryMemberInfo[] Members { get; set; }
    }
}
