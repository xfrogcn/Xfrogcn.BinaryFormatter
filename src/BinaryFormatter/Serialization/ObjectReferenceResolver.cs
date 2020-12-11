using System.Collections.Generic;
using System.Linq;

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
        }

      //  private readonly Dictionary<uint, RefItem> _referenceIdToObjectMap;
        private readonly Dictionary<object, RefItem> _objectToReferenceIdMap;

        

        public ObjectReferenceResolver()
        {
        //    _referenceIdToObjectMap = new Dictionary<uint, RefItem>();
            _objectToReferenceIdMap = new Dictionary<object, RefItem>();
        }

        public override uint GetReference(object value, ulong offset, out bool alreadyExists)
        {
            if (_objectToReferenceIdMap.ContainsKey(value))
            {
                alreadyExists = true;
                RefItem item = _objectToReferenceIdMap[value];
                item.RefCount++;
                if (item.Seq == 0)
                {
                    item.Seq = _referenceCount;
                    _referenceCount++;
                }
                return item.Seq;
            }
            else
            {
              
                RefItem item = new RefItem()
                {
                    RefCount = 0,
                    Value = value,
                    Seq = 0,
                    Offset = offset
                };
               // _referenceIdToObjectMap.Add(seq, item);
                _objectToReferenceIdMap.Add(value, item);
                alreadyExists = false;
                return 0;
            }
            
        }

        public override void AddReference(uint seq, ulong offset)
        {
            throw new System.NotImplementedException();
        }

        public override Dictionary<uint, ulong> GetReferenceOffsetMap()
        {
            return new Dictionary<uint, ulong>(_objectToReferenceIdMap.Where(kv => kv.Value.RefCount > 1).Select(kv => new KeyValuePair<uint, ulong>(kv.Value.Seq, kv.Value.Offset)));
        }
    }
}
