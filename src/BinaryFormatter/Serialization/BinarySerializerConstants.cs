﻿namespace Xfrogcn.BinaryFormatter
{
    internal static class BinarySerializerConstants
    {
        internal const int BytesCount_Boolean = 1;
        internal const int BytesCount_Byte = 1;
        internal const int BytesCount_Int16 = 2;
        internal const int BytesCount_Int32 = 4;
        internal const int BytesCount_Int64 = 8;
        internal const int BytesCount_SByte = 1;
        internal const int BytesCount_UInt16 = 2;
        internal const int BytesCount_UInt32 = 4;
        internal const int BytesCount_UInt64 = 8;
        internal const int BytesCount_Single = 4;
        internal const int BytesCount_Double = 8;
        internal const int BytesCount_Decimal = 16;
        internal const int BytesCount_Guid = 16;
        internal const int BytesCount_DateTime = 9;
        internal const int BytesCount_DateTimeOffset = 16;
        internal const int BytesCount_TimeSpan = 8;
        internal const int BytesCount_Char = 2;
        internal const int BytesCount_DBNull = 0;
        internal const int BytesCount_Auto = -1;
        internal const int BytesCount_Dynamic = 0;

        public const int UnboxedParameterCountThreshold = 4;
    }
}