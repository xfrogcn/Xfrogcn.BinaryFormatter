using System;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class StringConverter : BinaryConverter<string>
    {
        public override string Read(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options)
        {
            return reader.GetString();
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            typeInfo.Type = TypeEnum.String;
            typeInfo.SerializeType = ClassType.Value;
        }

        public override void Write(BinaryWriter writer, string value, BinarySerializerOptions options)
        {
            if (value != null)
            {
                writer.WriteStringValue(BinaryReaderHelper.s_utf8Encoding.GetBytes(value));
            }
        }
    }
}
