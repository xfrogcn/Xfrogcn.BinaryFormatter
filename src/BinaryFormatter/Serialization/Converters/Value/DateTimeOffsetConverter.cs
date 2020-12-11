using System;
using System.Numerics;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class DateTimeOffsetConverter : BinaryConverter<DateTimeOffset>
    {
        public override int GetBytesCount(ref BinaryReader reader, BinarySerializerOptions options)
        {
            return BinarySerializerConstants.BytesCount_DateTimeOffset;
        }

        public override DateTimeOffset Read(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options)
        {
            return reader.GetDateTimeOffset();
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            typeInfo.Type = TypeEnum.DateTimeOffset;
            typeInfo.SerializeType = ClassType.Value;
        }

        public override void Write(BinaryWriter writer, DateTimeOffset value, BinarySerializerOptions options)
        {
            writer.WriteDateTimeOffsetValue(value);
        }
    }
}
