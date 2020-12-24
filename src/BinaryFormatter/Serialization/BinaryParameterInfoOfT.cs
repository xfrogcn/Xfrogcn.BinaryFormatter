using System;
using System.Reflection;

namespace Xfrogcn.BinaryFormatter.Serialization
{
    /// <summary>
    /// Represents a strongly-typed parameter to prevent boxing where have less than 4 parameters.
    /// Holds relevant state like the default value of the parameter, and the position in the method's parameter list.
    /// </summary>
    internal class BinaryParameterInfo<T> : BinaryParameterInfo
    {
        public T TypedDefaultValue { get; private set; } = default!;

        public override void Initialize(
            TypeMap typeMap,
            Type runtimePropertyType,
            ParameterInfo parameterInfo,
            BinaryPropertyInfo matchingProperty,
            BinarySerializerOptions options)
        {
            base.Initialize(
                typeMap,
                runtimePropertyType,
                parameterInfo,
                matchingProperty,
                options);

            if (parameterInfo!=null && parameterInfo.HasDefaultValue)
            {
                DefaultValue = parameterInfo.DefaultValue;
                TypedDefaultValue = (T)parameterInfo.DefaultValue!;
            }
            else
            {
                DefaultValue = TypedDefaultValue;
            }
        }
    }
}
