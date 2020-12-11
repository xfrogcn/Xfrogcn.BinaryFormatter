using System;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class UriConverter : BinaryConverter<Uri>
    {
        public override Uri Read(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options)
        {
            return new Uri(reader.GetString());
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            typeInfo.Type = TypeEnum.Uri;
            typeInfo.SerializeType = ClassType.Value;
        }

        public override void Write(BinaryWriter writer, Uri value, BinarySerializerOptions options)
        {
            if (value != null)
            {
                writer.WriteStringValue(BinaryReaderHelper.s_utf8Encoding.GetBytes(value.ToString()));
            }
        }
    }
}
