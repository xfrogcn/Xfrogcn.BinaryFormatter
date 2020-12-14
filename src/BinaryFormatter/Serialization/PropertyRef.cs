namespace Xfrogcn.BinaryFormatter
{
    internal readonly struct PropertyRef
    {
        public PropertyRef(ulong key, BinaryPropertyInfo info, byte[] nameFromBinary)
        {
            Key = key;
            Info = info;
            NameFromBinary = nameFromBinary;
        }

        public readonly ulong Key;
        public readonly BinaryPropertyInfo Info;

        // NameFromBinary may be different than Info.NameAsUtf8Bytes when case insensitive is enabled.
        public readonly byte[] NameFromBinary;
    }
}
