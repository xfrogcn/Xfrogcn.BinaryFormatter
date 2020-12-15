using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class Vector3Converter :
        SmallObjectWithParameterizedConstructorConverter<Vector3, float, float, float, object>
    {
        private static readonly ConstructorInfo s_constructorInfo =
            typeof(Vector3).GetConstructor(new[] { typeof(Single), typeof(Single), typeof(Single) })!;

        internal override bool IncludeFields => true;

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(Vector3);
        }

        public Vector3Converter()
        {
            ConstructorInfo = s_constructorInfo;
        }



        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            typeInfo.Type = TypeEnum.Vector3;
            typeInfo.SerializeType = ClassType.Object;
        }
    }
}
