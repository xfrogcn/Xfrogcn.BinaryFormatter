namespace Xfrogcn.BinaryFormatter
{
    public class BinaryTypeInfo
    {
        internal ushort Seq { get;  set; }

        internal ClassType SerializeType { get; set; }

        internal bool IsGeneric { get; set; }

        internal sbyte GenericArgumentCount { get; set; }

        public TypeEnum Type { get; set; }
        public string FullName { get; set; }

        internal ushort[] GenericArguments { get; set; }

        //public BinaryMemberInfo[] Members { get; set; }
    }
}
