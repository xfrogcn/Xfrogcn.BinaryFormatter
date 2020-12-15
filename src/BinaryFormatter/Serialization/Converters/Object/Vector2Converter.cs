using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class Vector2Converter :
        SmallObjectWithParameterizedConstructorConverter<Vector2, float, float, object, object>
    {
        private static readonly ConstructorInfo s_constructorInfo =
            typeof(Vector2).GetConstructor(new[] { typeof(Single), typeof(Single) })!;

        internal override bool IncludeFields => true;

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(Vector2);
        }

        public Vector2Converter()
        {
            ConstructorInfo = s_constructorInfo;
        }



        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            typeInfo.Type = TypeEnum.Vector2;
            typeInfo.SerializeType = ClassType.Object;
        }
    }
}
