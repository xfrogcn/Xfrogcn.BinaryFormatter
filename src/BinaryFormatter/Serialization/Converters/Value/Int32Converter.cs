using System;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class Int32Converter : BinaryConverter<int>
    {
        public override int FixBytesCount => 4;

        public override int Read(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options)
        {
            return reader.GetInt32();
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap)
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
