using System;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal class NullableConverter<T> : BinaryConverter<T?> where T : struct
    {
        private readonly BinaryConverter<T> _converter;

        public NullableConverter(BinaryConverter<T> converter)
        {
            _converter = converter;
          //  IsInternalConverterForNumberType = converter.IsInternalConverterForNumberType;
        }

        public override T? Read(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(BinaryWriter writer, T? value, BinarySerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
