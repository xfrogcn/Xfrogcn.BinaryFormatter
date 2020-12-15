using System;
using System.Numerics;
using System.Reflection;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class QuaternionConverter :
        SmallObjectWithParameterizedConstructorConverter<Quaternion, float, float, float, float>
    {
        private static readonly ConstructorInfo s_constructorInfo =
            typeof(Quaternion).GetConstructor(new[] { typeof(Single), typeof(Single), typeof(Single), typeof(Single) })!;

        internal override bool IncludeFields => true;

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(Quaternion);
        }

        public QuaternionConverter()
        {
            ConstructorInfo = s_constructorInfo;
        }



        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            typeInfo.Type = TypeEnum.Quaternion;
            typeInfo.SerializeType = ClassType.Object;
        }
    }
}
