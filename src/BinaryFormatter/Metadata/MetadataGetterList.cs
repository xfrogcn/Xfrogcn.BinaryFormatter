using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Xfrogcn.BinaryFormatter.Serialization;

namespace Xfrogcn.BinaryFormatter
{
    internal sealed class MetadataGetterList : IList<IMetadataGetter>
    {
        private readonly List<IMetadataGetter> _list = new List<IMetadataGetter>();
        private readonly BinarySerializerOptions _options;

        public MetadataGetterList(BinarySerializerOptions options)
        {
            _options = options;
        }

        public MetadataGetterList(BinarySerializerOptions options, MetadataGetterList source)
        {
            _options = options;
            _list = new List<IMetadataGetter>(source._list);
        }

        private IList<IMetadataGetter> DefaultMetadataGetter()
        {
            List<IMetadataGetter> list = new List<IMetadataGetter>();
            // 默认内置处理器
            //list.Add(new Metadata.Internal.NumbericGetter());
            //list.Add(new Metadata.Internal.DateTimeGetter());
            //list.Add(new Metadata.Internal.BaseTypeGetter());
            //list.Add(new Metadata.Internal.ValueTupleGetter());
            //list.Add(new Metadata.Internal.TupleGetter());
            //list.Add(new Metadata.Internal.NullableTypeGetter());
            //list.Add(new Metadata.Internal.ArrayTypeGetter());
            //list.Add(new Metadata.Internal.ListTypeGetter());
            //list.Add(new Metadata.Internal.DictionaryTypeGetter());
            //list.Add(new Metadata.Internal.ObjectTypeGetter());
            return list;
        }

        internal void InitMetadataGetterList()
        {
            _list.InsertRange(0, DefaultMetadataGetter());
            //_list.Add(new Metadata.Internal.StructTypeGetter());
            //_list.Add(new Metadata.Internal.ClassTypeGetter());
        }

        public IMetadataGetter this[int index]
        {
            get
            {
                return _list[index];
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _options.VerifyMutable();
                _list[index] = value;
            }
        }

        public int Count => _list.Count;

        public bool IsReadOnly => false;

        public void Add(IMetadataGetter item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _options.VerifyMutable();
            _list.Add(item);
        }

        public void Clear()
        {
            _options.VerifyMutable();
            _list.Clear();
        }

        public bool Contains(IMetadataGetter item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(IMetadataGetter[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<IMetadataGetter> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public int IndexOf(IMetadataGetter item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, IMetadataGetter item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _options.VerifyMutable();
            _list.Insert(index, item);
        }

        public bool Remove(IMetadataGetter item)
        {
            _options.VerifyMutable();
            return _list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _options.VerifyMutable();
            _list.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }
    }
}
