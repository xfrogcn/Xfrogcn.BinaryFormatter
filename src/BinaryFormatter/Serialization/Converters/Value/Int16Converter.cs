﻿using System;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class Int16Converter : BinaryConverter<short>
    {
        public override int FixBytesCount => 2;

        public override short Read(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options)
        {
            return reader.GetInt16();
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap)
        {
            typeInfo.Type = TypeEnum.Int16;
            typeInfo.SerializeType = ClassType.Value;
        }

        public override void Write(BinaryWriter writer, short value, BinarySerializerOptions options)
        {
            writer.WriteInt16Value(value);
        }
    }
}
