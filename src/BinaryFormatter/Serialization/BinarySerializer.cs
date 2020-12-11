using Xfrogcn.BinaryFormatter.Serialization;

namespace Xfrogcn.BinaryFormatter
{
    public static partial class BinarySerializer
    {
        internal static bool WriteReferenceForObject(
            BinaryConverter binaryConverter,
            object currentValue,
            ref WriteStack state,
            BinaryWriter writer)
        {
            ulong offset = (ulong)(writer.BytesCommitted + writer.BytesPending);
            uint seq = state.ReferenceResolver.GetReference(currentValue, offset, out bool alreadyExists);
            if(alreadyExists)
            {
                // 写引用及引用序号
                writer.WriteByteValue(0xFF);
                writer.WriteUInt32Value(seq);
                return true;
            }
            else
            {
                // 标记为非引用
                writer.WriteByteValue(0x00);
                return false;
            }
            
        }
    }
}
