using System;
using System.Numerics;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class ComplexConverter : BinaryConverter<Complex>
    {
        public override int FixBytesCount => 0;

        public override Complex Read(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options)
        {
            return reader.GetComplex();
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap)
        {
            typeInfo.Type = TypeEnum.Complex;
            typeInfo.SerializeType = ClassType.Value;
        }

        public override void Write(BinaryWriter writer, Complex value, BinarySerializerOptions options)
        {
            writer.WriteComplexValue(value);
        }
    }
}
