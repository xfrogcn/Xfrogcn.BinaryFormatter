using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Serialization
{
    /// <summary>
    /// 转换器工厂，一个转换器支持多种类型
    /// </summary>
    public abstract class BinaryConverterFactory : BinaryConverter
    {
        /// <summary>
        /// When overidden, constructs a new <see cref="BinaryConverterFactory"/> instance.
        /// </summary>
        protected BinaryConverterFactory() { }

        internal sealed override ClassType ClassType
        {
            get
            {
                return ClassType.None;
            }
        }

        /// <summary>
        /// 创建实际的转换器
        /// </summary>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public abstract BinaryConverter CreateConverter(Type typeToConvert, BinarySerializerOptions options);

        internal override BinaryPropertyInfo CreateBinaryPropertyInfo()
        {
            Debug.Fail("We should never get here.");

            throw new InvalidOperationException();
        }

        internal override BinaryParameterInfo CreateBinaryParameterInfo()
        {
            Debug.Fail("We should never get here.");

            throw new InvalidOperationException();
        }

        internal sealed override Type ElementType => null;

        internal BinaryConverter GetConverterInternal(Type typeToConvert, BinarySerializerOptions options)
        {
            Debug.Assert(CanConvert(typeToConvert));

            BinaryConverter converter = CreateConverter(typeToConvert, options);
            if (converter == null)
            {
                ThrowHelper.ThrowInvalidOperationException_SerializerConverterFactoryReturnsNull(GetType());
            }

            return converter!;
        }

        internal sealed override object ReadCoreAsObject(
            ref BinaryReader reader,
            BinarySerializerOptions options,
            ref ReadStack state)
        {
            Debug.Fail("We should never get here.");

            throw new InvalidOperationException();
        }

        internal sealed override bool TryReadAsObject(
            ref BinaryReader reader,
            BinarySerializerOptions options,
            ref ReadStack state,
            out object value)
        {
            Debug.Fail("We should never get here.");

            throw new InvalidOperationException();
        }

        internal sealed override bool TryWriteAsObject(
            BinaryWriter writer,
            object value,
            BinarySerializerOptions options,
            ref WriteStack state)
        {
            Debug.Fail("We should never get here.");

            throw new InvalidOperationException();
        }

        internal sealed override Type TypeToConvert => null!;

        internal sealed override bool WriteCoreAsObject(
            BinaryWriter writer,
            object value,
            BinarySerializerOptions options,
            ref WriteStack state)
        {
            Debug.Fail("We should never get here.");

            throw new InvalidOperationException();
        }

        internal sealed override void WriteWithQuotesAsObject(
            BinaryWriter writer, object value,
            BinarySerializerOptions options,
            ref WriteStack state)
        {
            Debug.Fail("We should never get here.");

            throw new InvalidOperationException();
        }
    }
}
