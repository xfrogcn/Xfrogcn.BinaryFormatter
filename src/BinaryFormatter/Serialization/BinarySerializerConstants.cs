namespace Xfrogcn.BinaryFormatter
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
        internal const ushort EndObjectSeq = 0x7FFF;
        internal const byte EndDictionaryKey = 0xFF;
        internal const byte DictionaryKeySeq = 0x80;

        internal const byte MetadataBlock_TypeInfo = 0x00;
        internal const byte MetadataBlock_RefMap = 0x01;
        internal const byte MetadataBlock_End = 0xFF;

        internal const int MaxWriterDepth = 1_000;

        public const int RemoveFlagsBitMask = 0x7FFFFFFF;
        

        public const int UnboxedParameterCountThreshold = 4;
    }
}
