using System.Collections.Generic;

namespace Xfrogcn.BinaryFormatter
{
    public class ObjectReferenceResolver
    {
        private int _referenceCount;

        private readonly Dictionary<int, object> _referenceIdToObjectMap;
        private readonly Dictionary<object, int> _objectToReferenceIdMap;

        public ObjectReferenceResolver()
        {
            _referenceIdToObjectMap = new Dictionary<int, object>();
            _objectToReferenceIdMap = new Dictionary<object, int>();
        }

        public virtual int AddReference(object value)
        {
            if (_objectToReferenceIdMap.ContainsKey(value))
            {
                return _objectToReferenceIdMap[value];
            }
            else
            {
                int seq = _referenceCount;
                _referenceIdToObjectMap.Add(seq, value);
                _objectToReferenceIdMap.Add(value, seq);
                _referenceCount++;
                return seq;
            }
            
        }
    }
}
