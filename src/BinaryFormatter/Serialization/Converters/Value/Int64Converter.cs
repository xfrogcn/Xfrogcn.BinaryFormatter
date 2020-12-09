﻿using System;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class Int64Converter : BinaryConverter<long>
    {
        public override int FixBytesCount => 8;

        public override long Read(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options)
        {
            return reader.GetInt64();
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap)
        {
            typeInfo.Type = TypeEnum.Int64;
            typeInfo.SerializeType = ClassType.Value;
        }

        public override void Write(BinaryWriter writer, long value, BinarySerializerOptions options)
        {
            writer.WriteInt64Value(value);
        }
    }
}
