using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "IEnumerable")]
    public class IEnumerableTests : SerializerTestsBase
    {
        [InlineData(0)]
        [InlineData(127)]
        [InlineData(512)]
        [Theory(DisplayName = "Test_Array_Simple_Number")]
        public async Task Test_Array_Simple_Number(int len)
        {
            long[] a1 = new long[len];
            
            for(long i = 0; i < len; i++)
            {
                a1[i] = i;
            }
            
            await Test(a1, CheckIEnumerable(a1, (a, b) => Assert.Equal(a, b)));

        }

        [InlineData(0)]
        [InlineData(127)]
        [InlineData(512)]
        [Theory(DisplayName = "Test_Array_Simple_String")]
        public async Task Test_Array_Simple_String(int len)
        {
            string[] a1 = new string[len];

            for (long i = 0; i < len; i++)
            {
                a1[i] = i.ToString();
            }

            await Test(a1, CheckIEnumerable(a1, (a, b) => Assert.Equal(a, b)));

        }

        [InlineData(0,0)]
        [InlineData(127, 1024*10)]
        [InlineData(512, 1024 * 1024)]
        [Theory(DisplayName = "Test_Array_Simple_String_Buffer")]
        public async Task Test_Array_Simple_String_Buffer(int arrayLen, int strLen)
        {
            BinarySerializerOptions options = new BinarySerializerOptions()
            {
                DefaultBufferSize = 1
            };
            string[] a1 = new string[arrayLen];

            for (long i = 0; i < arrayLen; i++)
            {
                a1[i] = new string('A', strLen);
            }

            await Test(a1, CheckIEnumerable(a1, (a, b) => Assert.Equal(a, b)), options);

        }

       
        [Fact(DisplayName = "Test_Array_Simple_Object_Value")]
        public async Task Test_Array_Simple_Object_Value()
        {
            object[] a1 = new object[]
            {
                "A",
                0,
                (Nullable<int>)null,
                new Vector2(1,2)
            };


            await Test(a1, CheckIEnumerable(a1, (a, b) => Assert.Equal(a, b)));

        }

        [Fact(DisplayName = "Test_Array_Complex_Object")]
        public async Task Test_Array_Complex_Object()
        {
            TestCtorA[] a1 = new TestCtorA[]
            {
                createComplexCtorC(1),
                createComplexCtorC(2),
                null,
                createComplexCtorC(3),
                null
            };


            await Test(a1, CheckIEnumerable(a1, (a, b) => checkCtorCProc(a)(b)));

        }

        protected virtual Action<TElement[]> CheckIEnumerable<TElement>(TElement[] a,  Action<TElement,TElement> checker )
        {
            return (b) =>
            {
                Assert.Equal(a.Length, b.Length);
                for(int i = 0; i < a.Length; i++)
                {
                    TElement a1 = a[i];
                    TElement b1 = b[i];

                    if (a1 != null)
                    {
                        Assert.Equal(a1.GetType(), b1.GetType());
                        checker(a1, b1);
                    }
                    else
                    {
                        Assert.Null(b1);
                    }
                }
            };
        }
    }
}
