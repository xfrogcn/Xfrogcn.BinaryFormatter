using System;

namespace Xfrogcn.BinaryFormatter.Serialization
{
    /// <summary>
    /// 指定序列化时所使用的类型转换器
    /// </summary>
    [AttributeUsage( AttributeTargets.Class| AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class BinaryConverterAttribute : BinaryAttribute
    {
        public BinaryConverterAttribute(Type converterType)
        {
            ConverterType = converterType;
        }


        protected BinaryConverterAttribute() { }

        public Type ConverterType { get; private set; }

        /// <summary>
        /// 由子类重置，允许子类自己创建BinaryConverter，只有在ConverterType属性为null时才有效
        /// </summary>
        /// <param name="typeToConvert">待处理的类型</param>
        /// <returns>类型转换器</returns>
        public virtual BinaryConverter CreateConverter(Type typeToConvert)
        {
            return null;
        }
    }
}
