using System;

namespace Xfrogcn.BinaryFormatter
{
    public static partial class BinarySerializer
    {
        /// <summary>
        /// 直接序列化，并返回结果byte数组
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static byte[] Serialize<TValue>(
            TValue value,
            BinarySerializerOptions options = null)
        {
            return WriteCoreBytes<TValue>(value, typeof(TValue), options);
        }

        private static byte[] WriteCoreBytes<TValue>(in TValue value, Type inputType, BinarySerializerOptions options)
        {
            if (options == null)
            {
                options = BinarySerializerOptions.s_defaultOptions;
            }

            using var output = new PooledByteBufferWriter(options.DefaultBufferSize);
            using (var writer = new BinaryWriter(output, options))
            {
                // 写入头
                writer.WriteHeader();
                if (value == null)
                {
                    writer.Flush();
                    return output.WrittenMemory.ToArray();
                }
                if (value != null)
                {
                    inputType = value!.GetType();
                }

                WriteStack state = default;
                WriteCore<TValue>(writer, value, inputType, ref state, options);

                writer.WriteMetadata(ref state, inputType);
                writer.Flush();

            }

            return output.WrittenMemory.ToArray();
        }
    }
}
