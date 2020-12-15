using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    public ref partial struct BinaryReader
    {
        public bool TrySkip(BinarySerializerOptions options)
        {
            if (_tokenType == BinaryTokenType.Null)
            {
                return true;
            }
            else if (_tokenType == BinaryTokenType.EndObject)
            {
                return true;
            }
            else if (_tokenType == BinaryTokenType.PropertyName)
            {
                int offset = _consumed;
                if(!TryReadTypeSeq(ref offset, out ushort typeSeq))
                {
                    return false;
                }

                if(typeSeq == TypeMap.NullTypeSeq)
                {
                    return true;
                }

                return TryForwardRead(_typeMap.GetTypeInfo(typeSeq), ref offset);
            }
            else if (_tokenType == BinaryTokenType.TypeSeq)
            {
                int offset = _consumed;
                return TryForwardRead(CurrentTypeInfo, ref offset);
            }

            return false;
        }

        internal bool TryForwardRead(BinaryTypeInfo typeInfo, ref int offset)
        {
            if (typeInfo.SerializeType == ClassType.Value)
            {
                if (_binaryTypeToBytesLen.ContainsKey(typeInfo.Type))
                {
                    offset += _binaryTypeToBytesLen[typeInfo.Type];
                }
                else
                {
                    if (!TrySkipBytes(ref offset))
                    {
                        return false;
                    }
                }
            }
            else if (typeInfo.SerializeType == ClassType.Object)
            {
                // 引用、非引用
                if (!TryRequestData(offset, 1))
                {
                    return false;
                }

                byte refSign = _buffer[offset++];
                if(refSign == 0xFF)
                {
                    if (!TryRequestData(offset, 4))
                    {
                        return false;
                    }
                    offset += 4;
                }

                // 读取属性
                while (true)
                {
                    if(!TryReadPropertySeq(ref offset, out ushort propertySeq))
                    {
                        return false;
                    }

                    if( propertySeq == 0x7FFF)
                    {
                        break;
                    }

                    if (!TryReadTypeSeq(ref offset, out ushort typeSeq))
                    {
                        return false;
                    }

                    if(typeSeq == TypeMap.NullTypeSeq)
                    {
                        continue;
                    }

                    BinaryTypeInfo ti = _typeMap.GetTypeInfo(typeSeq);
                    if(!TryForwardRead(ti, ref offset))
                    {
                        return false;
                    }
                }

                

            }
            else
            {
                ThrowHelper.ThrowBinaryException();
            }
            _consumed = offset;
            return true;

        }


        internal bool TryReadPropertySeq(ref int offset, out ushort propertySeq)
        {
            propertySeq = default;
            if (!TryRequestData(offset, 1))
            {
                return false;
            }

            byte b = _buffer[offset];
            if ((b & 0x80) == 0x80)
            {
                // 键值对方式
            }
            else
            {
                if (!TryRequestData(offset, 2))
                {
                    return false;
                }

                var valueSpan = _buffer.Slice(offset, 2);
                propertySeq = (ushort)((valueSpan[0] << 8) | valueSpan[1]);
 
                offset += 2;

                return true;
            }

            return false;
        }


        public bool TrySkipBytes(ref int offset)
        {
            // 长度 16位或32位
            if (!TryRequestData(offset, 2))
            {
                return false;
            }

            // 如果最高位是1，表示31位长度，否则表示15位长度
            int len = default;
            byte b1 = _buffer[offset];
            int lenBytes = 4;
            if ((b1 & 0x80) == 0x80)
            {
                if (!TryRequestData(offset, 4))
                {
                    return false;
                }

                len = ((_buffer[_consumed] & 0x7F) << 24) |
                    (_buffer[_consumed + 1] << 16) |
                    (_buffer[_consumed + 2] << 8) |
                    _buffer[_consumed + 3];

                lenBytes = 4;
            }
            else
            {
                len = _buffer[_consumed] << 8 | _buffer[_consumed + 1];
                lenBytes = 2;
            }


            if (!TryRequestData(offset, len + lenBytes))
            {
                return false;
            }

            offset += (len + lenBytes);
            return true;
        }


        private bool TryRequestData(int offset, int len)
        {
            if ((offset + len) > _buffer.Length)
            {
                return false;
            }

            return true;
        }

        internal bool TryReadTypeSeq(ref int offset, out ushort typeSeq)
        {
            typeSeq = default;
            if (!TryRequestData(offset, 2))
            {
                return false;
            }


            typeSeq = BitConverter.ToUInt16(_buffer.Slice(_consumed, 2));
            offset += 2;
            return true;
        }
    }
}
