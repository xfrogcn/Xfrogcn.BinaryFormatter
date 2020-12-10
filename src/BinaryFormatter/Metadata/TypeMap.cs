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

        private int _currentSeq;
        public ushort CurrentSeq => (ushort)_currentSeq;

        private readonly Dictionary<Type, ushort> _typeSeqMap; 
        private readonly Dictionary<ushort, Type> _seqTypeMap; 
        private readonly Dictionary<Type, BinaryTypeInfo> _typeInfoMap;
        private readonly Dictionary<ushort, BinaryTypeInfo> _typeSeqToTypeInfoMap;

        internal const ushort NullTypeSeq = 0xFFFF;

        public TypeMap()
        {
            _typeSeqMap = new Dictionary<Type, ushort>();
            _seqTypeMap = new Dictionary<ushort, Type>();
            _typeInfoMap = new Dictionary<Type, BinaryTypeInfo>();
            _typeSeqToTypeInfoMap = new Dictionary<ushort, BinaryTypeInfo>();
        }

        internal TypeMap(BinaryTypeInfo[] typeList)
        {
            _typeSeqToTypeInfoMap = new Dictionary<ushort, BinaryTypeInfo>(typeList.Select(ti => new KeyValuePair<ushort, BinaryTypeInfo>(ti.Seq, ti)));
            _typeSeqMap = new Dictionary<Type, ushort>();
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

            lock (_typeSeqMap)
            {
                if (HasType(type))
                {
                    typeInfo = _typeInfoMap[type];
                    return false;
                }

                int s = System.Threading.Interlocked.Increment(ref _currentSeq);
                
                ushort cur = (ushort)_currentSeq;
                if (cur >= 0xFF)
                {
                    // OXFF 固定为Null值类型
                    // TODO 
                    throw new Exception();
                }
                _currentSeq++;
                BinaryTypeInfo ti = new BinaryTypeInfo()
                {
                    Seq = cur
                };
                _typeSeqMap.Add(type, cur);
                _typeInfoMap.Add(type, ti);
                _seqTypeMap.Add(cur, type);
                _typeSeqToTypeInfoMap.Add(cur, ti);

                typeInfo = ti;
                return true;
            }
            

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
        internal string GetTypeName(ushort seq)
        {
            BinaryTypeInfo ti = GetTypeInfo(seq);
            if(ti == null)
            {
                return null;
            }

            return ti.GetFullName(this);
        }


    }
}
