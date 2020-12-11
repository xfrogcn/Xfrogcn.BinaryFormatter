using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    public ref partial struct BinaryReader
    {
        public bool TryGetValue(out object value)
        {
            if (CurrentTypeInfo == null || CurrentTypeInfo.Type == TypeEnum.None)
            {
                value = default;
                return false;
            }

            TypeEnum t = CurrentTypeInfo.Type;
            bool isOk = true;
            switch (t)
            {
                case TypeEnum.Byte:
                    value = GetByte();
                    break;
                case TypeEnum.Int16:
                    value = GetInt16();
                    break;
                case TypeEnum.Int32:
                    value = GetInt32();
                    break;
                case TypeEnum.Int64:
                    value = GetInt64();
                    break;
                case TypeEnum.SByte:
                    value = GetSByte();
                    break;
                case TypeEnum.UInt16:
                    value = GetUInt16();
                    break;
                case TypeEnum.UInt32:
                    value = GetUInt32();
                    break;
                case TypeEnum.UInt64:
                    value = GetUInt64();
                    break;
                case TypeEnum.Single:
                    value = GetSingle();
                    break;
                case TypeEnum.Double:
                    value = GetDouble();
                    break;
                case TypeEnum.Decimal:
                    value = GetDecimal();
                    break;
                case TypeEnum.BigInteger:
                    value = GetBigInteger();
                    break;
                case TypeEnum.Complex:
                    value = GetComplex();
                    break;
                case TypeEnum.Guid:
                    value = GetGuid();
                    break;
                case TypeEnum.ByteArray:
                    value = ValueSpan.ToArray();
                    break;
                case TypeEnum.DateTime:
                    value = GetDateTime();
                    break;
                case TypeEnum.DateTimeOffset:
                    value = GetDateTimeOffset();
                    break;
                case TypeEnum.TimeSpan:
                    value = GetTimeSpan();
                    break;
                case TypeEnum.Boolean:
                    value = GetBoolean();
                    break;
                case TypeEnum.Char:
                    value = GetUInt16();
                    break;
                case TypeEnum.String:
                    value = GetString();
                    break;
                case TypeEnum.Uri:
                    value = new Uri(GetString());
                    break;
                case TypeEnum.DBNull:
                    value = DBNull.Value;
                    break;
                default:
                    isOk = false;
                    value = default;
                    break;

            }

            return isOk;
        }

    }
}
