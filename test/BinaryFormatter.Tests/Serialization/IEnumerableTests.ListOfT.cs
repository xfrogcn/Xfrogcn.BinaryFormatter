using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
   
    public partial class IEnumerableTests 
    {
        [InlineData(0)]
        [InlineData(127)]
        [InlineData(512)]
        [Theory(DisplayName = "Test_List_Simple_Number")]
        public async Task Test_List_Simple_Number(int len)
        {
            List<long> a1 = new List<long>();
            
            for(long i = 0; i < len; i++)
            {
                a1.Add(i);
            }
            
            await Test(a1, CheckIEnumerableOfList(a1, (a, b) => Assert.Equal(a, b)));

        }

        [InlineData(0)]
        [InlineData(127)]
        [InlineData(512)]
        [Theory(DisplayName = "Test_List_Simple_String")]
        public async Task Test_List_Simple_String(int len)
        {
            List<string> a1 = new List<string>();

            for (long i = 0; i < len; i++)
            {
                a1.Add(i.ToString());
            }

            await Test(a1, CheckIEnumerableOfList(a1, (a, b) => Assert.Equal(a, b)));

        }

        [InlineData(0, 0)]
        [InlineData(127, 1024 * 10)]
        [InlineData(512, 1024 * 1024)]
        [Theory(DisplayName = "Test_List_Simple_String_Buffer")]
        public async Task Test_List_Simple_String_Buffer(int arrayLen, int strLen)
        {
            BinarySerializerOptions options = new BinarySerializerOptions()
            {
                DefaultBufferSize = 1
            };
            List<string> a1 = new List<string>();

            for (long i = 0; i < arrayLen; i++)
            {
                a1.Add(new string('A', strLen));
            }

            await Test(a1, CheckIEnumerableOfList(a1, (a, b) => Assert.Equal(a, b)), options);

        }


        [Fact(DisplayName = "Test_List_Simple_Object_Value")]
        public async Task Test_List_Simple_Object_Value()
        {
            List<object> a1 = new List<object>
            {
                "A",
                0,
                (Nullable<int>)null,
                new Vector2(1,2),
                new object()
            };


            await Test(a1, CheckIEnumerableOfList(a1, (a, b) =>
            {
                if (a != null && a.GetType() == typeof(object))
                {
                    Assert.IsType<object>(b);
                }
                else
                {
                    Assert.Equal(a, b);
                }
            }));

        }

        [InlineData(500)]
        [InlineData(1024 * 10)]
        [InlineData(1024 * 512)]
        [InlineData(1024 * 1024)]
        [Theory(DisplayName = "Test_List_Complex_Object_Buffer")]
        public async Task Test_List_Complex_Object_Buffer(int len)
        {
            List<TestCtorA> a1 = new List<TestCtorA>
            {
                new TestCtorB(new string('A',len),1),
                createComplexCtorC(len),
                createComplexCtorC(len),
                new TestCtorB(new string('A',len),1),
                null,
                createComplexCtorC(len),
                null
            };


            await Test(a1, CheckIEnumerableOfList(a1, (a, b) => checkCtorCProc(a)(b)), new BinarySerializerOptions() { DefaultBufferSize = 1 });

        }

        class TestListA : List<TestCtorA>
        {
            public string A { get; set; }
        }

        [Fact(DisplayName = "Test_List_Custom_Type")]
        public async Task Test_List_Custom_Type()
        {
            TestListA a1 = new TestListA()
            {
                new TestCtorA("A",1),
            };
            a1.A = "A";

            await Test(a1,(b1)=>
            {
                CheckIEnumerableOfList(a1, (a, b) => checkCtorCProc(a)(b));
                Assert.Equal(a1.A, b1.A);
            });
        }

        [InlineData(500)]
        [InlineData(1024 * 10)]
        [InlineData(1024 * 512)]
        [InlineData(1024 * 1024)]
        [Theory(DisplayName = "Test_List_Custom_Type_Buffer")]
        public async Task Test_List_Custom_Type_Buffer(int len)
        {
            TestListA a1 = new TestListA()
            {
                createComplexCtorC(len),
                new TestCtorA("A",1),
                new TestCtorB("B",1),
            };
            a1.A = new string('A',len) ;

            await Test(a1, (b1) =>
            {
                CheckIEnumerableOfList(a1, (a, b) => checkCtorCProc(a)(b));
                Assert.Equal(a1.A, b1.A);
            }, new BinarySerializerOptions() { DefaultBufferSize = 1 });
        }

        class TestListCtorObj
        {
            [BinaryFormatter.Serialization.BinaryConstructor]
            public TestListCtorObj(List<TestCtorA> a)
            {
                A = a;
            }

            public List<TestCtorA> A { get; set; }
        }

        [Fact(DisplayName = "Test_List_Ctor_Object")]
        public async Task Test_List_Ctor_Object()
        {
            List<TestCtorA> a1 = new List<TestCtorA>()
            {
                createComplexCtorC(1),
                createComplexCtorC(2),
                null,
                createComplexCtorC(3),
                null
            };

            TestListCtorObj obj = new TestListCtorObj(a1);


            await Test(obj, b =>
            {
                CheckIEnumerableOfList(obj.A, (a, b) => checkCtorCProc(a)(b));
            });

        }

        [InlineData(500)]
        [InlineData(1024 * 10)]
        [InlineData(1024 * 512)]
        [InlineData(1024 * 1024)]
        [Theory(DisplayName = "Test_List_Ctor_Object_Buffer")]
        public async Task Test_List_Ctor_Object_Buffer(int len)
        {
            BinarySerializerOptions options = new BinarySerializerOptions()
            {
                DefaultBufferSize = 1
            };
            TestListA a1 = new TestListA
            {
                new TestCtorB(new string('A',len),1),
                createComplexCtorC(len),
                createComplexCtorC(len),
                null,
                createComplexCtorC(len),
                null
            };
            a1.A = new string('A', len);
            TestListCtorObj obj = new TestListCtorObj(a1);


            await Test(obj, b =>
            {
                CheckIEnumerableOfList(obj.A, (a, b) => checkCtorCProc(a)(b));
                Assert.Equal(obj.A.GetType(), b.A.GetType());
                if(obj.A is TestListA ta)
                {
                    Assert.Equal(ta.A, (b.A as TestListA).A);
                }
            }, options);

        }

        class TestListSkipObj
        {
            public List<TestCtorA> A { get; set; }

            public TestListA B { get; internal set; }

            public string C { get; set; }
        }

        [Fact(DisplayName = "Test_List_Skip")]
        public async Task Test_List_Skip()
        {
            TestListSkipObj a = new TestListSkipObj()
            {
                A = new List<TestCtorA> { createComplexCtorC(1) },
                B = new TestListA { createComplexCtorC(1) },
                C = "C"
            };
            a.B.A = "B";


            await Test(a, b =>
            {
                CheckIEnumerableOfList(a.A, (a1, b1) => checkCtorCProc(a1)(b1));
                Assert.Null(b.B);
                Assert.Equal(a.C, b.C);
            });

        }

        [InlineData(1024 * 10)]
        [InlineData(1024 * 512)]
        [InlineData(1024 * 1024)]
        [Theory(DisplayName = "Test_List_Skip_Buffer")]
        public async Task Test_List_Skip_Buffer(int len)
        {
            TestListSkipObj a = new TestListSkipObj()
            {
                A = new List<TestCtorA> { createComplexCtorC(len) },
                B = new TestListA { createComplexCtorC(len) },
                C = "C"
            };


            await Test(a, b =>
            {
                CheckIEnumerableOfList(a.A, (a1, b1) => checkCtorCProc(a1)(b1));
                Assert.Null(b.B);
                Assert.Equal(a.C, b.C);
            }, new BinarySerializerOptions() { DefaultBufferSize = 1 });

        }

        [InlineData(500)]
        [InlineData(1024 * 10)]
        [InlineData(1024 * 1024)]
        [Theory(DisplayName = "Test_List_With_List_Buffer")]
        public async Task Test_List_With_List_Buffer(int len)
        {
            //IList<List<IList<TestCtorA>>> a = new List<List<IList<TestCtorA>>>()
            //{
            //    new List<IList<TestCtorA>>()
            //    {
            //        new TestListA()
            //        {
            //            createComplexCtorC(len),
            //            createComplexCtorC(len)
            //        }
            //    }
            //};
            IList<TestListA> a = new List<TestListA>()
            {

                    new TestListA()
                    {
                        new TestCtorA("A",1)
                    }

            };

            await Test(a, (b) =>
            {

            });
            
        }

        protected virtual Action<List<TElement>> CheckIEnumerableOfList<TElement>(List<TElement> a,  Action<TElement,TElement> checker )
        {
            return (b) =>
            {
                Assert.Equal(a.Count, b.Count);
                for(int i = 0; i < a.Count; i++)
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
