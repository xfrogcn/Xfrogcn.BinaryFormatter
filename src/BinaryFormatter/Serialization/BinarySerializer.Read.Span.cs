using System;
using Xfrogcn.BinaryFormatter.Serialization;

namespace Xfrogcn.BinaryFormatter
{
    public static partial class BinarySerializer
    {

        public static TValue Deserialize<TValue>(ReadOnlySpan<byte> bytes, BinarySerializerOptions options = null)
        {
            return (TValue)Deserialize(bytes, typeof(TValue), options);
        }

        public static object Deserialize(ReadOnlySpan<byte> bytes, BinarySerializerOptions options = null)
        {
            return Deserialize(bytes, BinaryClassInfo.ObjectType, options);
        }

        public static object Deserialize(ReadOnlySpan<byte> bytes, Type returnType, BinarySerializerOptions options = null)
        {
            if (options == null)
            {
                options = BinarySerializerOptions.s_defaultOptions;
            }

            ReadStack state = default;

            returnType ??= BinaryClassInfo.ObjectType;

            // 读取头
            if (bytes.Length < 4)
            {
                ThrowHelper.ThrowBinaryException_InvalidBinaryFormat();
            }

            var headerBytes = bytes.Slice(0, 4);
            if (headerBytes[0] != (byte)'X' || headerBytes[1] != (byte)'B' || headerBytes[2] != (byte)'F')
            {
                ThrowHelper.ThrowBinaryException_InvalidBinaryFormat();
            }

            state.Version = headerBytes[3];
            // 空值
            if (bytes.Length <= 4 )
            {
                return (object)null;
            }

            // 读取类型映射
            uint mapPosition = BitConverter.ToUInt32(bytes[^4..]);
            if (mapPosition >= bytes.Length)
            {
                ThrowHelper.ThrowBinaryException_InvalidBinaryFormat();
            }

            // 读取元数据
            var metadataBytes = bytes[(bytes.Length - (int)mapPosition - 4)..];

            ReadMetadata(metadataBytes, ref state);

            // 类型解析
            state.ResolveTypes(options, returnType);
            // 初始化
            state.Initialize(state.PrimaryType, options, false);

            BinaryConverter converter = state.Current.BinaryPropertyInfo.ConverterBase;

            var readerState = new BinaryReaderState(state.TypeMap, state.Version, options.GetReaderOptions());
            var data = bytes[4..];
            var reader = new BinaryReader(data, isFinalBlock: true, readerState);

            return ReadCore<object>(ref reader, state.PrimaryType,ref state, options);
        }
    }
}
