﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

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
            }, new BinarySerializerOptions() { DefaultBufferSize = 1 });

        }



    }
}
