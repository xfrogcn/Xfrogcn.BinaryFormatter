using System;
using System.Numerics;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class TimeSpanConverter : BinaryConverter<TimeSpan>
    {
        public override int GetBytesCount(ref BinaryReader reader, BinarySerializerOptions options)
        {
            return BinarySerializerConstants.BytesCount_TimeSpan;
        }

        public override TimeSpan Read(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options)
        {
            return reader.GetTimeSpan();
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            typeInfo.Type = TypeEnum.TimeSpan;
            typeInfo.SerializeType = ClassType.Value;
        }

        public override void Write(BinaryWriter writer, TimeSpan value, BinarySerializerOptions options)
        {
            writer.WriteTimeSpanValue(value);
        }
    }
}
