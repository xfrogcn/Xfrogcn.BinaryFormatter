using System;
using System.Numerics;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class DateTimeConverter : BinaryConverter<DateTime>
    {
        public override int GetBytesCount(ref BinaryReader reader, BinarySerializerOptions options)
        {
            return BinarySerializerConstants.BytesCount_DateTime;
        }

        public override DateTime Read(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options)
        {
            return reader.GetDateTime();
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            typeInfo.Type = TypeEnum.DateTime;
            typeInfo.SerializeType = ClassType.Value;
        }

        public override void Write(BinaryWriter writer, DateTime value, BinarySerializerOptions options)
        {
            writer.WriteDateTimeValue(value);
        }
    }
}
