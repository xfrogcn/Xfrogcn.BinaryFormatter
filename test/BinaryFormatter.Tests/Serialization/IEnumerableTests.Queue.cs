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
        [Theory(DisplayName = "Test_Queue_Simple_Number")]
        public async Task Test_Queue_Simple_Number(int len)
        {
            Queue<long> a = new Queue<long>();
            for (long i = 0; i < len; i++)
            {
                a.Enqueue(i);
            }

            

            await Test(a, CheckIEnumerableOfIEnumerable(a, (a, b) => Assert.Equal(a, b)));

        }


        [InlineData(500)]
        [InlineData(1024)]
        [InlineData(1024 * 1024)]
        [Theory(DisplayName = "Test_Queue_Complex_Object_Buffer")]
        public async Task Test_Queue_Complex_Object_Buffer(int len)
        {
            Queue<TestCtorA> a = new Queue<TestCtorA>();
            
            a.Enqueue(createComplexCtorC(len));
            a.Enqueue(null);
            a.Enqueue(createComplexCtorC(len));
            a.Enqueue(new TestCtorA(new string('A', len), 0));
            a.Enqueue(createComplexCtorC(len));
           
            await Test(a, CheckIEnumerableOfIEnumerable(a, (a, b) => checkCtorCProc(a)(b)));

        }

        class TestQueueA : Queue<TestCtorA>
        {
            public string A { get; set; }
        }

        [InlineData(500)]
        [InlineData(1024)]
        [InlineData(1024 * 1024)]
        [Theory(DisplayName = "Test_Queue_Custom_Complex_Object_Buffer")]
        public async Task Test_Queue_Custom_Complex_Object_Buffer(int len)
        {
            TestQueueA a = new TestQueueA()
            {
                A = new string('A', len)
            };

            
            a.Enqueue(null);
          

            await Test(a, (b)=>
            {
                Assert.Equal(a.A, b.A);
                CheckIEnumerableOfIEnumerable(a, (a, b) => checkCtorCProc(a)(b));
            });

        }

        //[InlineData(500)]
        //[InlineData(1024 * 10)]
        //[Theory(DisplayName = "Test_ImmutableArray_Nest_Buffer")]
        //public async Task Test_ImmutableArray_Nest_Buffer(int len)
        //{
        //    IList<ImmutableArray<TestListA>> a1 = new List<ImmutableArray<TestListA>>()
        //    {
        //        ImmutableArray.Create(new TestListA[]
        //        {
        //            new TestListA()
        //            {
        //                createComplexCtorC(len)
        //            }
        //        })
        //    };

        //    IImmutableList<ImmutableArray<TestListA>> a = ImmutableArray.Create(a1.ToArray());

        //    await Test(a, (b)=>
        //    {
        //        Assert.Equal(a.Count, b.Count);
        //        Assert.Equal(a[0].Length, b[0].Length);
        //        checkCtorCProc(a[0][0][0])(b[0][0][0]);

        //    });

        //}


    }
}
