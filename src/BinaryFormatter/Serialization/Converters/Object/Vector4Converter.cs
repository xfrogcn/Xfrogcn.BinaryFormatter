using System;
using System.Numerics;
using System.Reflection;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class Vector4Converter :
        SmallObjectWithParameterizedConstructorConverter<Vector4, float, float, float, float>
    {
        private static readonly ConstructorInfo s_constructorInfo =
            typeof(Vector4).GetConstructor(new[] { typeof(Single), typeof(Single), typeof(Single), typeof(Single) })!;

        internal override bool IncludeFields => true;

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(Vector4);
        }

        public Vector4Converter()
        {
            ConstructorInfo = s_constructorInfo;
        }



        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            typeInfo.Type = TypeEnum.Vector4;
            typeInfo.SerializeType = ClassType.Object;
        }
    }
}
