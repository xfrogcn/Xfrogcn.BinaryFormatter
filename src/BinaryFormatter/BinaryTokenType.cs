namespace Xfrogcn.BinaryFormatter
{
    public enum BinaryTokenType : byte
    {
        None,
        StartObject,
        ObjectRef,
        EndObject,
        StartArray,
        EndArray,
        PropertyName,
        TypeSeq,
        DictionaryKeySeq,
        EndDictionaryKey,
        Bytes,
        Null
    }
}
