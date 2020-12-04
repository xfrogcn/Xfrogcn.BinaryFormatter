﻿using System;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class BooleanConverter : BinaryConverter<bool>
    {
        public override bool Read(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap)
        {
            typeInfo.Type = TypeEnum.Boolean;
        }

        public override void Write(BinaryWriter writer, bool value, BinarySerializerOptions options)
        {
            writer.WriteBooleanValue(value);
        }
    }
}