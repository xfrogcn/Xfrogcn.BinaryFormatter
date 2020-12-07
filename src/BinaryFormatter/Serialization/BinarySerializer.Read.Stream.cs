using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Xfrogcn.BinaryFormatter
{
    public static partial class BinarySerializer
    {
        /// <summary>
        /// 从二进制流中反序列化指定类型
        /// </summary>
        /// <typeparam name="TValue">目标类型</typeparam>
        /// <param name="stream">数据流</param>
        /// <param name="options">序列化设置</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>实例对象</returns>
        public static ValueTask<TValue> DeserializeAsync<TValue>(
            Stream stream,
            BinarySerializerOptions options = null,
            CancellationToken cancellationToken  = default)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            return ReadAsync<TValue>(stream, typeof(TValue), options, cancellationToken);
        }

        /// <summary>
        /// 从二进制流中反序列化
        /// </summary>
        /// <param name="stream">数据流</param>
        /// <param name="options">序列化设置</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>实例对象</returns>
        public static ValueTask<object> DeserializeAsync(
            Stream stream,
            BinarySerializerOptions options = null,
            CancellationToken cancellationToken = default)
        {
            if(stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }


            return ReadAsync<object>(stream, null, options, cancellationToken);
        }


        /// <summary>
        /// 从二进制流中反序列化
        /// </summary>
        /// <typeparam name="TValue">返回类型</typeparam>
        /// <param name="stream">二进制流</param>
        /// <param name="returnType">返回类型，可为null，如果为null，自动从流中获取类型</param>
        /// <param name="options">序列化设置</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>实例对象</returns>
        public static async ValueTask<TValue> ReadAsync<TValue>(
            Stream stream,
            Type returnType,
            BinarySerializerOptions options,
            CancellationToken cancellationToken)
        {
            if(options == null)
            {
                options = BinarySerializerOptions.s_defaultOptions;
            }

            ReadStack state = default;

            // 读取头
            if (stream.Length < 4)
            {
                //
                throw new Exception();
            }

            byte[] headerBytes =  new byte[4];
            await stream.ReadAsync(headerBytes, 0, 4).ConfigureAwait(false);
            if(headerBytes[0]!= (byte)'X' || headerBytes[1] != (byte)'B' || headerBytes[2] != (byte)'F')
            {
                throw new Exception();
            }

            state.Version = headerBytes[3];

            // 读取类型映射
            long position = stream.Position;

            stream.Seek(-4, SeekOrigin.End);
            await stream.ReadAsync(headerBytes, 0, 4).ConfigureAwait(false);
            uint mapPosition = BitConverter.ToUInt32(headerBytes);
            if (mapPosition >= stream.Length)
            {
                throw new Exception();
            }

            stream.Seek(-(mapPosition+4), SeekOrigin.End);
            byte[] buffer = ArrayPool<byte>.Shared.Rent(options.DefaultBufferSize);
            int bytesInBuffer = 0;

            while (true)
            {
                while (true)
                {
                    int bytesRead = await stream.ReadAsync(buffer.AsMemory(bytesInBuffer), cancellationToken).ConfigureAwait(false);
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    if(bytesInBuffer == buffer.Length)
                    {
                        break;
                    }
                }

                // Read



            }

            return default;

        }

    }
}
