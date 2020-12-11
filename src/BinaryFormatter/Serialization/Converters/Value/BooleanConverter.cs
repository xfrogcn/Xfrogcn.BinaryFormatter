using System;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class BooleanConverter : BinaryConverter<bool>
    {
        public override int GetBytesCount(ref BinaryReader reader, BinarySerializerOptions options)
        {
            return BinarySerializerConstants.BytesCount_Boolean;
        }

        public override bool Read(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options)
        {
            return reader.GetBoolean();
        }



        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            typeInfo.Type = TypeEnum.Boolean;
            typeInfo.SerializeType = ClassType.Value;
        }

        public override void Write(BinaryWriter writer, bool value, BinarySerializerOptions options)
        {
            writer.WriteBooleanValue(value);
        }
    }
}
