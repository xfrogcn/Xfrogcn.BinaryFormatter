using System;
using System.Numerics;
using System.Reflection;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class Matrix3x2Converter : LargeObjectWithParameterizedConstructorConverter<Matrix3x2>
    {
        private static readonly ConstructorInfo s_constructorInfo =
           typeof(Matrix3x2).GetConstructor(new[] { typeof(Single), typeof(Single), typeof(Single), typeof(Single), typeof(Single), typeof(Single) })!;

        public Matrix3x2Converter()
        {
            ConstructorInfo = s_constructorInfo;
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(Matrix3x2);
        }

        internal override bool IncludeFields => true;

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            typeInfo.Type = TypeEnum.Matrix3x2;
            typeInfo.SerializeType = ClassType.Object;
        }
    }
}
