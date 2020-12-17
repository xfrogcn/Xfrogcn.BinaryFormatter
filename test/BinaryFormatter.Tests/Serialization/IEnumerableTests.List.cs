using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
   
    public partial class IEnumerableTests 
    {
        class TestList : IList
        {
            private readonly IList<object> _list = new List<object>();

            public object this[int index]
            {
                get => _list[index];
                set => _list[index] = value;
            }

            public bool IsFixedSize => false;

            public bool IsReadOnly => false;

            public int Count => _list.Count;

            public bool IsSynchronized => false;

            public object SyncRoot => new object();

            public int Add(object value)
            {
                _list.Add(value);
                return _list.Count;
            }

            public void Clear()
            {
                _list.Clear();
            }

            public bool Contains(object value)
            {
                return _list.Contains(value as TestCtorA);
            }

            public void CopyTo(Array array, int index)
            {
               
            }

            public IEnumerator GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            public int IndexOf(object value)
            {
                return _list.IndexOf(value);
            }

            public void Insert(int index, object value)
            {
                _list.Insert(index, value);
            }

            public void Remove(object value)
            {
                _list.Remove(value);
            }

            public void RemoveAt(int index)
            {
                _list.RemoveAt(index);
            }
        }


        [Fact(DisplayName = "Test_IList_Custom")]
        public async Task Test_IList_Custom()
        {
            TestList list = new TestList()
            {
                createComplexCtorC(1),
                new object(),
                "A",
                1,
                null
            };

            await Test(list, (b) =>
            {
                checkCtorCProc(list[0] as TestCtorA)(b[0] as TestCtorA);
                Assert.Equal(typeof(object), b[1].GetType());
                Assert.Equal(list[2], b[2]);
                Assert.Equal(list[3], b[3]);
                Assert.Null(b[4]);
            });
            
        }
    }
}
