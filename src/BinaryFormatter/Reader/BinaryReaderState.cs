using System;
namespace Xfrogcn.BinaryFormatter
{
    public struct BinaryReaderState
    {
        internal long _bytePosition;
        internal bool _inObject;
        internal bool _isNotPrimitive;
        internal BinaryTokenType _tokenType;
        internal BinaryTokenType _previousTokenType;
        internal BinaryReaderOptions _readerOptions;
        internal TypeMap _typeMap;
        internal ushort _typeSeq;
        //internal BitStack _bitStack;


        public BinaryReaderState(TypeMap typeMap, BinaryReaderOptions options = default)
        {
            _bytePosition = default;
            _inObject = default;
            _isNotPrimitive = default;
            _tokenType = default;
            _previousTokenType = default;
            _readerOptions = options;
            _typeMap = typeMap;
            _typeSeq = default;

            // Only allocate if the user reads a JSON payload beyond the depth that the _allocationFreeContainer can handle.
            // This way we avoid allocations in the common, default cases, and allocate lazily.
            // _bitStack = default;
        }
    }
}
