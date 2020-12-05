using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
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
        private bool _inObject;
        private bool _isNotPrimitive;
        private BinaryTokenType _tokenType;
        private BinaryTokenType _previousTokenType;
        private BinarySerializerOptions _readerOptions;
        // private BitStack _bitStack;

        private long _totalConsumed;
        private bool _isLastSegment;
        internal bool _stringHasEscaping;
        private readonly bool _isMultiSegment;
        private bool _trailingCommaBeforeComment;

        private SequencePosition _nextPosition;
        private SequencePosition _currentPosition;
        private readonly ReadOnlySequence<byte> _sequence;

        private bool IsLastSpan => _isFinalBlock && (!_isMultiSegment || _isLastSegment);

        internal ReadOnlySequence<byte> OriginalSequence => _sequence;

        internal ReadOnlySpan<byte> OriginalSpan => _sequence.IsEmpty ? _buffer : default;

        
        public ReadOnlySpan<byte> ValueSpan { get; private set; }

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

        internal bool IsInArray => !_inObject;
    }
}
