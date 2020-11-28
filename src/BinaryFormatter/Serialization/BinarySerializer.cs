using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xfrogcn.BinaryFormatter.Serialization
{
    public static partial class BinarySerializer
    {
        public static Task SerializeAsync<TValue>(
            Stream stream,
            TValue value,
            BinarySerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return WriteAsyncCore(stream, value, typeof(TValue), options, cancellationToken);
        }

        public static Task SerializeAsync(
            Stream stream,
            object value,
            Type inputType,
            BinarySerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (inputType == null)
            {
                throw new ArgumentNullException(nameof(inputType));
            }

            if (value != null && !inputType.IsAssignableFrom(value.GetType()))
            {
                throw new InvalidOperationException("错误的序列化类型");
            }

            return WriteAsyncCore<object>(stream, value!, inputType, options, cancellationToken);
        }

        private static async Task WriteAsyncCore<TValue>(
            Stream stream,
            TValue value,
            Type inputType,
            BinarySerializerOptions? options,
            CancellationToken cancellationToken)
        {
            if( options == null)
            {
                options = BinarySerializerOptions.s_defaultOptions;
            }

            using(var bufferWriter = new PooledByteBufferWriter(options.DefaultBufferSize))
            {
                using(var writer = new Writer.BinaryWriter(bufferWriter))
                {
                    // 写入头
                    writer.WriteHeader();
                    if(value == null)
                    {
                        return;
                    }
                    if (inputType == typeof(object) && value != null)
                    {
                        inputType = value!.GetType();
                    }

                    // 获取类型元数据
                    TypeMap typeMap = options.GetTypeMap(inputType);

                    // 写入序列化数据
                   

                    // 写入对象映射


                }
            }
        }
    }
}
