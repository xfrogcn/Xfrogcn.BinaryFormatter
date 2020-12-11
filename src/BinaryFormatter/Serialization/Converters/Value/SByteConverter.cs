using System;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class SByteConverter : BinaryConverter<sbyte>
    {
        public override int GetBytesCount(ref BinaryReader reader, BinarySerializerOptions options)
        {
            return BinarySerializerConstants.BytesCount_SByte;
        }

        public override sbyte Read(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options)
        {
            return reader.GetSByte();
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            typeInfo.Type = TypeEnum.SByte;
            typeInfo.SerializeType = ClassType.Value;
        }

        public override void Write(BinaryWriter writer, sbyte value, BinarySerializerOptions options)
        {
            writer.WriteSByteValue(value);
        }
    }
}
