namespace Xfrogcn.BinaryFormatter
{
    internal readonly struct ParameterRef
    {
        public ParameterRef(ulong key, BinaryParameterInfo info, byte[] nameFromBinary)
        {
            Key = key;
            Info = info;
            NameFromBinary = nameFromBinary;
        }

        public readonly ulong Key;

        public readonly BinaryParameterInfo Info;

        // NameFromBinary may be different than Info.NameAsUtf8Bytes when case insensitive is enabled.
        public readonly byte[] NameFromBinary;
    }
}
