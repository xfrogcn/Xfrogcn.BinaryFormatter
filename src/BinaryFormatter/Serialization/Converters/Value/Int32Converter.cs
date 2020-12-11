using System;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class Int32Converter : BinaryConverter<int>
    {
        public override int GetBytesCount(ref BinaryReader reader, BinarySerializerOptions options)
        {
            return BinarySerializerConstants.BytesCount_Int32;
        }

        public override int Read(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options)
        {
            return reader.GetInt32();
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            typeInfo.Type = TypeEnum.Int32;
            typeInfo.SerializeType = ClassType.Value;
        }

        public override void Write(BinaryWriter writer, int value, BinarySerializerOptions options)
        {
            writer.WriteInt32Value(value);
        }
    }
}
