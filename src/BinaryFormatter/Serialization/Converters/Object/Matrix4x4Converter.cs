using System;
using System.Numerics;
using System.Reflection;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class Matrix4x4Converter : LargeObjectWithParameterizedConstructorConverter<Matrix4x4>
    {
        private static readonly ConstructorInfo s_constructorInfo =
           typeof(Matrix4x4).GetConstructor(new[] 
           { 
               typeof(Single), typeof(Single), typeof(Single), typeof(Single),
               typeof(Single), typeof(Single), typeof(Single), typeof(Single),
               typeof(Single), typeof(Single), typeof(Single), typeof(Single),
               typeof(Single), typeof(Single), typeof(Single), typeof(Single)
           })!;

        public Matrix4x4Converter()
        {
            ConstructorInfo = s_constructorInfo;
        }

        internal override bool IncludeFields => true;

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(Matrix4x4);
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            typeInfo.Type = TypeEnum.Matrix4x4;
            typeInfo.SerializeType = ClassType.Object;
        }
    }
}
