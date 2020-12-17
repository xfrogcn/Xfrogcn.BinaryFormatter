using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal class VectorOfTConverterFactory : BinaryConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Vector<>);
        }

        public override BinaryConverter CreateConverter(Type typeToConvert, BinarySerializerOptions options)
        {
            Debug.Assert(typeToConvert.GetGenericArguments().Length > 0);

            Type elementType = typeToConvert.GetGenericArguments()[0];

            return (BinaryConverter)Activator.CreateInstance(
                 typeof(VectorOfTConverter<>).MakeGenericType(elementType),
                 BindingFlags.Instance | BindingFlags.Public,
                 binder: null,
                 args: new object[] { },
                 culture: null)!;
        }

       
        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
           
        }
    }
}
