using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Immutable;
using System.Collections.Concurrent;
using System.Collections;

namespace Xfrogcn.BinaryFormatter.Tests
{

    public partial class IEnumerableTests
    {

        /// <summary>
        /// 如果从IEnumerable<>接口实现集合类型，必须提供Add方法及Count/Length属性/字段
        /// </summary>
        class TestEnumerableA : IEnumerable
        {
            protected readonly List<TestCtorA> _list = new List<TestCtorA>();



            public IEnumerator<TestCtorA> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            public string A { get; set; }
        }


        [Fact(DisplayName = "Test_IEnumerable_Custom_Exception")]
        public async Task Test_IEnumerable_Custom_Exception()
        {
            TestEnumerableA a = new TestEnumerableA();


            await Assert.ThrowsAsync<NotSupportedException>(async () =>
            {
                await Test(a, b =>
                {

                });
            });


        }

        class TestEnumerableB : TestEnumerableA
        {
            public void Add(TestCtorA item)
            {
                _list.Add(item);
            }

            public int Count => _list.Count;
        }


        [InlineData(0)]
        [InlineData(127)]
        [InlineData(512)]
        [Theory(DisplayName = "Test_IEnumerable_Custom_Buffer")]
        public async Task Test_IEnumerable_Custom_Buffer(int len)
        {
            TestEnumerableB a = new TestEnumerableB();

            a.A = new string('A', len);
            a.Add(createComplexCtorC(len));
            a.Add(null);


            await Test(a, b =>
            {
                CheckIEnumerableOfIEnumerable(a, (a, b) =>
                {
                    if (a == null)
                    {
                        Assert.Null(b);
                    }
                    else if (a is TestCtorA ta)
                    {
                        checkCtorCProc(ta)(b as TestCtorA);
                    }
                });
                Assert.Equal(a.A, b.A);
            });

        }


    
        [Fact(DisplayName = "Test_IEnumerable_ArrayList")]
        public async Task Test_IEnumerable_ArrayList()
        {
            ArrayList a = new ArrayList();
            a.Add(createComplexCtorC(1));
            a.Add(null);
            await Test(a, CheckIEnumerableOfIEnumerable(a, (a, b) =>
            {
                if (a == null)
                {
                    Assert.Null(b);
                }
                else if (a is TestCtorA ta)
                {
                    checkCtorCProc(ta)(b as TestCtorA);
                }
            }));

        }

        [Fact(DisplayName = "Test_IEnumerable_BitArray ")]
        public async Task Test_IEnumerable_BitArray()
        {
            BitArray a = new BitArray(new bool[]{
                true,
                false,
                true,
                true,
                false
            });

            await Test(a, CheckIEnumerableOfIEnumerable(a, (a, b) =>
            {
                Assert.Equal(a, b);
            }));

        }

        [Fact(DisplayName = "Test_IEnumerable_Queue ")]
        public async Task Test_IEnumerable_Queue()
        {
            Queue a = new Queue();
            a.Enqueue(1L);
            a.Enqueue(2L);
            await Test(a, CheckIEnumerableOfIEnumerable(a, (a, b) =>
            {
                if (a == null)
                {
                    Assert.Null(b);
                }
                else if (a is TestCtorA ta)
                {
                    checkCtorCProc(ta)(b as TestCtorA);
                }
            }));

        }

        [Fact(DisplayName = "Test_IEnumerable_Stack ")]
        public async Task Test_IEnumerable_Stack()
        {
            Stack a = new Stack();
            a.Push(1L);
            a.Push(2L);
            await Test(a, CheckIEnumerableOfIEnumerable(a, (a, b) =>
            {
                if (a == null)
                {
                    Assert.Null(b);
                }
                else if (a is TestCtorA ta)
                {
                    checkCtorCProc(ta)(b as TestCtorA);
                }
            }));

        }

        protected virtual Action<IEnumerable> CheckIEnumerableOfIEnumerable(IEnumerable a, Action<object, object> checker)
        {
            return (b) =>
            {
                Assert.Equal(a.GetType(), b.GetType());
                var e1 = a.GetEnumerator();
                var e2 = b.GetEnumerator();
                while (e1.MoveNext() && e2.MoveNext())
                {
                    var a1 = e1.Current;
                    var b1 = e2.Current;
                    if (a1 == null)
                    {
                        Assert.Null(b1);
                    }
                    else
                    {
                        Assert.Equal(a1.GetType(), b1.GetType());
                        checker(a1, b1);
                    }
                }
            };

        }
    }
}
