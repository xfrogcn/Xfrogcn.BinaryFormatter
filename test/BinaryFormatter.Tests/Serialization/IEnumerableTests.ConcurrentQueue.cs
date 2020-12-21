using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Immutable;
using System.Collections.Concurrent;

namespace Xfrogcn.BinaryFormatter.Tests
{

    public partial class IEnumerableTests 
    {

        [InlineData(0)]
        [InlineData(127)]
        [InlineData(512)]
        [Theory(DisplayName = "Test_ConcurrentQueue_Simple_Number")]
        public async Task Test_ConcurrentQueue_Simple_Number(int len)
        {
            ConcurrentQueue<long> a = new ConcurrentQueue<long>();
            for (long i = 0; i < len; i++)
            {
                a.Enqueue(i);
            }

            

            await Test(a, CheckIEnumerableOfIEnumerable(a, (a, b) => Assert.Equal(a, b)));

        }


        [InlineData(500)]
        [InlineData(1024)]
        [InlineData(1024 * 1024)]
        [Theory(DisplayName = "Test_ConcurrentQueue_Complex_Object_Buffer")]
        public async Task Test_ConcurrentQueue_Complex_Object_Buffer(int len)
        {
            ConcurrentQueue<TestCtorA> a = new ConcurrentQueue<TestCtorA>();
            
            a.Enqueue(createComplexCtorC(len));
            a.Enqueue(null);
            a.Enqueue(createComplexCtorC(len));
            a.Enqueue(new TestCtorA(new string('A', len), 0));
            a.Enqueue(createComplexCtorC(len));
           
            await Test(a, CheckIEnumerableOfIEnumerable(a, (a, b) => checkCtorCProc(a)(b)));

        }

        class TestConcurrentQueueA : ConcurrentQueue<TestCtorA>
        {
            public string A { get; set; }
        }

        [InlineData(500)]
        [InlineData(1024)]
        [InlineData(1024 * 1024)]
        [Theory(DisplayName = "Test_ConcurrentQueue_Custom_Complex_Object_Buffer")]
        public async Task Test_ConcurrentQueue_Custom_Complex_Object_Buffer(int len)
        {
            TestQueueA a = new TestQueueA()
            {
                A = new string('A', len)
            };

            
            a.Enqueue(null);
            a.Enqueue(createComplexCtorC(len));
          

            await Test(a, (b)=>
            {
                Assert.Equal(a.A, b.A);
                CheckIEnumerableOfIEnumerable(a, (a, b) => checkCtorCProc(a)(b));
            }, new BinarySerializerOptions() { DefaultBufferSize = 1 });

        }



    }
}
