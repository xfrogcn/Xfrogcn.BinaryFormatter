using System;
using System.Numerics;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class GuidConverter : BinaryConverter<Guid>
    {
        public override int FixBytesCount => 16;

        public override Guid Read(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options)
        {
            return reader.GetGuid();
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap)
        {
            typeInfo.Type = TypeEnum.Guid;
            typeInfo.SerializeType = ClassType.Value;
        }

        public override void Write(BinaryWriter writer, Guid value, BinarySerializerOptions options)
        {
            writer.WriteGuidValue(value);
        }
    }
}
