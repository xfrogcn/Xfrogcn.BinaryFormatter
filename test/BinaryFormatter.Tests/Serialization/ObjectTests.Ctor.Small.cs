using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    public partial class ObjectTests
    {
        class TestCtorA
        {
            [BinaryFormatter.Serialization.BinaryConstructor]
            public TestCtorA(string a, uint b)
            {
                A = a;
                B = b;
            }

            public string A { get; set; }

            public uint B { get; set; }

            public string C { get; set; }
        }

        private Action<TestCtorA> checkTestCtorA(TestCtorA a)
        {
            return (b) =>
            {
                Assert.Equal(a.A, b.A);
                Assert.Equal(a.B, b.B);
                Assert.Equal(a.C, b.C);
            };
        }

        [Fact(DisplayName = "Object_Simple_Ctor")]
        public async Task Test_Simple_Ctor()
        {
            TestCtorA a = new TestCtorA("A", 0);
            await Test(a, checkTestCtorA(a));

            a.C = "C";
            await Test(a, checkTestCtorA(a));
        }

        [InlineData(1024 * 10)]
        [InlineData(1024 * 512)]
        [InlineData(1024 * 1024)]
        [Theory(DisplayName = "Object_Simple_Ctor_Buffer")]
        public async Task Test_Simple_Ctor_Buffer(int len)
        {
            BinarySerializerOptions options = new BinarySerializerOptions()
            {
                DefaultBufferSize = 1
            };
            TestCtorA a = new TestCtorA(new string('A', len), 0);
            await Test(a, checkTestCtorA(a), options);

            a.C = new string('C', len);
            await Test(a, checkTestCtorA(a), options);
        }

        class TestCtorB : TestCtorA
        {
            public TestCtorB(string a, uint b) : base(a,b)
            {

            }

            public ObjTestA TestA { get; set; }
        }

        [Fact(DisplayName = "Object_Simple_Inherit_Ctor")]
        public async Task Test_Simple_Inherit_Ctor()
        {
            TestCtorB a = new TestCtorB("A", 0);
            await Test(a, checkTestCtorA(a));

            a.C = "C";
            await Test(a, checkTestCtorA(a));
        }

        class TestCtorC : TestCtorB
        {
            [BinaryFormatter.Serialization.BinaryConstructor]
            public TestCtorC(TestCtorB parent, TestCtorA temp) : base(parent.A,parent.B)
            {
                Temp = temp;
                Parent = parent;
            }

            public TestCtorB Parent { get; set; }

            public TestCtorA Temp { get; }
        }

        private Action<TestCtorA> checkCtorCProc(TestCtorA a)
        {
            Action<TestCtorA> check = b =>
            {
                Assert.Equal(a.GetType(), b.GetType());
                Assert.Equal(a.A, b.A);
                Assert.Equal(a.B, b.B);

                if (a is TestCtorB cb)
                {
                    TestCtorB cb2 = b as TestCtorB;
                    Assert.Equal(cb.C, cb2.C);
                    if(cb.TestA == null)
                    {
                        Assert.Null(cb2.TestA);
                    }
                    else
                    {
                        checkTestAProc(cb.TestA)(cb2.TestA);
                    }
                }
                if (a is TestCtorC cc)
                {
                    TestCtorB p1 = cc.Parent;
                    TestCtorB p2 = (b as TestCtorC).Parent;
                    if (p1 == null)
                    {
                        Assert.Null(p2);
                    }
                    else
                    {
                        checkCtorCProc(p1).Invoke(p2);
                    }
                    

                    TestCtorA t1 = cc.Temp;
                    TestCtorA t2 = (b as TestCtorC).Temp;
                    if(t1 == null)
                    {
                        Assert.Null(t2);
                    }
                    else
                    {
                        checkCtorCProc(t1).Invoke(t2);
                    }
                    
                }


            };

            return check;
        }


        [Fact(DisplayName = "Object_Simple_Object_Ctor")]
        public async Task Test_Simple_Object_Ctor()
        {
            TestCtorB b = new TestCtorB("B", 1);
            b.C = "C";
            TestCtorA a = new TestCtorA("A", 1);

            TestCtorC c = new TestCtorC(b, a);

            await Test<TestCtorC>(c, checkCtorCProc(c));

            b.TestA = new ObjTestB()
            {
                A = 1,
                D = new ObjTestB()
                {
                   A = 1,
                   B = "B",
                   D = new ObjTestA()
                   {
                       A = 2
                   }
                },
                E = 1,
                C = new ObjTestB()
                {
                    A = 1,
                    B = "B"
                }
            };

            await Test<TestCtorC>(c, checkCtorCProc(c));

        }

        [InlineData(1024 * 10)]
        [InlineData(1024 * 512)]
        [InlineData(1024 * 1024)]
        [Theory(DisplayName = "Object_Simple_Object_Ctor_Buffer")]
        public async Task Test_Simple_Object_Ctor_Buffer(int len)
        {
            BinarySerializerOptions options = new BinarySerializerOptions() { DefaultBufferSize = 1 };
            
            TestCtorB b = new TestCtorB(new string('B',len), 1);
            b.C = new string('C',len) ;
            TestCtorA a = new TestCtorA(new string('A',len), 1);

            TestCtorC c = new TestCtorC(b, a);

            await Test<TestCtorC>(c, checkCtorCProc(c), options);



            b.TestA = new ObjTestB()
            {
                A = 1,
                D = new ObjTestB()
                {
                    A = 1,
                    B = new string('B', len),
                    D = new ObjTestA()
                    {
                        A = 2
                    }
                },
                E = 1,
                C = new ObjTestB()
                {
                    A = 1,
                    B = new string('B', len)
                }
            };

            options = new BinarySerializerOptions() { DefaultBufferSize = 32 };
            await Test<TestCtorC>(c, checkCtorCProc(c), options);

        }


        private TestCtorC createComplexCtorC(int len)
        {
            TestCtorB b = new TestCtorB(new string('B', len), 1);
            b.C = new string('C', len);
            TestCtorA a = new TestCtorC(new TestCtorC(
                new TestCtorB(new string('B', len), 1),
                new TestCtorB(new string('B', len), 1))
            {
                TestA = new ObjTestB()
                {
                    E = 1,
                    B = new string('B', len)
                }
            },
            new TestCtorB(new string('A', len), 1)
            {
                TestA = new ObjTestA()
                {
                    B = new string('B', len),
                    A = 1
                },
                C = new string('C', len)
            });


            TestCtorC c = new TestCtorC(b, a);

            return c;
        }


        [InlineData(1024 * 10)]
        [InlineData(1024 * 512)]
        [InlineData(1024 * 1024)]
        [Theory(DisplayName = "Test_Simple_Object_Ctor_With_Polymorphic_Buffer")]
        public async Task Test_Simple_Object_Ctor_With_Polymorphic_Buffer(int len)
        {
            BinarySerializerOptions options = new BinarySerializerOptions() { DefaultBufferSize = 1 };

            TestCtorC c = createComplexCtorC(len);

            await Test<TestCtorA>(c, checkCtorCProc(c), options);


            c.Parent.TestA = new ObjTestB()
            {
                A = 1,
                D = new ObjTestB()
                {
                    A = 1,
                    B = new string('B', len),
                    D = new ObjTestA()
                    {
                        A = 2
                    }
                },
                E = 1,
                C = new ObjTestB()
                {
                    A = 1,
                    B = new string('B', len)
                }
            };

            options = new BinarySerializerOptions() { DefaultBufferSize = 32 };
            await Test<TestCtorA>(c, checkCtorCProc(c), options);

        }
    }
}
