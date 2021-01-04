using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xfrogcn.BinaryFormatter.Serialization;

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

            return ReadAsync<object>(stream, typeof(object), options, cancellationToken);
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
            if (!stream.CanSeek)
            {
                throw new Exception();
            }

            if(options == null)
            {
                options = BinarySerializerOptions.s_defaultOptions;
            }

            ReadStack state = default;

            // 读取头
            if (stream.Length < 4)
            {
                ThrowHelper.ThrowBinaryException_InvalidBinaryFormat();
            }

            byte[] headerBytes =  new byte[4];
            await stream.ReadAsync(headerBytes, 0, 4).ConfigureAwait(false);
            if(headerBytes[0]!= (byte)'X' || headerBytes[1] != (byte)'B' || headerBytes[2] != (byte)'F')
            {
                ThrowHelper.ThrowBinaryException_InvalidBinaryFormat();
            }

            state.Version = headerBytes[3];

            // 空值
            if (stream.Length <= 4 && (returnType == typeof(object) || returnType.IsClass || returnType.IsNullableType()))
            {
                return (TValue)(object)null;
            }
  
            stream.Seek(-4, SeekOrigin.End);
            await stream.ReadAsync(headerBytes, 0, 4).ConfigureAwait(false);
            uint mapPosition = BitConverter.ToUInt32(headerBytes);
            if (mapPosition >= stream.Length)
            {
                ThrowHelper.ThrowBinaryException_InvalidBinaryFormat();
            }

            
            stream.Seek(-(mapPosition+4), SeekOrigin.End);

            byte[] buffer = ArrayPool<byte>.Shared.Rent((int)mapPosition);
            await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);

            ReadMetadata(new ReadOnlySpan<byte>(buffer), ref state);

            // 类型解析
            state.ResolveTypes(options, returnType);

            

            // 初始化
            state.Initialize(state.PrimaryType, options, true);

            BinaryConverter converter = state.Current.BinaryPropertyInfo.ConverterBase;

            var readerState = new BinaryReaderState(state.TypeMap, state.Version, options.GetReaderOptions());
           
            buffer = ArrayPool<byte>.Shared.Rent(options.DefaultBufferSize);
            int bytesInBuffer = 0;
            long totalBytesRead = 0;
            int clearMax = 0;

            stream.Seek(4, SeekOrigin.Begin);
            long dataLength = stream.Length - mapPosition - 4 - 4;

            while (true)
            {
                bool isFinalBlock = false;
                // 从流读取到缓冲区
                while (true)
                {
                    int bytesRead = await stream.ReadAsync(buffer.AsMemory(bytesInBuffer), cancellationToken).ConfigureAwait(false);
                    if (bytesRead == 0 )
                    {
                        isFinalBlock = true;
                        break;
                    }

                    totalBytesRead += bytesRead;
                    bytesInBuffer += bytesRead;

                    if( totalBytesRead>= dataLength)
                    {
                        isFinalBlock = true;
                        bytesInBuffer -= (int)(totalBytesRead - dataLength);
                        totalBytesRead = dataLength;

                        break;
                    }

                    if (bytesInBuffer == buffer.Length)
                    {
                        break;
                    }
                }

                // 可清理的数据
                if(bytesInBuffer > clearMax)
                {
                    clearMax = bytesInBuffer;
                }
                Debug.WriteLine(bytesInBuffer);
                TValue value = ReadCore<TValue>(
                    ref readerState,
                    isFinalBlock,
                    new ReadOnlySpan<byte>(buffer, 0, bytesInBuffer),
                    options,
                    ref state,
                    converter);

                int bytesConsumed = checked((int)state.BytesConsumed);

                bytesInBuffer -= bytesConsumed;

                if (isFinalBlock)
                {
                    // The reader should have thrown if we have remaining bytes.
                    Debug.Assert(bytesInBuffer == 0);

                    return value;
                }

                // 如果剩余处理的数据量大于缓冲区的一半，扩大缓冲区
                if ((uint)bytesInBuffer > ((uint)buffer.Length / 2))
                {
                    // We have less than half the buffer available, double the buffer size.
                    byte[] dest = ArrayPool<byte>.Shared.Rent((buffer.Length < (int.MaxValue / 2)) ? buffer.Length * 2 : int.MaxValue);

                    // Copy the unprocessed data to the new buffer while shifting the processed bytes.
                    Buffer.BlockCopy(buffer, bytesConsumed, dest, 0, bytesInBuffer);

                    new Span<byte>(buffer, 0, clearMax).Clear();
                    ArrayPool<byte>.Shared.Return(buffer);

                    clearMax = bytesInBuffer;
                    buffer = dest;
                }
                else if (bytesInBuffer != 0)
                {
                    // 将缓冲区数据移到开始位置.
                    Buffer.BlockCopy(buffer, bytesConsumed, buffer, 0, bytesInBuffer);
                }



            }

            //return default;

        }

        internal static void ReadMetadata(ReadOnlySpan<byte> buffer, ref ReadStack state)
        {
            BinaryReader reader = new BinaryReader(buffer);
            reader.ReadMetadata(ref state);
        }

        private static TValue ReadCore<TValue>(
           ref BinaryReaderState readerState,
           bool isFinalBlock,
           ReadOnlySpan<byte> buffer,
           BinarySerializerOptions options,
           ref ReadStack state,
           BinaryConverter converterBase)
        {
            var reader = new BinaryReader(buffer, isFinalBlock, readerState);

            //state.ReadAhead = !isFinalBlock;
            state.BytesConsumed = 0;

            TValue value = ReadCore<TValue>(converterBase, ref reader, options, ref state);

            readerState = reader.CurrentState;
            return value!;
        }

    }
}
