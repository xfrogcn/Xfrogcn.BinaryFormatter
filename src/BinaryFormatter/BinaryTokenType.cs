namespace Xfrogcn.BinaryFormatter
{
    public enum BinaryTokenType : byte
    {
        None,
        StartObject,
        EndObject,
        StartArray,
        EndArray,
        PropertyName,
        TypeSeq,
        Null
    }
}
