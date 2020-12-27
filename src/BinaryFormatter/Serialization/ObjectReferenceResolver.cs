using System.Collections.Generic;
using System.Linq;
using ReferenceResolverCallback = System.ValueTuple<object, object, System.Action<object, object>>;

namespace Xfrogcn.BinaryFormatter.Serialization
{
    public class ObjectReferenceResolver : ReferenceResolver
    {
        private uint _referenceCount;



        class RefItem
        {
            public object Value { get; set; }

            public int RefCount { get; set; }

            public ulong Offset { get; set; }

            public uint Seq { get; set; }

            public RefState State { get; set; }
        }

        private readonly Dictionary<uint, RefItem> _referenceIdToObjectMap;
        private readonly Dictionary<object, RefItem> _objectToReferenceIdMap;
        // 回调 所需引用
        private readonly List<ReferenceResolverCallback> ResolverCallback = new List<ReferenceResolverCallback>();

        public ObjectReferenceResolver()
        {
            _referenceIdToObjectMap = new Dictionary<uint, RefItem>();
            _objectToReferenceIdMap = new Dictionary<object, RefItem>();
        }

        public override uint GetReference(object value, ulong offset, out bool alreadyExists)
        {
            if (_objectToReferenceIdMap.ContainsKey(value))
            {
                alreadyExists = true;
                RefItem item = _objectToReferenceIdMap[value];
                item.RefCount++;
                //if (item.Seq == 0)
                //{
                //    item.Seq = _referenceCount;
                //    _referenceCount++;
                //}
                return item.Seq;
            }
            else
            {
              
                RefItem item = new RefItem()
                {
                    RefCount = 0,
                    Value = value,
                    Seq = _referenceCount,
                    Offset = offset
                };
               
                // _referenceIdToObjectMap.Add(seq, item);
                _objectToReferenceIdMap.Add(value, item);
                _referenceCount++;
                alreadyExists = false;
                return item.Seq;
            }
            
        }

        public override void AddReference(uint seq)
        {
            RefItem ri = null;
            _referenceIdToObjectMap.TryGetValue(seq, out ri);
            if (ri == null)
            {
                ri = new RefItem()
                {
                    Seq = seq,
                    State = RefState.Start,
                    RefCount = 0
                };
                _referenceIdToObjectMap.Add(seq, ri);
            }
           
        }

        public override Dictionary<uint, ulong> GetReferenceOffsetMap()
        {
            return new Dictionary<uint, ulong>(_objectToReferenceIdMap.Where(kv => kv.Value.RefCount > 1).Select(kv => new KeyValuePair<uint, ulong>(kv.Value.Seq, kv.Value.Offset)));
        }

        public override RefState TryGetReference(uint seq, out object value)
        {
            RefItem ri = _referenceIdToObjectMap[seq];
            if (ri == null)
            {
                value = default;
                return RefState.None;
            }
            value = ri.Value;
            return ri.State;
        }

        public override void AddReferenceObject(uint seq, object value)
        {
            RefItem ri = _referenceIdToObjectMap[seq];
            ri.State = RefState.Created;
            ri.Value = value;
        }
    }
}
