namespace Xfrogcn.BinaryFormatter
{
    internal enum StackFrameWriteObjectState : byte
    {
        None,
        WriteStartToken,
        WriteElements,
        WriteProperties,
        WriteEndToken
    }
}
