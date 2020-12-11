using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    public ref partial struct BinaryReader
    {
        private readonly static Dictionary<TypeEnum, int> _binaryTypeToBytesLen
            = new Dictionary<TypeEnum, int>()
            {
                { TypeEnum.Byte, BinarySerializerConstants.BytesCount_Byte },
                { TypeEnum.Int16 , BinarySerializerConstants.BytesCount_Int16 },
                { TypeEnum.Int32 , BinarySerializerConstants.BytesCount_Int32 },
                { TypeEnum.Int64 , BinarySerializerConstants.BytesCount_Int64 },
                { TypeEnum.SByte , BinarySerializerConstants.BytesCount_SByte },
                { TypeEnum.UInt16 , BinarySerializerConstants.BytesCount_UInt16 },
                { TypeEnum.UInt32 , BinarySerializerConstants.BytesCount_UInt32 },
                { TypeEnum.UInt64 , BinarySerializerConstants.BytesCount_UInt64 },
                { TypeEnum.Single , BinarySerializerConstants.BytesCount_Single },
                { TypeEnum.Double , BinarySerializerConstants.BytesCount_Double },
                { TypeEnum.Decimal , BinarySerializerConstants.BytesCount_Decimal },
                { TypeEnum.Guid , BinarySerializerConstants.BytesCount_Guid },
                { TypeEnum.DateTime , BinarySerializerConstants.BytesCount_DateTime },
                { TypeEnum.DateTimeOffset , BinarySerializerConstants.BytesCount_DateTimeOffset },
                { TypeEnum.TimeSpan , BinarySerializerConstants.BytesCount_TimeSpan },
                { TypeEnum.Boolean , BinarySerializerConstants.BytesCount_Boolean },
                { TypeEnum.Char , BinarySerializerConstants.BytesCount_Char },
                { TypeEnum.DBNull , BinarySerializerConstants.BytesCount_DBNull },
            };

        public bool TryReadValue(out bool success)
        {
            if(_typeSeq == TypeMap.NullTypeSeq)
            {
                ValueSpan = ReadOnlySpan<byte>.Empty;
                success = true;
                return true;
            }
            if (CurrentTypeInfo == null || CurrentTypeInfo.Type == TypeEnum.None)
            {
                success = false;
                return false;
            }

            TypeEnum t = CurrentTypeInfo.Type;
            if(_binaryTypeToBytesLen.ContainsKey(t))
            {
                success = true;
                return ReadBytes(_binaryTypeToBytesLen[t]);
            }

            if(t == TypeEnum.ByteArray || t == TypeEnum.String)
            {
                success = true;
                return ReadBytes();
            }

            success = false;
            return false;
        }

       
    }
}
