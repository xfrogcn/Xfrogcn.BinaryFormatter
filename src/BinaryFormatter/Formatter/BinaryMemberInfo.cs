namespace Xfrogcn.BinaryFormatter
{
    public class BinaryMemberInfo
    {
        public bool IsField { get; set; }

        public ushort Seq { get; set; }

        public string Name { get; set; }

        public ushort TypeSeq { get; set; }
    }
}
