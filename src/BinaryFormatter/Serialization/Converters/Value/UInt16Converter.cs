using System;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class UInt16Converter : BinaryConverter<ushort>
    {
        public override int GetBytesCount(ref BinaryReader reader, BinarySerializerOptions options)
        {
            return BinarySerializerConstants.BytesCount_UInt16;
        }

        public override ushort Read(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options)
        {
            return reader.GetUInt16();
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            typeInfo.Type = TypeEnum.UInt16;
            typeInfo.SerializeType = ClassType.Value;
        }

        public override void Write(BinaryWriter writer, ushort value, BinarySerializerOptions options)
        {
            writer.WriteUInt16Value(value);
        }
    }
}
