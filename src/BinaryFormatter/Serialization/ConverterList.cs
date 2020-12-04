using System;
using System.Collections;
using System.Collections.Generic;

namespace Xfrogcn.BinaryFormatter.Serialization
{
    /// <summary>
    /// A list of BinaryConverters that respects the options class being immuttable once (de)serialization occurs.
    /// </summary>
    internal sealed class ConverterList : IList<BinaryConverter>
    {
        private readonly List<BinaryConverter> _list = new List<BinaryConverter>();
        private readonly BinarySerializerOptions _options;

        public ConverterList(BinarySerializerOptions options)
        {
            _options = options;
        }

        public ConverterList(BinarySerializerOptions options, ConverterList source)
        {
            _options = options;
            _list = new List<BinaryConverter>(source._list);
        }

        public BinaryConverter this[int index]
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

        public void Add(BinaryConverter item)
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

        public bool Contains(BinaryConverter item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(BinaryConverter[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<BinaryConverter> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public int IndexOf(BinaryConverter item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, BinaryConverter item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _options.VerifyMutable();
            _list.Insert(index, item);
        }

        public bool Remove(BinaryConverter item)
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
