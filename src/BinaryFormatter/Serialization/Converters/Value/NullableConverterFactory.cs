using System;
using System.Diagnostics;
using System.Reflection;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    /// <summary>
    /// 可空类型转换器
    /// </summary>
    internal class NullableConverterFactory : BinaryConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return Nullable.GetUnderlyingType(typeToConvert) != null;
        }

        public override BinaryConverter CreateConverter(Type typeToConvert, BinarySerializerOptions options)
        {
            Debug.Assert(typeToConvert.GetGenericArguments().Length > 0);

            Type valueTypeToConvert = typeToConvert.GetGenericArguments()[0];

            BinaryConverter valueConverter = options.GetConverter(valueTypeToConvert);
            Debug.Assert(valueConverter != null);

            // If the value type has an interface or object converter, just return that converter directly.
            if (!valueConverter.TypeToConvert.IsValueType && valueTypeToConvert.IsValueType)
            {
                return valueConverter;
            }

            return CreateValueConverter(valueTypeToConvert, valueConverter);
        }

        public static BinaryConverter CreateValueConverter(Type valueTypeToConvert, BinaryConverter valueConverter)
        {
            return (BinaryConverter)Activator.CreateInstance(
                typeof(NullableConverter<>).MakeGenericType(valueTypeToConvert),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { valueConverter },
                culture: null)!;
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            typeInfo.Type = TypeEnum.Nullable;
        }
    }
}
