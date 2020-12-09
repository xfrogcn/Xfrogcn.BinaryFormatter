using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    public ref partial struct BinaryReader
    {
        public string GetString()
        {
            if (_tokenType == BinaryTokenType.Null)
            {
                return null;
            }

            if (TokenType != BinaryTokenType.Bytes)
            {
                throw ThrowHelper.GetInvalidOperationException_ExpectedString(TokenType);
            }

            ReadOnlySpan<byte> span = ValueSpan;

            return BinaryReaderHelper.TranscodeHelper(span);
        }
    }
}
