using System;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    internal static partial class BinaryReaderHelper
    {
        public static readonly UTF8Encoding s_utf8Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        public static string TranscodeHelper(ReadOnlySpan<byte> utf8Unescaped)
        {
            return s_utf8Encoding.GetString(utf8Unescaped);
        }

    }
}
