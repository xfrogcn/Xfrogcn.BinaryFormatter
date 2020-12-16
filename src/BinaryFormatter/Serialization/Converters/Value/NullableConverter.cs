using System;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal class NullableConverter<T> : BinaryConverter<T?> where T : struct
    {
        private readonly BinaryConverter<T> _converter;

        public override int GetBytesCount(ref BinaryReader reader, BinarySerializerOptions options)
        {
            return _converter.GetBytesCount(ref reader, options);
        }

        public NullableConverter(BinaryConverter<T> converter)
        {
            _converter = converter;
        }

        public override T? Read(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options)
        {
            if (reader.TokenType == BinaryTokenType.Null)
            {
                return null;
            }
            if (!reader.ReadTypeSeq())
            {
                return null;
            }
            T value = _converter.Read(ref reader, typeof(T), options);
            return value;
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            _converter.SetTypeMetadata(typeInfo, typeMap, options);
            typeInfo.FullName = null;
            typeInfo.Type = TypeEnum.Nullable;
        }

        internal override bool OnTryWrite(BinaryWriter writer, T? value, BinarySerializerOptions options, ref WriteStack state)
        {
            if (!value.HasValue)
            {
                writer.WriteNullValue();
                return true;
            }
            else
            {
                return _converter.TryWrite(writer, value.Value, options, ref state);
            }
        }

        public override void Write(BinaryWriter writer, T? value, BinarySerializerOptions options)
        {
            ThrowHelper.ThrowBinaryException();
            
        }
    }
}
