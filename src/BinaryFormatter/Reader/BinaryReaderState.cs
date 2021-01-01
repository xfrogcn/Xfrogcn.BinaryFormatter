using System;
namespace Xfrogcn.BinaryFormatter
{
    public struct BinaryReaderState
    {
        internal long _bytePosition;
        internal bool _inObject;
        internal bool _isNotPrimitive;
        internal int _version;
        internal BinaryTokenType _tokenType;
        internal BinaryTokenType _previousTokenType;
        internal BinaryReaderOptions _readerOptions;
        internal BinaryTypeInfo _typeInfo;
        internal ushort _typeSeq;
        internal byte _dicKeySeq;
        internal ushort _propertySeq;
        internal TypeMap _typeMap;
        //internal BitStack _bitStack;


        public BinaryReaderState(TypeMap typeMap, int version, BinaryReaderOptions options = default)
        {
            _bytePosition = default;
            _inObject = default;
            _isNotPrimitive = default;
            _tokenType = default;
            _previousTokenType = default;
            _readerOptions = options;
            _typeMap = typeMap;
            _typeSeq = default;
            _version = version;
            _typeInfo = default;
            _propertySeq = default;
            _dicKeySeq = default;

            
        }
    }
}
