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
            state.TypeMap = new TypeMap(ReadTypeList());
            state.PrimaryTypeSeq = ReadUInt16Value();
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

        internal BinaryMemberInfo[] ReadMembers()
        {
            ushort memberCount = ReadUInt16Value();
            BinaryMemberInfo[] members = new BinaryMemberInfo[memberCount];
            for(ushort i = 0; i < memberCount; i++)
            {
                members[i] = ReadMemberInfo();
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
