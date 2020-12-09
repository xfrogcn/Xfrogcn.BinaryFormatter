using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
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
            _typeSeq = default;
            CurrentTypeInfo = null;
            _typeMap = state._typeMap;

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

        public bool ReadBytes(int len)
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

        public BinaryReaderState CurrentState => new BinaryReaderState
        {
            _bytePosition = _consumed,
            _tokenType = _tokenType,
            _previousTokenType = _previousTokenType,
            _typeMap = _typeMap
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
            bool retVal = false;
            ValueSpan = default;

            if (!HasMoreData())
            {
                goto Done;
            }

            byte first = _buffer[_consumed];

            TokenStartIndex = _consumed;

            if (_tokenType == BinaryTokenType.None)
            {
                goto ReadFirstToken;
            }


            if (_tokenType == BinaryTokenType.StartObject)
            {
                //if (first == JsonConstants.CloseBrace)
                //{
                //    EndObject();
                //}
                //else
                //{
                //    if (first != JsonConstants.Quote)
                //    {
                //        ThrowHelper.ThrowJsonReaderException(ref this, ExceptionResource.ExpectedStartOfPropertyNotFound, first);
                //    }

                //    int prevConsumed = _consumed;
                //    long prevPosition = _bytePositionInLine;
                //    long prevLineNumber = _lineNumber;
                //    retVal = ConsumePropertyName();
                //    if (!retVal)
                //    {
                //        // roll back potential changes
                //        _consumed = prevConsumed;
                //        _tokenType = JsonTokenType.StartObject;
                //        _bytePositionInLine = prevPosition;
                //        _lineNumber = prevLineNumber;
                //    }
                //    goto Done;
                //}
            }
            else if (_tokenType == BinaryTokenType.StartArray)
            {
                //if (first == JsonConstants.CloseBracket)
                //{
                //    EndArray();
                //}
                //else
                //{
                //    retVal = ConsumeValue(first);
                //    goto Done;
                //}
            }
            else if (_tokenType == BinaryTokenType.PropertyName)
            {
                //retVal = ConsumeValue(first);
                goto Done;
            }
            else
            {
                //retVal = ConsumeNextTokenOrRollback(first);
                goto Done;
            }

            retVal = true;

        Done:
            return retVal;

        ReadFirstToken:
            retVal = ReadFirstToken();
            goto Done;
        }

        private bool ReadFirstToken()
        {
            if((_consumed+2)> _buffer.Length)
            {
                return false;
            }

            return ReadTypeSeq();
            
        }

        private bool ReadTypeSeq()
        {
            if ((_consumed + 2) >= _buffer.Length)
            {
                return false;
            }

            _tokenType = BinaryTokenType.TypeSeq;
            ValueSpan = _buffer.Slice(_consumed, 2);
            _typeSeq = BitConverter.ToUInt16(ValueSpan);
            CurrentTypeInfo = _typeMap.GetTypeInfo(_typeSeq);
            if (CurrentTypeInfo == null)
            {
                throw new Exception();
            }
            _consumed += 2;


            return true;
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

    }
}
