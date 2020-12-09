using System;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class DoubleConverter : BinaryConverter<double>
    {
        public override int FixBytesCount => 8;

        public override double Read(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options)
        {
            return reader.GetDouble();
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap)
        {
            typeInfo.Type = TypeEnum.Double;
            typeInfo.SerializeType = ClassType.Value;
        }

        public override void Write(BinaryWriter writer, double value, BinarySerializerOptions options)
        {
            writer.WriteDoubleValue(value);
        }
    }
}
