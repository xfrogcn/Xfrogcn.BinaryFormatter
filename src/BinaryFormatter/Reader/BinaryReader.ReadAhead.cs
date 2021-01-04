using System;

namespace Xfrogcn.BinaryFormatter
{
    /// <summary>
    /// 一次性读取
    /// </summary>
    public ref partial struct BinaryReader
    {
        /// <summary>
        /// 读取指定字节数，并返回
        /// 此方法会修改Reader的读取位置，但不会修改Reader的ValueSpan属性
        /// </summary>
        /// <param name="len">待读取的长度</param>
        /// <param name="output">读取结果</param>
        public void AheadReadBytes(int len, out ReadOnlySpan<byte> output)
        {
            output = _buffer.Slice(_consumed, len);
            _consumed += len;
        }

        /// <summary>
        /// 读取指定字节数，并返回
        /// 此方法会修改Reader的读取位置及Reader的ValueSpan属性
        /// </summary>
        /// <param name="len">待读取的长度</param>
        /// <param name="output">读取结果</param>
        internal void AheadReadBytes(int len)
        {
            _tokenType = BinaryTokenType.Bytes;
            ValueSpan = _buffer.Slice(_consumed, len);
            _consumed += len;
        }

        /// <summary>
        /// 读取包含长度信息的字节组
        /// </summary>
        public void AheadReadBytes()
        {
            byte b1 = _buffer[_consumed];
            // 如果最高位是1，表示31位长度，否则表示15位长度
            int len;
            int lenBytes;
            if ((b1 & 0x80) == 0x80)
            {

                len = ((_buffer[_consumed] & 0x7F) << 24) |
                    (_buffer[_consumed + 1] << 16) |
                    (_buffer[_consumed + 2] << 8) |
                    _buffer[_consumed + 3];

                lenBytes = 4;
            }
            else
            {
                len = _buffer[_consumed] << 8 | _buffer[_consumed + 1];
                lenBytes = 2;
            }

            if (len == 0)
            {
                ValueSpan = ReadOnlySpan<byte>.Empty;
            }

            _consumed += lenBytes;
            _tokenType = BinaryTokenType.Bytes;
            ValueSpan = _buffer.Slice(_consumed, len);
            _consumed += len;

        }

        public void AheadRead()
        {
            AheadReadSingleSegment();

            if (TokenType == BinaryTokenType.None)
            {
                ThrowHelper.ThrowBinaryReaderException(ref this, ExceptionResource.ExpectedBinaryTokens);
            }

        }

        private void AheadReadSingleSegment()
        {
            if (_tokenType == BinaryTokenType.None)
            {
                AheadReadFirstToken();
            }
        }

        private void AheadReadFirstToken()
        {
            AheadReadTypeSeq();

        }

        internal void AheadReadDictionaryKeySeq()
        {
            ValueSpan = _buffer.Slice(_consumed, 1);
            _dicKeySeq = ValueSpan[0];
            if (_dicKeySeq == BinarySerializerConstants.EndDictionaryKey)
            {
                _tokenType = BinaryTokenType.EndDictionaryKey;
            }
            else if (_dicKeySeq == BinarySerializerConstants.DictionaryKeySeq)
            {
                _tokenType = BinaryTokenType.DictionaryKeySeq;
            }
            else
            {
                ThrowHelper.ThrowBinaryReaderException(ref this, ExceptionResource.InvalidByte);
            }
            _consumed++;
        }


        internal void AheadReadTypeSeq()
        {
            _tokenType = BinaryTokenType.TypeSeq;
            ValueSpan = _buffer.Slice(_consumed, 2);
            _typeSeq = BitConverter.ToUInt16(ValueSpan);
            if (_typeSeq == TypeMap.NullTypeSeq)
            {
                _tokenType = BinaryTokenType.Null;
                CurrentTypeInfo = null;
            }
            else
            {
                CurrentTypeInfo = _typeMap.GetTypeInfo(_typeSeq);
            }

            if (CurrentTypeInfo == null && _tokenType != BinaryTokenType.Null)
            {
                throw new Exception();
            }
            _consumed += 2;
        }

        internal void AheadReadStartToken()
        {
           
            byte objType = _buffer[_consumed];
            ValueSpan = _buffer.Slice(_consumed + 1, 4);
            _consumed += 5;

            if (objType == 0x00)
            {
                //正常
                _tokenType = BinaryTokenType.StartObject;
            }
            else if (objType == 0xFF)
            {
                _tokenType = BinaryTokenType.ObjectRef;
            }
            else
            {
                ThrowHelper.ThrowBinaryReaderException(ref this, ExceptionResource.ExpectedBinaryTokens);
            }
        }

        internal void AheadReadEndArrayToken()
        {
            AheadReadPropertyName();
        }

        internal void AheadReadPropertyName()
        {
            byte b = _buffer[_consumed];
            if ((b & 0x80) == 0x80)
            {
                // 键值对方式
            }
            else
            {
                ValueSpan = _buffer.Slice(_consumed, 2);
                ushort seq = (ushort)((ValueSpan[0] << 8) | ValueSpan[1]);
                if (seq == BinarySerializerConstants.EndObjectSeq)
                {
                    // 对象结束
                    _tokenType = BinaryTokenType.EndObject;
                    CurrentPropertySeq = default;
                }
                else
                {
                    _tokenType = BinaryTokenType.PropertyName;
                    CurrentPropertySeq = seq;
                }

                _consumed += 2;

            }
        }


    }
}
