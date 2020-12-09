using System;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class SingleConverter : BinaryConverter<float>
    {
        public override int FixBytesCount => 4;

        public override float Read(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options)
        {
            return reader.GetSingle();
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap)
        {
            typeInfo.Type = TypeEnum.Single;
            typeInfo.SerializeType = ClassType.Value;
        }

        public override void Write(BinaryWriter writer, float value, BinarySerializerOptions options)
        {
            writer.WriteSingleValue(value);
        }
    }
}
