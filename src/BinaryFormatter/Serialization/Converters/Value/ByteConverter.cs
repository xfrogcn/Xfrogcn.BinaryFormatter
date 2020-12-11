using System;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class ByteConverter : BinaryConverter<byte>
    {
        public override int GetBytesCount(ref BinaryReader reader, BinarySerializerOptions options)
        {
            return BinarySerializerConstants.BytesCount_Byte;
        }

        public override byte Read(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options)
        {
            return reader.GetByte();
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            typeInfo.Type = TypeEnum.Byte;
            typeInfo.SerializeType = ClassType.Value;
        }

        public override void Write(BinaryWriter writer, byte value, BinarySerializerOptions options)
        {
            writer.WriteByteValue(value);
        }
    }
}
