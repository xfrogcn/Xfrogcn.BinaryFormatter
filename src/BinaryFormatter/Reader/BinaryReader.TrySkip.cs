using System;

namespace Xfrogcn.BinaryFormatter
{

    public ref partial struct BinaryReader
    {


        /// <summary>
        /// 跳过节点读取
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
#pragma warning disable IDE0060 // 删除未使用的参数
        public bool TrySkip(BinarySerializerOptions options)
#pragma warning restore IDE0060 // 删除未使用的参数
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
                if (!TryReadTypeSeq(ref offset, out ushort typeSeq))
                {
                    return false;
                }

                if (typeSeq == TypeMap.NullTypeSeq)
                {
                    _consumed = offset;
                    return true;
                }

                if (TryForwardRead(_typeMap.GetTypeInfo(typeSeq), ref offset))
                {
                    _consumed = offset;
                    return true;
                }
            }
            else if (_tokenType == BinaryTokenType.TypeSeq)
            {
                int offset = _consumed;
                if (TryForwardRead(CurrentTypeInfo, ref offset))
                {
                    _consumed = offset;
                    return true;
                }
            }

            return false;
        }

        internal bool TryForwardRead(BinaryTypeInfo typeInfo, ref int offset)
        {
            if (typeInfo.SerializeType == ClassType.Value)
            {
                if (_binaryTypeToBytesLen.ContainsKey(typeInfo.Type))
                {

                    int len = _binaryTypeToBytesLen[typeInfo.Type];
                    if (!TryRequestData(offset, len))
                    {
                        return false;
                    }

                    offset += len;
                    return true;

                }
                else if (typeInfo.Type == TypeEnum.Nullable)
                {
                    int curOffset = offset;
                    if (!TryReadTypeSeq(ref curOffset, out ushort typeSeq))
                    {
                        return false;
                    }
                    if (typeSeq == TypeMap.NullTypeSeq)
                    {
                        offset = curOffset;
                        return true;
                    }
                    if (!TryForwardRead(_typeMap.GetTypeInfo(typeSeq), ref curOffset))
                    {
                        return false;
                    }

                    offset = curOffset;
                    return true;
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
                int curOffset = offset;
                // 引用、非引用
                if (!TryRequestData(curOffset, 1))
                {
                    return false;
                }

                byte _ = _buffer[curOffset++];

                if (!TryRequestData(curOffset, 4))
                {
                    return false;
                }
                curOffset += 4;

                // 读取属性
                if (!TrySkipObjectProperties(ref curOffset))
                {
                    return false;
                }

                offset = curOffset;

            }
            else if (typeInfo.SerializeType == ClassType.Enumerable)
            {
                int curOffset = offset;
                // 可枚举类型 
                // 引用、非引用
                if (!TryRequestData(curOffset, 1))
                {
                    return false;
                }

                byte refSign = _buffer[curOffset++];

                // 引用
                if (!TryRequestData(curOffset, 4))
                {
                    return false;
                }
                curOffset += 4;

                // 非引用
                if (refSign == 0x00)
                {
                    // 索引长度
                    if (!TryRequestData(curOffset, 1))
                    {
                        return false;
                    }

                    byte lenBytes = _buffer[curOffset++];
                    if (!TryRequestData(curOffset, lenBytes))
                    {
                        return false;
                    }

                    var lenSpan = _buffer.Slice(curOffset, lenBytes);
                    ulong len = GetEnumerableLength(lenSpan);
                    curOffset += lenBytes;

                    // 按顺序读取
                    ulong curIdx = 0;
                    while (curIdx < len)
                    {
                        // 读取索引
                        if (!TrySkipBytes(ref curOffset, lenBytes))
                        {
                            return false;
                        }

                        if (!TryReadTypeSeq(ref curOffset, out ushort typeSeq))
                        {
                            return false;
                        }

                        if (typeSeq == TypeMap.NullTypeSeq)
                        {
                            continue;
                        }

                        BinaryTypeInfo ti = _typeMap.GetTypeInfo(typeSeq);
                        if (!TryForwardRead(ti, ref curOffset))
                        {
                            return false;
                        }
                        curIdx++;
                    }
                }


                // 读取自定义属性
                if (!TrySkipObjectProperties(ref curOffset))
                {
                    return false;
                }

                offset = curOffset;
            }
            else if (typeInfo.SerializeType == ClassType.Dictionary)
            {
                int curOffset = offset;
                // 引用、非引用
                if (!TryRequestData(curOffset, 1))
                {
                    return false;
                }

                byte refSign = _buffer[curOffset++];

                if (!TryRequestData(curOffset, 4))
                {
                    return false;
                }
                curOffset += 4;

                if (refSign == 0x00)
                {
                    // Key Value
                    while (true)
                    {
                        if (!TryRequestData(curOffset, 1))
                        {
                            return false;
                        }

                        byte keySeq = _buffer[curOffset++];

                        if (keySeq == BinarySerializerConstants.EndDictionaryKey)
                        {
                            break;
                        }
                        else if (keySeq != BinarySerializerConstants.DictionaryKeySeq)
                        {
                            ThrowHelper.ThrowBinaryReaderException(ref this, ExceptionResource.InvalidByte);
                        }

                        // Key
                        if (!TryReadTypeSeq(ref curOffset, out ushort typeSeq))
                        {
                            return false;
                        }

                        if (typeSeq != TypeMap.NullTypeSeq)
                        {
                            BinaryTypeInfo ti = _typeMap.GetTypeInfo(typeSeq);
                            if (!TryForwardRead(ti, ref curOffset))
                            {
                                return false;
                            }
                        }

                        // Value
                        if (!TryReadTypeSeq(ref curOffset, out typeSeq))
                        {
                            return false;
                        }

                        if (typeSeq != TypeMap.NullTypeSeq)
                        {
                            BinaryTypeInfo ti = _typeMap.GetTypeInfo(typeSeq);
                            if (!TryForwardRead(ti, ref curOffset))
                            {
                                return false;
                            }
                        }
                    }

                }


                // 读取属性
                if (!TrySkipObjectProperties(ref curOffset))
                {
                    return false;
                }

                offset = curOffset;
            }
            else
            {
                ThrowHelper.ThrowBinaryException();
            }

            return true;

        }

        internal bool TrySkipObjectProperties(ref int offset)
        {
            int curOffset = offset;
            // 读取属性
            while (true)
            {
                if (!TryReadPropertySeq(ref curOffset, out ushort propertySeq))
                {
                    return false;
                }

                if (propertySeq == BinarySerializerConstants.EndObjectSeq)
                {
                    break;
                }

                // 记得处理扩展属性

                if (!TryReadTypeSeq(ref curOffset, out ushort typeSeq))
                {
                    return false;
                }

                if (typeSeq == TypeMap.NullTypeSeq)
                {
                    continue;
                }

                BinaryTypeInfo ti = _typeMap.GetTypeInfo(typeSeq);
                if (ti == null)
                {

                }
                if (!TryForwardRead(ti, ref curOffset))
                {
                    return false;
                }
            }

            offset = curOffset;
            return true;
        }
        internal bool TrySkipBytes(ref int offset, int len)
        {
            if (!TryRequestData(offset, len))
            {
                return false;
            }
            offset += len;
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

            byte b1 = _buffer[offset];

            // 如果最高位是1，表示31位长度，否则表示15位长度
            int len;
            int lenBytes;
            if ((b1 & 0x80) == 0x80)
            {
                if (!TryRequestData(offset, 4))
                {
                    return false;
                }

                len = ((_buffer[offset] & 0x7F) << 24) |
                    (_buffer[offset + 1] << 16) |
                    (_buffer[offset + 2] << 8) |
                    _buffer[offset + 3];

                lenBytes = 4;
            }
            else
            {
                len = _buffer[offset] << 8 | _buffer[offset + 1];
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


            typeSeq = BitConverter.ToUInt16(_buffer.Slice(offset, 2));
            offset += 2;
            return true;
        }
    }
}
