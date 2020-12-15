using System;
using System.Numerics;
using System.Reflection;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class PlaneConverter :
        SmallObjectWithParameterizedConstructorConverter<Plane, Vector3, float, object, object>
    {
        private static readonly ConstructorInfo s_constructorInfo =
            typeof(Plane).GetConstructor(new[] { typeof(Vector3), typeof(Single) })!;

        internal override bool IncludeFields => true;

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(Plane);
        }

        public PlaneConverter()
        {
            ConstructorInfo = s_constructorInfo;
        }



        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            typeInfo.Type = TypeEnum.Plane;
            typeInfo.SerializeType = ClassType.Object;
        }
    }
}
