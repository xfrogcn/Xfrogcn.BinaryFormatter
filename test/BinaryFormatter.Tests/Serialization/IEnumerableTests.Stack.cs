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
        [Theory(DisplayName = "Test_Stack_Simple_Number")]
        public async Task Test_Stack_Simple_Number(int len)
        {
            Stack<long> a = new Stack<long>();
            for (long i = 0; i < len; i++)
            {
                a.Push(i);
            }

            

            await Test(a, CheckIEnumerableOfIEnumerable(a, (a, b) => Assert.Equal(a, b)));

        }


        [InlineData(500)]
        [InlineData(1024)]
        [InlineData(1024 * 1024)]
        [Theory(DisplayName = "Test_Stack_Complex_Object_Buffer")]
        public async Task Test_Stack_Complex_Object_Buffer(int len)
        {
            Stack<TestCtorA> a = new Stack<TestCtorA>();
            
            a.Push(createComplexCtorC(len));
            a.Push(null);
            a.Push(createComplexCtorC(len));
            a.Push(new TestCtorA(new string('A', len), 0));
            a.Push(createComplexCtorC(len));
           
            await Test(a, CheckIEnumerableOfIEnumerable(a, (a, b) => checkCtorCProc(a)(b)), new BinarySerializerOptions() { DefaultBufferSize = 1 });

        }

        class TestStackA : Stack<TestCtorA>
        {
            public string A { get; set; }
        }

        [InlineData(500)]
        [InlineData(1024)]
        [InlineData(1024 * 1024)]
        [Theory(DisplayName = "Test_Stack_Custom_Complex_Object_Buffer")]
        public async Task Test_Stack_Custom_Complex_Object_Buffer(int len)
        {
            TestStackA a = new TestStackA()
            {
                A = new string('A', len)
            };

            
            a.Push(null);
          

            await Test(a, (b)=>
            {
                Assert.Equal(a.A, b.A);
                CheckIEnumerableOfIEnumerable(a, (a, b) => checkCtorCProc(a)(b));
            }, new BinarySerializerOptions() { DefaultBufferSize = 1 });

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
