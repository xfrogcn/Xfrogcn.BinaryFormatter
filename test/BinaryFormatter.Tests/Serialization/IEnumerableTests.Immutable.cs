using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Immutable;

namespace Xfrogcn.BinaryFormatter.Tests
{

    public partial class IEnumerableTests 
    {

        [InlineData(0)]
        [InlineData(127)]
        [InlineData(512)]
        [Theory(DisplayName = "Test_ImmutableArray_Simple_Number")]
        public async Task Test_ImmutableArray_Simple_Number(int len)
        {
            long[] a1 = new long[len];

            for (long i = 0; i < len; i++)
            {
                a1[i] = i;
            }

            var a = ImmutableArray.Create<long>(a1);

            await Test(a, CheckIEnumerableOfIEnumerable(a, (a, b) => Assert.Equal(a, b)));

        }

        [InlineData(0)]
        [InlineData(127)]
        [Theory(DisplayName = "Test_ImmutableList_Simple_Number")]
        public async Task Test_ImmutableList_Simple_Number(int len)
        {
            long[] a1 = new long[len];

            for (long i = 0; i < len; i++)
            {
                a1[i] = i;
            }

            var a = ImmutableList.Create<long>(a1);

            await Test(a, CheckIEnumerableOfIEnumerable(a, (a, b) => Assert.Equal(a, b)));

        }


        [InlineData(0)]
        [InlineData(127)]
        [Theory(DisplayName = "Test_ImmutableStack_Simple_Number")]
        public async Task Test_ImmutableStack_Simple_Number(int len)
        {
            long[] a1 = new long[len];

            for (long i = 0; i < len; i++)
            {
                a1[i] = i;
            }

            var a = ImmutableStack.Create(a1);

            await Test(a, CheckIEnumerableOfIEnumerable(a, (a, b) => Assert.Equal(a, b)));

        }

        [InlineData(0)]
        [InlineData(127)]
        [Theory(DisplayName = "Test_ImmutableHashSet_Simple_Number")]
        public async Task Test_ImmutableHashSet_Simple_Number(int len)
        {
            long[] a1 = new long[len];

            for (long i = 0; i < len; i++)
            {
                a1[i] = i;
            }

            var a = ImmutableHashSet.Create(a1);

            await Test(a, CheckIEnumerableOfIEnumerable(a, (a, b) => Assert.Equal(a, b)));

        }

        [InlineData(0)]
        [InlineData(127)]
        [Theory(DisplayName = "Test_ImmutableSortedSet_Simple_Number")]
        public async Task Test_ImmutableSortedSet_Simple_Number(int len)
        {
            long[] a1 = new long[len];

            for (long i = len-1; i >= 0; i--)
            {
                a1[i] = i;
            }

            var a = ImmutableSortedSet.Create(a1);

            await Test(a, CheckIEnumerableOfIEnumerable(a, (a, b) => Assert.Equal(a, b)));

        }

        [InlineData(0)]
        [InlineData(127)]
        [Theory(DisplayName = "Test_ImmutableQueue_Simple_Number")]
        public async Task Test_ImmutableQueue_Simple_Number(int len)
        {
            long[] a1 = new long[len];

            for (long i = len - 1; i >= 0; i--)
            {
                a1[i] = i;
            }

            var a = ImmutableQueue.Create(a1);

            await Test(a, CheckIEnumerableOfIEnumerable(a, (a, b) => Assert.Equal(a, b)));

        }

        [InlineData(500)]
        [InlineData(1024)]
        [InlineData(1024 * 1024)]
        [Theory(DisplayName = "Test_ImmutableArray_Complex_Object_Buffer")]
        public async Task Test_ImmutableArray_Complex_Object_Buffer(int len)
        {
            List<TestCtorA> a1 = new List<TestCtorA>()
            {
                createComplexCtorC(len),
                null,
                createComplexCtorC(len),
                new TestCtorA(new string('A', len),0),
                createComplexCtorC(len)
            };

            var a = ImmutableArray.Create(a1.ToArray());

            await Test(a, CheckIEnumerableOfIEnumerable(a, (a, b) => checkCtorCProc(a)(b)), new BinarySerializerOptions() { DefaultBufferSize = 1 });

        }

        [InlineData(500)]
        [InlineData(1024 * 10)]
        [InlineData(1024 * 1024)]
        [Theory(DisplayName = "Test_ImmutableArray_Nest_Buffer")]
        public async Task Test_ImmutableArray_Nest_Buffer(int len)
        {
            IList<ImmutableArray<TestListA>> a1 = new List<ImmutableArray<TestListA>>()
            {
                ImmutableArray.Create(new TestListA[]
                {
                    new TestListA()
                    {
                        createComplexCtorC(len)
                    }
                })
            };

            IImmutableList<ImmutableArray<TestListA>> a = ImmutableArray.Create(a1.ToArray());

            await Test(a, (b)=>
            {
                Assert.Equal(a.Count, b.Count);
                Assert.Equal(a[0].Length, b[0].Length);
                checkCtorCProc(a[0][0][0])(b[0][0][0]);

            }, new BinarySerializerOptions() { DefaultBufferSize = 1 });

        }

        protected virtual Action<IEnumerable<TElement>> CheckIEnumerableOfIEnumerable<TElement>(IEnumerable<TElement> a,  Action<TElement,TElement> checker )
        {
            return (b) =>
            {
                Assert.Equal(a.Count(), b.Count());
                Assert.Equal(a.GetType(), b.GetType());
                var e1 = a.GetEnumerator();
                var e2 = b.GetEnumerator();
                while(e1.MoveNext() && e2.MoveNext())
                {
                    var a1 = e1.Current;
                    var b1 = e2.Current;
                    if(a1 == null)
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
