using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class EnumConverterFactory : BinaryConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsEnum;
        }

        public override BinaryConverter CreateConverter(Type typeToConvert, BinarySerializerOptions options)
        {
            BinaryConverter converter = (BinaryConverter)Activator.CreateInstance(
                typeof(EnumConverter<>).MakeGenericType(typeToConvert),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                new object[] {  },
                culture: null)!;

            return converter;
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            typeInfo.Type = TypeEnum.Enum;
        }
    }
}
