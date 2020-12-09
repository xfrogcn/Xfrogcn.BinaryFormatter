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
        private readonly Dictionary<ushort, BinaryTypeInfo> _typeSeqToTypeInfoMap;

        internal const ushort NullTypeSeq = 0xFFFF;

        public TypeMap()
        {
            _typeSeqMap = new ConcurrentDictionary<Type, ushort>();
            _seqTypeMap = new Dictionary<ushort, Type>();
            _typeInfoMap = new Dictionary<Type, BinaryTypeInfo>();
            _typeSeqToTypeInfoMap = new Dictionary<ushort, BinaryTypeInfo>();
        }

        internal TypeMap(BinaryTypeInfo[] typeList)
        {
            _typeSeqToTypeInfoMap = new Dictionary<ushort, BinaryTypeInfo>(typeList.Select(ti => new KeyValuePair<ushort, BinaryTypeInfo>(ti.Seq, ti)));
            _typeSeqMap = new ConcurrentDictionary<Type, ushort>();
            _seqTypeMap = new Dictionary<ushort, Type>(typeList.Length);
            _typeInfoMap = new Dictionary<Type, BinaryTypeInfo>(typeList.Length);
        }

        internal void ResolveTypes(TypeResolver resolver)
        {
            foreach (var kv in _typeSeqToTypeInfoMap)
            {
                var ti = kv.Value;
                if (resolver.TryResolveType(this, ti, out Type t) && t!=null)
                {
                    _typeSeqMap.TryAdd(t, kv.Key);
                    _seqTypeMap.Add(kv.Key, t);
                    _typeInfoMap.Add(t, kv.Value);
                }
            }
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
                _typeSeqToTypeInfoMap.Add(cur, ti);
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

        public Type GetType(ushort seq)
        {
            if (_seqTypeMap.ContainsKey(seq))
            {
                return _seqTypeMap[seq];
            }

            return null;
        }

        public BinaryTypeInfo GetTypeInfo(ushort seq)
        {
            if (!_typeSeqToTypeInfoMap.ContainsKey(seq))
            {
                return null;
            }

            return _typeSeqToTypeInfoMap[seq];
        }

        internal bool TrySetTypeMemberInfos(ushort seq, Func<BinaryMemberInfo[]> getMemberInfos)
        {
            BinaryTypeInfo ti = GetTypeInfo(seq);
            if(ti !=null && ti.MemberInfos == null)
            {
                ti.MemberInfos = getMemberInfos();
                return true;
            }
            return false;
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
