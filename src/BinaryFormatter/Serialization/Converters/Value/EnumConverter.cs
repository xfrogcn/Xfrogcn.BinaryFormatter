using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal class EnumConverter<T> : BinaryConverter<T>
        where T : struct, Enum
    {
        private static readonly TypeCode s_enumTypeCode = Type.GetTypeCode(typeof(T));

        // Odd type codes are conveniently signed types (for enum backing types).
        private static readonly string? s_negativeSign = ((int)s_enumTypeCode % 2) == 0 ? null : NumberFormatInfo.CurrentInfo.NegativeSign;

        public override T Read(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap)
        {
            throw new NotImplementedException();
        }

        public override void Write(BinaryWriter writer, T value, BinarySerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
