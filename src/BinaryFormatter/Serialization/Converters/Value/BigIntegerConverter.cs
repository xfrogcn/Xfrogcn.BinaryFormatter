using System;
using System.Numerics;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class BigIntegerConverter : BinaryConverter<BigInteger>
    {

        public override BigInteger Read(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options)
        {
            return reader.GetBigInteger();
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            typeInfo.Type = TypeEnum.BigInteger;
            typeInfo.SerializeType = ClassType.Value;
        }

        public override void Write(BinaryWriter writer, BigInteger value, BinarySerializerOptions options)
        {
            writer.WriteBigIntegerValue(value);
        }
    }
}
