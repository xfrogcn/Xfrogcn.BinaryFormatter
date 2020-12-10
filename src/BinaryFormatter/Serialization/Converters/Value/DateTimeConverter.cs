using System;
using System.Numerics;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class DateTimeConverter : BinaryConverter<DateTime>
    {
        public override int FixBytesCount => 9;

        public override DateTime Read(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options)
        {
            return reader.GetDateTime();
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap)
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
