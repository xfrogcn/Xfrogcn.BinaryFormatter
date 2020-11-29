using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    public class TypeMap
    {
        private ushort _currentSeq;
        public ushort CurrentSeq => _currentSeq;

        private readonly Dictionary<Type, ushort> _typeSeqMap; 
        private readonly Dictionary<ushort, Type> _seqTypeMap; 
        private readonly Dictionary<Type, BinaryTypeInfo> _typeInfoMap; 

        public TypeMap()
        {
            _typeSeqMap = new Dictionary<Type, ushort>();
            _seqTypeMap = new Dictionary<ushort, Type>();
            _typeInfoMap = new Dictionary<Type, BinaryTypeInfo>();
        }

        internal TypeMap(TypeMap other)
        {
            _typeSeqMap = new Dictionary<Type, ushort>(other._typeSeqMap);
            _seqTypeMap = new Dictionary<ushort, Type>(other._seqTypeMap);
            _typeInfoMap = new Dictionary<Type, BinaryTypeInfo>(other._typeInfoMap);
        }

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
