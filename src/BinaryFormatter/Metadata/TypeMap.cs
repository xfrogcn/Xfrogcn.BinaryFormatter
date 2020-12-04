using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace Xfrogcn.BinaryFormatter
{
    public class TypeMap
    {
        public static readonly Type ObjectType = typeof(object);

        private ushort _currentSeq;
        public ushort CurrentSeq => _currentSeq;

        private readonly ConcurrentDictionary<Type, ushort> _typeSeqMap; 
        private readonly Dictionary<ushort, Type> _seqTypeMap; 
        private readonly Dictionary<Type, BinaryTypeInfo> _typeInfoMap; 

        public TypeMap()
        {
            _typeSeqMap = new ConcurrentDictionary<Type, ushort>();
            _seqTypeMap = new Dictionary<ushort, Type>();
            _typeInfoMap = new Dictionary<Type, BinaryTypeInfo>();
        }

        internal TypeMap(TypeMap other)
        {
            _typeSeqMap = new ConcurrentDictionary<Type, ushort>(other._typeSeqMap);
            _seqTypeMap = new Dictionary<ushort, Type>(other._seqTypeMap);
            _typeInfoMap = new Dictionary<Type, BinaryTypeInfo>(other._typeInfoMap);
        }

        public bool HasType([NotNull]Type type) => _typeSeqMap.ContainsKey(type);

        public bool TryAdd(Type type, out BinaryTypeInfo typeInfo )
        {
            if (HasType(type))
            {
                typeInfo = _typeInfoMap[type];
                return false;
            }

            bool isAdd = false;
            ushort seq = _typeSeqMap.GetOrAdd(type, (t) =>
            {
                ushort cur = _currentSeq;
                BinaryTypeInfo ti = new BinaryTypeInfo()
                {
                    Seq = cur
                };
                _typeInfoMap.Add(t, ti);
                _seqTypeMap.Add(cur, t);
                _currentSeq++;
                isAdd = true;
                return cur;
            });

            typeInfo = _typeInfoMap[type];
            return isAdd;
        } 

        public ushort GetTypeSeq(Type type)
        {
            TryAdd(type, out BinaryTypeInfo ti);
            return ti.Seq;
        }

        public BinaryTypeInfo GetTypeInfo(ushort seq)
        {
            if (!_seqTypeMap.ContainsKey(seq))
            {
                return null;
            }

            return _typeInfoMap[_seqTypeMap[seq]];
        }


        //public BinaryTypeInfo PrimaryTypeInfo
        //{
        //    get
        //    {
        //        return GetTypeInfo(0);
        //    }
        //}


         
    }
}
