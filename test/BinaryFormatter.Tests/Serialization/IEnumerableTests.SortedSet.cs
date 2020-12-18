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
        [Theory(DisplayName = "Test_SortedSet_Simple_Number")]
        public async Task Test_SortedSet_Simple_Number(int len)
        {
            SortedSet<long> a = new SortedSet<long>();
            for (long i = 0; i < len; i++)
            {
                a.Add(i);
            }

            

            await Test(a, CheckIEnumerableOfIEnumerable(a, (a, b) => Assert.Equal(a, b)));

        }


        [InlineData(500)]
        [InlineData(1024)]
        [InlineData(1024 * 1024)]
        [Theory(DisplayName = "Test_SortedSet_Complex_Object_Buffer")]
        public async Task Test_SortedSet_Complex_Object_Buffer(int len)
        {
            SortedSet<TestCtorA> a = new SortedSet<TestCtorA>();
            
            a.Add(createComplexCtorC(len));
            a.Add(null);
            a.Add(createComplexCtorC(len));
            a.Add(new TestCtorA(new string('A', len), 0));
            a.Add(createComplexCtorC(len));
           
            await Test(a, CheckIEnumerableOfIEnumerable(a, (a, b) => checkCtorCProc(a)(b)), new BinarySerializerOptions() { DefaultBufferSize = 1 });

        }

        class TestSortedSetA : SortedSet<TestCtorA>
        {
            public string A { get; set; }
        }

        [InlineData(500)]
        [InlineData(1024)]
        [InlineData(1024 * 1024)]
        [Theory(DisplayName = "Test_SortedSet_Custom_Complex_Object_Buffer")]
        public async Task Test_SortedSet_Custom_Complex_Object_Buffer(int len)
        {
            TestSortedSetA a = new TestSortedSetA()
            {
                A = new string('A', len)
            };

            
            a.Add(null);
          

            await Test(a, (b)=>
            {
                Assert.Equal(a.A, b.A);
                CheckIEnumerableOfIEnumerable(a, (a, b) => checkCtorCProc(a)(b));
            }, new BinarySerializerOptions() { DefaultBufferSize = 1 });

        }


    }
}
