using System;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class UInt64Converter : BinaryConverter<ulong>
    {
        public override int FixBytesCount => 8;

        public override ulong Read(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options)
        {
            return reader.GetUInt64();
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap)
        {
            typeInfo.Type = TypeEnum.UInt64;
            typeInfo.SerializeType = ClassType.Value;
        }

        public override void Write(BinaryWriter writer, ulong value, BinarySerializerOptions options)
        {
            writer.WriteUInt64Value(value);
        }
    }
}
