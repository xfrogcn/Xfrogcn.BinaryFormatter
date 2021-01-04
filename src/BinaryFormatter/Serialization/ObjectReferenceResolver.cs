using System;
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
        private readonly Dictionary<uint, List<Func<bool>>> _refCallback;

        public ObjectReferenceResolver()
        {
            _referenceIdToObjectMap = new Dictionary<uint, RefItem>();
            _objectToReferenceIdMap = new Dictionary<object, RefItem>();
            _refCallback = new Dictionary<uint, List<Func<bool>>>();
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
            _referenceIdToObjectMap.TryGetValue(seq, out RefItem ri);
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
            return new Dictionary<uint, ulong>(_objectToReferenceIdMap.Where(kv => kv.Value.RefCount > 0).Select(kv => new KeyValuePair<uint, ulong>(kv.Value.Seq, kv.Value.Offset)));
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
            if(_refCallback.ContainsKey(seq))
            {
                List<Func<bool>> callback = _refCallback[seq];
                for(int i = 0; i < callback.Count; i++)
                {
                    callback[i]();
                }
            }
        }

        public override bool AddReferenceCallback(object instance, object propertyValue, Func<object, object, bool> action)
        {
            if(instance.IsRefId() || propertyValue.IsRefId())
            {
                uint? insRefId = null;
                uint? propRefId = null;
                if( instance is ReferenceID insId)
                {
                    RefState s = TryGetReference(insId.RefSeq, out object tmpObj);
                    if (s == RefState.Created)
                    {
                        instance = tmpObj;
                    }
                    else
                    {
                        insRefId = insId.RefSeq;
                    }
                }
                if(propertyValue is ReferenceID propId)
                {
                    RefState s = TryGetReference(propId.RefSeq, out object tmpObj);
                    if (s == RefState.Created)
                    {
                        propertyValue = tmpObj;
                    }
                    else
                    {
                        propRefId = propId.RefSeq;
                    }
                }

                if( !insRefId.HasValue && !propRefId.HasValue)
                {
                    return action(instance, propertyValue);
                }

                bool callback()
                {
                    object actualInstance;
                    object actualPropValue;
                    if (instance is ReferenceID insId)
                    {
                        RefState s = TryGetReference(insId.RefSeq, out actualInstance);
                        if (s != RefState.Created)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        actualInstance = instance;
                    }

                    if (propertyValue is ReferenceID propId)
                    {
                        RefState s = TryGetReference(propId.RefSeq, out actualPropValue);
                        if (s != RefState.Created)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        actualPropValue = propertyValue;
                    }

                    return action(actualInstance, actualPropValue);
                }

                if ( insRefId.HasValue)
                {
                    AddReferenceCallback(insRefId.Value, callback);
                }
                if( propRefId.HasValue )
                {
                    AddReferenceCallback(propRefId.Value, callback);
                }

                return true;
            }


            return action(instance, propertyValue);
        }

        protected virtual void AddReferenceCallback(uint seq, Func<bool> action)
        {
            List<Func<bool>> callback;
            if (_refCallback.ContainsKey(seq))
            {
                callback = _refCallback[seq];
            }
            else
            {
                callback = new List<Func<bool>>();
                _refCallback[seq] = callback;
            }

            callback.Add(action);
        }
    }
}
