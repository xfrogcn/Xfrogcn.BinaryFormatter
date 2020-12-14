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
        private ReadOnlySpan<byte> _buffer;

        private readonly bool _isFinalBlock;
        private readonly bool _isInputSequence;

        // private long _lineNumber;
        // private long _bytePositionInLine;

        // bytes consumed in the current segment (not token)
        private int _consumed;
        //private bool _inObject;
        //private bool _isNotPrimitive;
        private BinaryTokenType _tokenType;
        private BinaryTokenType _previousTokenType;
        private ushort _typeSeq;
        private int _version;
      //  private BinarySerializerOptions _readerOptions;
        // private BitStack _bitStack;

        private long _totalConsumed;
        //private bool _isLastSegment;
        //internal bool _stringHasEscaping;
        private readonly bool _isMultiSegment;
        //private bool _trailingCommaBeforeComment;

        //private SequencePosition _nextPosition;
        //private SequencePosition _currentPosition;
        private readonly ReadOnlySequence<byte> _sequence;

        public BinaryTokenType TokenType => _tokenType;


        // private bool IsLastSpan => _isFinalBlock && (!_isMultiSegment || _isLastSegment);

        internal ReadOnlySequence<byte> OriginalSequence => _sequence;

        internal ReadOnlySpan<byte> OriginalSpan => _sequence.IsEmpty ? _buffer : default;

        public bool IsFinalBlock => _isFinalBlock;

        public ReadOnlySpan<byte> ValueSpan { get; private set; }

        public BinaryTypeInfo CurrentTypeInfo { get; private set; }

        public ushort  CurrentPropertySeq { get; set; }

        public int Version => _version;

        /// <summary>
        /// Returns the total amount of bytes consumed by the <see cref="Utf8JsonReader"/> so far
        /// for the current instance of the <see cref="Utf8JsonReader"/> with the given UTF-8 encoded input text.
        /// </summary>
        public long BytesConsumed
        {
            get
            {
#if DEBUG
                if (!_isInputSequence)
                {
                    Debug.Assert(_totalConsumed == 0);
                }
#endif
                return _totalConsumed + _consumed;
            }
        }

        
        public long TokenStartIndex { get; private set; }

        /// <summary>
        /// Tracks the recursive depth of the nested objects / arrays within the JSON text
        /// processed so far. This provides the depth of the current token.
        /// </summary>
        public int CurrentDepth
        {
            //get
            //{
            //    int readerDepth = _bitStack.CurrentDepth;
            //    if (TokenType == JsonTokenType.StartArray || TokenType == JsonTokenType.StartObject)
            //    {
            //        Debug.Assert(readerDepth >= 1);
            //        readerDepth--;
            //    }
            //    return readerDepth;
            //}
            get
            {
                return 0;
            }
        }

        private TypeMap _typeMap;

       // internal bool IsInArray => !_inObject;

        public BinaryReader(ReadOnlySpan<byte> binaryData, bool isFinalBlock, BinaryReaderState state)
        {
            _buffer = binaryData;

            _isFinalBlock = isFinalBlock;
            _isInputSequence = false;
            _typeSeq = state._typeSeq;
            CurrentTypeInfo = state._typeInfo;
            CurrentPropertySeq = state._propertySeq;
            _typeMap = state._typeMap;
            _version = state._version;
            //_lineNumber = state._lineNumber;
            //_bytePositionInLine = state._bytePositionInLine;
            // _inObject = default; //state._inObject;
           // _isNotPrimitive = state._isNotPrimitive;
            //_stringHasEscaping = state._stringHasEscaping;
            //_trailingCommaBeforeComment = state._trailingCommaBeforeComment;
            _tokenType = state._tokenType;
            _previousTokenType = state._previousTokenType;
            //_readerOptions = state._readerOptions;
            //if (_readerOptions.MaxDepth == 0)
            //{
            //    _readerOptions.MaxDepth = JsonReaderOptions.DefaultMaxDepth;  // If max depth is not set, revert to the default depth.
            //}
            //_bitStack = state._bitStack;

            _consumed = 0;
            TokenStartIndex = 0;
            _totalConsumed = 0;
            //_isLastSegment = _isFinalBlock;
            _isMultiSegment = false;

            ValueSpan = ReadOnlySpan<byte>.Empty;

            //_currentPosition = default;
            //_nextPosition = default;
            _sequence = default;
          // HasValueSequence = false;
           // ValueSequence = ReadOnlySequence<byte>.Empty;
        }


        public BinaryReader(ReadOnlySpan<byte> binaryData, BinarySerializerOptions options = default)
            : this(binaryData, isFinalBlock: true, new BinaryReaderState())
        {
        }

        public bool ReadBytes(int len, out ReadOnlySpan<byte> output)
        {
            if((_consumed+len)>= _buffer.Length)
            {
                output = default;
                return false;
            }
            output = _buffer.Slice(_consumed, len);
            _consumed += len;
            return true;
        }

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

            // 如果最高位是1，表示31位长度，否则表示15位长度
            int len = default;
            byte b1 = _buffer[_consumed];
            int lenBytes = 4;
            if ((b1 & 0x80) == 0x80)
            {
                if ( !RequestData(4))
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

            if(len == 0)
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
            _propertySeq = CurrentPropertySeq
        };

        public bool Read()
        {
            bool retVal = _isMultiSegment ? ReadMultiSegment() : ReadSingleSegment();

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
        //    bool retVal = false;
        //    ValueSpan = default;

        //    if (!HasMoreData())
        //    {
        //        goto Done;
        //    }

        //    byte first = _buffer[_consumed];

        //    TokenStartIndex = _consumed;

        //    if (_tokenType == BinaryTokenType.None)
        //    {
        //        goto ReadFirstToken;
        //    }


        //    if (_tokenType == BinaryTokenType.StartObject)
        //    {
        //        //if (first == JsonConstants.CloseBrace)
        //        //{
        //        //    EndObject();
        //        //}
        //        //else
        //        //{
        //        //    if (first != JsonConstants.Quote)
        //        //    {
        //        //        ThrowHelper.ThrowJsonReaderException(ref this, ExceptionResource.ExpectedStartOfPropertyNotFound, first);
        //        //    }

        //        //    int prevConsumed = _consumed;
        //        //    long prevPosition = _bytePositionInLine;
        //        //    long prevLineNumber = _lineNumber;
        //        //    retVal = ConsumePropertyName();
        //        //    if (!retVal)
        //        //    {
        //        //        // roll back potential changes
        //        //        _consumed = prevConsumed;
        //        //        _tokenType = JsonTokenType.StartObject;
        //        //        _bytePositionInLine = prevPosition;
        //        //        _lineNumber = prevLineNumber;
        //        //    }
        //        //    goto Done;
        //        //}
        //    }
        //    else if (_tokenType == BinaryTokenType.StartArray)
        //    {
        //        //if (first == JsonConstants.CloseBracket)
        //        //{
        //        //    EndArray();
        //        //}
        //        //else
        //        //{
        //        //    retVal = ConsumeValue(first);
        //        //    goto Done;
        //        //}
        //    }
        //    else if (_tokenType == BinaryTokenType.PropertyName)
        //    {
        //        //retVal = ConsumeValue(first);
        //        goto Done;
        //    }
        //    else if(_tokenType == BinaryTokenType.TypeSeq)
        //    {
        //        retVal = true;
        //        goto Done;
        //    }
        //    else
        //    {
        //        retVal = true;
        //        //retVal = ConsumeNextTokenOrRollback(first);
        //        goto Done;
        //    }

        //    retVal = true;

        //Done:
        //    return retVal;

        //ReadFirstToken:
        //    retVal = ReadFirstToken();
        //    goto Done;
        }

        private bool ReadFirstToken()
        {
            if( !RequestData(2) )
            {
                return false;
            }

            return ReadTypeSeq();
            
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
            if (!RequestData(1))
            {
                return false;
            }

            byte objType = _buffer[_consumed];
            if (objType == 0x00)
            {
                //正常
                _tokenType = BinaryTokenType.StartObject;
                _consumed++;
                return true;
            }
            else if (objType == 0xFF)
            {
                // 引用
                if(!RequestData(5))
                {
                    return false;
                }

                _tokenType = BinaryTokenType.ObjectRef;
                ValueSpan = _buffer.Slice(_consumed + 1, 4);
                _consumed += 5;
                return true;
            }
            else
            {
                ThrowHelper.ThrowBinaryReaderException(ref this, ExceptionResource.ExpectedBinaryTokens);
            }
            return false;
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
                if(seq == 0x7FFF)
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

        public void Rollback(int len)
        {
            _consumed -= len;
            Debug.Assert(_consumed >= 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasMoreData()
        {
            if (_consumed >= (uint)_buffer.Length)
            {
                return false;
            }
            return true;
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
