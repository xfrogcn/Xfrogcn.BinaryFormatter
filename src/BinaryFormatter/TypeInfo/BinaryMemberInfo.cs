namespace Xfrogcn.BinaryFormatter
{
    public struct BinaryMemberInfo
    {
        public bool IsField { get; set; }

        public ushort Seq { get; set; }

        public string Name { get; set; }

        public ushort TypeSeq { get; set; }
    }
}
