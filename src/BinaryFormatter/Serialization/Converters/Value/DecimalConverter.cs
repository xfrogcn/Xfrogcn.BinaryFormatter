using System;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class DecimalConverter : BinaryConverter<decimal>
    {
        public override int GetBytesCount(ref BinaryReader reader, BinarySerializerOptions options)
        {
            return BinarySerializerConstants.BytesCount_Decimal;
        }

        public override decimal Read(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options)
        {
            return reader.GetDecimal();
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            typeInfo.Type = TypeEnum.Decimal;
            typeInfo.SerializeType = ClassType.Value;
        }

        public override void Write(BinaryWriter writer, decimal value, BinarySerializerOptions options)
        {
            writer.WriteDecimalValue(value);
        }
    }
}
