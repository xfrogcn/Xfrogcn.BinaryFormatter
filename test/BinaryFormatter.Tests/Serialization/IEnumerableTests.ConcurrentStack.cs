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
        [Theory(DisplayName = "Test_ConcurrentStack_Simple_Number")]
        public async Task Test_ConcurrentStack_Simple_Number(int len)
        {
            ConcurrentStack<long> a = new ConcurrentStack<long>();
            for (long i = 0; i < len; i++)
            {
                a.Push(i);
            }

            

            await Test(a, CheckIEnumerableOfIEnumerable(a, (a, b) => Assert.Equal(a, b)));

        }


        [InlineData(500)]
        [InlineData(1024)]
        [InlineData(1024 * 1024)]
        [Theory(DisplayName = "Test_ConcurrentStack_Complex_Object_Buffer")]
        public async Task Test_ConcurrentStack_Complex_Object_Buffer(int len)
        {
            ConcurrentStack<TestCtorA> a = new ConcurrentStack<TestCtorA>();
            
            a.Push(createComplexCtorC(len));
            a.Push(null);
            a.Push(createComplexCtorC(len));
            a.Push(new TestCtorA(new string('A', len), 0));
            a.Push(createComplexCtorC(len));
           
            await Test(a, CheckIEnumerableOfIEnumerable(a, (a, b) => checkCtorCProc(a)(b)), new BinarySerializerOptions() { DefaultBufferSize = 1 });

        }

        class TestConcurrentStackA : ConcurrentStack<TestCtorA>
        {
            public string A { get; set; }
        }

        [InlineData(500)]
        [InlineData(1024)]
        [InlineData(1024 * 1024)]
        [Theory(DisplayName = "Test_ConcurrentStack_Custom_Complex_Object_Buffer")]
        public async Task Test_ConcurrentStack_Custom_Complex_Object_Buffer(int len)
        {
            TestConcurrentStackA a = new TestConcurrentStackA()
            {
                A = new string('A', len)
            };

            
            a.Push(null);
            a.Push(createComplexCtorC(len));
          

            await Test(a, (b)=>
            {
                Assert.Equal(a.A, b.A);
                CheckIEnumerableOfIEnumerable(a, (a, b) => checkCtorCProc(a)(b));
            }, new BinarySerializerOptions() { DefaultBufferSize = 1 });

        }


    }
}
