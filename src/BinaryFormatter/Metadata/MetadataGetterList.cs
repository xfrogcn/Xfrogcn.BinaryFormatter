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

        private void InitDefaltMetadataGetter()
        {
            // 默认内置处理器
            _list.Add(new Metadata.Internal.NumbericGetter());
            _list.Add(new Metadata.Internal.DateTimeGetter());
            _list.Add(new Metadata.Internal.BaseTypeGetter());
            _list.Add(new Metadata.Internal.ValueTupleGetter());
            _list.Add(new Metadata.Internal.TupleGetter());
            _list.Add(new Metadata.Internal.NullableTypeGetter());
            _list.Add(new Metadata.Internal.ArrayTypeGetter());
            _list.Add(new Metadata.Internal.ListTypeGetter());
            _list.Add(new Metadata.Internal.DictionaryTypeGetter());
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
