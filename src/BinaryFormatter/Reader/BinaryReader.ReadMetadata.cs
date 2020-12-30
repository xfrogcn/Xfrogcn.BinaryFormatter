using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    /// <summary>
    /// 读取序列化元数据
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public ref partial struct BinaryReader
    {
        // private ReadOnlySpan<byte> _metadataData;
        //public BinaryMetadataReader(ReadOnlySpan<byte> metadataData)
        //{
        //    _metadataData = metadataData;
        //}

        internal void ReadMetadata(ref ReadStack state)
        {
            while (true)
            {
                byte blockType = ReadByteValue();
                if( blockType == BinarySerializerConstants.MetadataBlock_End)
                {
                    break;
                }

                switch (blockType)
                {
                    case BinarySerializerConstants.MetadataBlock_TypeInfo:
                        {
                            state.TypeMap = new TypeMap(ReadTypeList());
                            state.PrimaryTypeSeq = ReadUInt16Value();
                            break;
                        }
                    case BinarySerializerConstants.MetadataBlock_RefMap:
                        {
                            state.RefMap = ReadRefMap();
                            break;
                        }
                    default:
                        break;
                }

            }
            
            
        }

        internal Dictionary<uint, ulong> ReadRefMap()
        {
            uint count = ReadUInt16Value();
            var map = new Dictionary<uint, ulong>();
            if (count == 0)
            {
                return map;
            }

            for(int i = 0; i < count; i++)
            {
                uint k = ReadUInt32Value();
                ulong v = ReadUInt64Value();
                map.Add(k, v);
            }
            return map;
        }

        internal BinaryTypeInfo[] ReadTypeList()
        {
            ushort typeLen = ReadUInt16Value();
            BinaryTypeInfo[] typeList = new BinaryTypeInfo[typeLen];
            for(ushort i = 0; i < typeLen; i++)
            {
                typeList[i] = ReadTypeInfo();
            }
            return typeList;
        }

        private BinaryTypeInfo ReadTypeInfo()
        {
            // ushort 
            ushort seq = ReadUInt16Value();
            ClassType sType = (ClassType)ReadByteValue();
            bool isGeneric = ReadBooleanValue();
            byte genericArgumentCount = ReadByteValue();
            ushort[] genericArguments = new ushort[genericArgumentCount];
            for(byte i = 0; i < genericArgumentCount; i++)
            {
                genericArguments[i] = ReadUInt16Value();
            }

            TypeEnum type = (TypeEnum)ReadByteValue();
            ushort nameLen = ReadUInt16Value();

            string name = null;
            if (nameLen > 0)
            {
                name = ReadStringValue(nameLen);
            }

            BinaryTypeInfo ti = new BinaryTypeInfo()
            {
                Seq = seq,
                SerializeType = sType,
                IsGeneric = isGeneric,
                GenericArgumentCount = genericArgumentCount,
                GenericArguments = genericArguments,
                FullName = name,
                Type = type
            };

            ti.MemberInfos = ReadMembers();

            return ti;

        }

        internal Dictionary<ushort, BinaryMemberInfo> ReadMembers()
        {
            ushort memberCount = ReadUInt16Value();
            Dictionary<ushort, BinaryMemberInfo> members = new Dictionary<ushort, BinaryMemberInfo>();
            for(ushort i = 0; i < memberCount; i++)
            {
                var mi = ReadMemberInfo();
                members.Add(mi.Seq, mi);
            }
            return members;
        }


        private BinaryMemberInfo ReadMemberInfo()
        {
            ushort seq = ReadUInt16Value();
            ushort nameLen = ReadUInt16Value();
            byte[] nameAsUtf8Bytes = null;
            string nameAsString = null;
            if (nameLen > 0)
            {
                if(ReadBytes(nameLen, out ReadOnlySpan<byte> nameData))
                {
                    nameAsUtf8Bytes = nameData.ToArray();
                    nameAsString = Encoding.UTF8.GetString(nameData);
                }
            }
            ushort typeSeq = ReadUInt16Value();

            return new BinaryMemberInfo()
            {
                Seq = seq,
                NameAsString = nameAsString,
                NameAsUtf8Bytes = nameAsUtf8Bytes,
                TypeSeq = typeSeq
            };
        }


    }
}
