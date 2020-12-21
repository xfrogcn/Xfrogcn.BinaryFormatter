using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{

    public partial class IEnumerableTests 
    {

        class TestCollectionOfT : ICollection<long>
        {
            public string A { get; set; }

            readonly List<long> _list = new List<long>();
            public int Count => _list.Count;

            public bool IsReadOnly => false;

            public void Add(long item)
            {
                _list.Add(item);
            }

            public void Clear()
            {
                _list.Clear();
            }

            public bool Contains(long item)
            {
                return _list.Contains(item);
            }

            public void CopyTo(long[] array, int arrayIndex)
            {
                _list.CopyTo(array, arrayIndex);
            }

            public IEnumerator<long> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            public bool Remove(long item)
            {
                return _list.Remove(item);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _list.GetEnumerator();
            }
        }

        [InlineData(0)]
        [InlineData(127)]
        [InlineData(512)]
        [Theory(DisplayName = "Test_CollectionOfT_Simple_Number")]
        public async Task Test_CollectionOfT_Simple_Number(int len)
        {
            TestCollectionOfT a1 = new TestCollectionOfT();
            
            for(long i = 0; i < len; i++)
            {
                a1.Add(i);
            }
            
            await Test(a1, (b)=>
            {
                CheckIEnumerableOfIEnumerable(a1, (a, b) => Assert.Equal(a, b));
                Assert.Equal(a1.A, b.A);
            });

        }

    }
}
