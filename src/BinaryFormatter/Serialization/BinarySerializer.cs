using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xfrogcn.BinaryFormatter.Serialization;

namespace Xfrogcn.BinaryFormatter
{
    public static partial class BinarySerializer
    {
        public static Task SerializeAsync<TValue>(
            Stream stream,
            TValue value,
            BinarySerializerOptions options = null,
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
            BinarySerializerOptions options = null,
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
            BinarySerializerOptions options,
            CancellationToken cancellationToken)
        {
            if( options == null)
            {
                options = BinarySerializerOptions.s_defaultOptions;
            }

            const float FlushThreshold = .9f;

            using (var bufferWriter = new PooledByteBufferWriter(options.DefaultBufferSize))
            {
                using(var writer = new BinaryWriter(bufferWriter, options))
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

                    WriteStack state = default;

                    BinaryConverter converterBase = state.Initialize(inputType, options, true);

                    bool isFinalBlock;
                    do
                    {
                        state.FlushThreshold = (int)(bufferWriter.Capacity * FlushThreshold);

                        isFinalBlock = WriteCore(converterBase, writer, value, options, ref state);

                        await bufferWriter.WriteToStreamAsync(stream, cancellationToken);

                        bufferWriter.Clear();

                    } while (!isFinalBlock);

                    //元数据
                    long startPosition = writer.BytesCommitted;

                    state.GetTypeList().ForEach(bi =>
                    {
                        writer.WriteTypeInfo(bi);
                    });
                    // 主类型
                    writer.WriteBytes(BitConverter.GetBytes(state.PrimaryTypeSeq));
                    // 类型元数据偏移
                    long endPosition = writer.BytesCommitted + writer.BytesPending;

                    writer.WriteBytes(BitConverter.GetBytes((uint)(endPosition - startPosition)));
                    writer.Flush();

                    await bufferWriter.WriteToStreamAsync(stream, cancellationToken);
                    bufferWriter.Clear();

                }
            }
        }
    }
}
