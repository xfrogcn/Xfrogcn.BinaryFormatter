using System;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class CharConverter : BinaryConverter<char>
    {
        public override int FixBytesCount => 2;

        public override char Read(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options)
        {
            return (char)reader.GetUInt16();
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap)
        {
            typeInfo.Type = TypeEnum.Char;
            typeInfo.SerializeType = ClassType.Value;
        }

        public override void Write(BinaryWriter writer, char value, BinarySerializerOptions options)
        {
            writer.WriteUInt16Value(value);
        }
    }
}
