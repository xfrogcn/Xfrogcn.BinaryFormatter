using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    public class TypeMap
    {
        private ushort _currentSeq;
        public ushort CurrentSeq => _currentSeq;

        private readonly Dictionary<Type, ushort> _typeSeqMap = new Dictionary<Type, ushort>();
        private readonly Dictionary<ushort, Type> _seqTypeMap = new Dictionary<ushort, Type>();
        private readonly Dictionary<Type, BinaryTypeInfo> _typeInfoMap = new Dictionary<Type, BinaryTypeInfo>();

        public bool HasType([NotNull]Type type) => _typeSeqMap.ContainsKey(type);

        public BinaryTypeInfo PushType(Type type)
        {
            if (HasType(type))
            {
                return _typeInfoMap[type]; 
            }
            ushort cur = _currentSeq;
            _typeSeqMap.Add(type, cur);
            BinaryTypeInfo ti = new BinaryTypeInfo()
            {
                Seq = cur
            };
            _typeInfoMap.Add(type, ti);
            _seqTypeMap.Add(cur, type);
            _currentSeq++;
            return ti;
        } 


        public BinaryTypeInfo GetTypeInfo(ushort seq)
        {
            if (!_seqTypeMap.ContainsKey(seq))
            {
                return null;
            }

            return _typeInfoMap[_seqTypeMap[seq]];
        }


        public BinaryTypeInfo PrimaryTypeInfo
        {
            get
            {
                return GetTypeInfo(0);
            }
        }

    }
}
