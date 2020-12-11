using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{

    internal class EnumConverter<T> : BinaryConverter<T>
        where T : struct, Enum
    {
        private static readonly HashSet<Type> typeCodeTypes = new HashSet<Type>
        {
            typeof(Int32),
            typeof(UInt32),
            typeof(Int64),
            typeof(UInt64),
            typeof(SByte),
            typeof(Byte),
            typeof(Int16),
            typeof(UInt16)
        };

        public override int GetBytesCount(ref BinaryReader reader, BinarySerializerOptions options)
        {
            return BinarySerializerConstants.BytesCount_Auto;
        }

        private static readonly TypeCode s_enumTypeCode = Type.GetTypeCode(typeof(T));

        public override T Read(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options)
        {
            switch (s_enumTypeCode)
            {
                // Switch cases ordered by expected frequency

                case TypeCode.Int32:
                    if (TryGetValue<int>(ref reader, out int int32))
                    {
                        return Unsafe.As<int, T>(ref int32);
                    }
                    break;
                case TypeCode.UInt32:
                    if (TryGetValue<uint>(ref reader, out uint uint32))
                    {
                        return Unsafe.As<uint, T>(ref uint32);
                    }
                    break;
                case TypeCode.UInt64:
                    if (TryGetValue<ulong>(ref reader, out ulong uint64))
                    {
                        return Unsafe.As<ulong, T>(ref uint64);
                    }
                    break;
                case TypeCode.Int64:
                    if (TryGetValue<long>(ref reader, out long int64))
                    {
                        return Unsafe.As<long, T>(ref int64);
                    }
                    break;

                // When utf8reader/writer will support all primitive types we should remove custom bound checks
                // https://github.com/dotnet/runtime/issues/29000
                case TypeCode.SByte:
                    if (TryGetValue(ref reader, out sbyte byte8))
                    {
                        return Unsafe.As<sbyte, T>(ref byte8);
                    }
                    break;
                case TypeCode.Byte:
                    if (TryGetValue(ref reader, out byte ubyte8))
                    {
                        return Unsafe.As<byte, T>(ref ubyte8);
                    }
                    break;
                case TypeCode.Int16:
                    if (TryGetValue(ref reader, out short shortValue) )
                    {
                        return Unsafe.As<short, T>(ref shortValue);
                    }
                    break;
                case TypeCode.UInt16:
                    if (TryGetValue(ref reader, out ushort ushortValue) )
                    {
                        return Unsafe.As<ushort, T>(ref ushortValue);
                    }
                    break;
            }

            ThrowHelper.ThrowBinaryException();
            return default;
        }


        private bool TryGetValue<TValue>(ref BinaryReader reader, out TValue value)
            where TValue : struct
        {
            if(reader.CurrentTypeInfo == null)
            {
                value = default;
                return false;
            }

            bool isOk = reader.TryGetValue(out object val);
            if( isOk && val != null && typeCodeTypes.Contains(  typeof(TValue)) && typeCodeTypes.Contains(val.GetType()))
            {
                value = (TValue)val;
            }
            else
            {
                value = default;
                isOk = false;
            }
            return isOk;
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            //typeInfo.Type = TypeEnum.Enum;
            typeInfo.SerializeType = ClassType.Value;
            switch (s_enumTypeCode)
            {
                case TypeCode.Int32:
                    typeInfo.Type = TypeEnum.Int32;
                    break;
                case TypeCode.UInt32:
                    typeInfo.Type = TypeEnum.UInt32;
                    break;
                case TypeCode.Int64:
                    typeInfo.Type = TypeEnum.Int64;
                    break;
                case TypeCode.UInt64:
                    typeInfo.Type = TypeEnum.UInt64;
                    break;
                case TypeCode.Byte:
                    typeInfo.Type = TypeEnum.Byte;
                    break;
                case TypeCode.SByte:
                    typeInfo.Type = TypeEnum.SByte;
                    break;
                case TypeCode.Int16:
                    typeInfo.Type = TypeEnum.Int16;
                    break;
                case TypeCode.UInt16:
                    typeInfo.Type = TypeEnum.UInt16;
                    break;
                default:
                    ThrowHelper.ThrowBinaryException();
                    break;
            }
            typeInfo.FullName = options.GetTypeFullName(typeof(T));
        }

        public override void Write(BinaryWriter writer, T value, BinarySerializerOptions options)
        {
            switch (s_enumTypeCode)
            {
                case TypeCode.Int32:
                    writer.WriteInt32Value(Unsafe.As<T, int>(ref value));
                    break;
                case TypeCode.UInt32:
                    writer.WriteUInt32Value(Unsafe.As<T, uint>(ref value));
                    break;
                case TypeCode.UInt64:
                    writer.WriteUInt64Value(Unsafe.As<T, ulong>(ref value));
                    break;
                case TypeCode.Int64:
                    writer.WriteInt64Value(Unsafe.As<T, long>(ref value));
                    break;
                case TypeCode.Int16:
                    writer.WriteInt16Value(Unsafe.As<T, short>(ref value));
                    break;
                case TypeCode.UInt16:
                    writer.WriteUInt16Value(Unsafe.As<T, ushort>(ref value));
                    break;
                case TypeCode.Byte:
                    writer.WriteByteValue(Unsafe.As<T, byte>(ref value));
                    break;
                case TypeCode.SByte:
                    writer.WriteSByteValue(Unsafe.As<T, sbyte>(ref value));
                    break;
                default:
                    ThrowHelper.ThrowBinaryException();
                    break;
            }
        }
    }
}
