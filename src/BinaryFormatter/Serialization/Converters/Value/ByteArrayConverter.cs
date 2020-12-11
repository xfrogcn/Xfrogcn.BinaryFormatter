using System;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class ByteArrayConverter : BinaryConverter<byte[]>
    {

        public override byte[] Read(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options)
        {
            return reader.ValueSpan.ToArray();
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            typeInfo.Type = TypeEnum.ByteArray;
            typeInfo.SerializeType = ClassType.Value;
        }

        public override void Write(BinaryWriter writer, byte[] value, BinarySerializerOptions options)
        {
            writer.WriteBytesValue(value);
        }
    }
}
