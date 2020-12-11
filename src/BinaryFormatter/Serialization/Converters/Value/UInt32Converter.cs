using System;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class UInt32Converter : BinaryConverter<uint>
    {
        public override int GetBytesCount(ref BinaryReader reader, BinarySerializerOptions options)
        {
            return BinarySerializerConstants.BytesCount_UInt32;
        }

        public override uint Read(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options)
        {
            return reader.GetUInt32();
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            typeInfo.Type = TypeEnum.UInt32;
            typeInfo.SerializeType = ClassType.Value;
        }

        public override void Write(BinaryWriter writer, uint value, BinarySerializerOptions options)
        {
            writer.WriteUInt32Value(value);
        }
    }
}
