using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public ref partial struct BinaryReader
    {
        private readonly ReadOnlySpan<byte> _buffer;

        private readonly bool _isFinalBlock;
        private int _consumed;
        private BinaryTokenType _tokenType;
        private readonly BinaryTokenType _previousTokenType;
        private ushort _typeSeq;
        private readonly int _version;
        private byte _dicKeySeq;
    
        private readonly long _totalConsumed;
      
     
        private readonly ReadOnlySequence<byte> _sequence;

        public BinaryTokenType TokenType => _tokenType;


      
        internal ReadOnlySequence<byte> OriginalSequence => _sequence;

        internal ReadOnlySpan<byte> OriginalSpan => _sequence.IsEmpty ? _buffer : default;

        public bool IsFinalBlock => _isFinalBlock;

        public ReadOnlySpan<byte> ValueSpan { get; private set; }

        public BinaryTypeInfo CurrentTypeInfo { get; private set; }

        public ushort  CurrentPropertySeq { get; set; }

        public int Version => _version;

        /// <summary>
        /// Returns the total amount of bytes consumed by the <see cref="BinaryReader"/> so far
        /// for the current instance of the <see cref="BinaryReader"/> with the given UTF-8 encoded input text.
        /// </summary>
        public long BytesConsumed
        {
            get
            {
                return _totalConsumed + _consumed;
            }
        }

        
        public long TokenStartIndex { get; private set; }

       

        private readonly TypeMap _typeMap;

    
        public BinaryReader(ReadOnlySpan<byte> binaryData, bool isFinalBlock, BinaryReaderState state)
        {
            _buffer = binaryData;

            _isFinalBlock = isFinalBlock;
            //_isInputSequence = false;
            _typeSeq = state._typeSeq;
            _dicKeySeq = state._dicKeySeq;
            CurrentTypeInfo = state._typeInfo;
            CurrentPropertySeq = state._propertySeq;
            _typeMap = state._typeMap;
            _version = state._version;
           
            _tokenType = state._tokenType;
            _previousTokenType = state._previousTokenType;
            

            _consumed = 0;
            TokenStartIndex = 0;
            _totalConsumed = 0;

            ValueSpan = ReadOnlySpan<byte>.Empty;

            
            _sequence = default;
         
        }


#pragma warning disable IDE0060 // 删除未使用的参数
        public BinaryReader(ReadOnlySpan<byte> binaryData, BinarySerializerOptions options = default)
#pragma warning restore IDE0060 // 删除未使用的参数
            : this(binaryData, isFinalBlock: true, new BinaryReaderState())
        {
        }

        /// <summary>
        /// 读取指定字节数，并返回
        /// 此方法会修改Reader的读取位置，但不会修改Reader的ValueSpan属性
        /// </summary>
        /// <param name="len">待读取的长度</param>
        /// <param name="output">读取结果</param>
        /// <returns>成功返回ture，如果没有足够的数据返回false</returns>
        public bool ReadBytes(int len, out ReadOnlySpan<byte> output)
        {
            if((_consumed+len)> _buffer.Length)
            {
                output = default;
                return false;
            }
            output = _buffer.Slice(_consumed, len);
            _consumed += len;
            return true;
        }

        /// <summary>
        /// 读取指定字节数，并返回
        /// 此方法会修改Reader的读取位置及Reader的ValueSpan属性
        /// </summary>
        /// <param name="len">待读取的长度</param>
        /// <param name="output">读取结果</param>
        /// <returns>成功返回ture，如果没有足够的数据返回false</returns>
        internal bool ReadBytes(int len)
        {
            if((_consumed + len) > _buffer.Length)
            {
                return false;
            }

            _tokenType = BinaryTokenType.Bytes;
            ValueSpan = _buffer.Slice(_consumed, len);
            _consumed += len;

            return true;
        }

        /// <summary>
        /// 读取包含长度信息的字节组
        /// </summary>
        /// <returns></returns>
        public bool ReadBytes()
        {
            // 长度 16位或32位
            if (!RequestData(2))
            {
                return false;
            }

            byte b1 = _buffer[_consumed];

            // 如果最高位是1，表示31位长度，否则表示15位长度
            int len;
            int lenBytes;
            if ((b1 & 0x80) == 0x80)
            {
                if (!RequestData(4))
                {
                    return false;
                }

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

            if( !RequestData(len+lenBytes) )
            {
                return false;
            }

            _consumed += lenBytes;
            _tokenType = BinaryTokenType.Bytes;
            ValueSpan = _buffer.Slice(_consumed, len);
            _consumed += len;
            return true;
        }

        public BinaryReaderState CurrentState => new BinaryReaderState
        {
            _bytePosition = _consumed,
            _tokenType = _tokenType,
            _previousTokenType = _previousTokenType,
            _typeMap = _typeMap,
            _typeSeq = _typeSeq,
            _version = _version,
            _typeInfo = CurrentTypeInfo,
            _propertySeq = CurrentPropertySeq,
            _dicKeySeq = _dicKeySeq
        };

        public bool Read()
        {
            bool retVal = ReadSingleSegment();

            if (!retVal)
            {
                if (_isFinalBlock && TokenType == BinaryTokenType.None)
                {
                    ThrowHelper.ThrowBinaryReaderException(ref this, ExceptionResource.ExpectedBinaryTokens);
                }
            }
            return retVal;
        }


        private bool ReadSingleSegment()
        {
            if(_tokenType == BinaryTokenType.None)
            {
                return ReadFirstToken();
            }
            return true;
        }

        private bool ReadFirstToken()
        {
            if( !RequestData(2) )
            {
                return false;
            }

            return ReadTypeSeq();
            
        }

        internal bool ReadDictionaryKeySeq()
        {
            if (!RequestData(1))
            {
                return false;
            }

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
            return true;
        }

        internal bool ReadTypeSeq()
        {
            if (!RequestData(2))
            {
                return false;
            }

            _tokenType = BinaryTokenType.TypeSeq;
            ValueSpan = _buffer.Slice(_consumed, 2);
            _typeSeq = BitConverter.ToUInt16(ValueSpan);
            if(_typeSeq == TypeMap.NullTypeSeq)
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


            return true;
        }

        internal bool ReadStartToken()
        {
            if (!RequestData(5))
            {
                return false;
            }

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
            return true;
        }

        internal bool ReadEndArrayToken()
        {
            return ReadPropertyName();
        }

        internal bool ReadPropertyName()
        {
            if (!RequestData(1))
            {
                return false;
            }

            byte b = _buffer[_consumed];
            if ((b & 0x80) == 0x80)
            {
                // 键值对方式
            }
            else
            {
                if (!RequestData(2))
                {
                    return false;
                }

                ValueSpan = _buffer.Slice(_consumed, 2);
                ushort seq = (ushort)((ValueSpan[0] << 8) | ValueSpan[1]);
                if(seq == BinarySerializerConstants.EndObjectSeq)
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
                
                return true;
            }
            
            return false;
        }

   

        private bool RequestData(int len)
        {
            if ((_consumed + len) > _buffer.Length)
            {
                return false;
            }

            return true;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"TokenType = {_tokenType} (TokenStartIndex = {TokenStartIndex}) Consumed = {BytesConsumed}";

    }
}
