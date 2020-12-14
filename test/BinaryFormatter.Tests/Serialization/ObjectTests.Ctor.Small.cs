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

        [Fact(DisplayName = "Test_Simple_Ctor")]
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
        [Theory(DisplayName = "Test_Simple_Ctor_Buffer")]
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
        }

        [Fact(DisplayName = "Test_Simple_Inherit_Ctor")]
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
            public TestCtorC(TestCtorB parent, TestCtorA tmp) : base(parent.A,parent.B)
            {
                Temp = tmp;
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
                    Assert.Equal(cb.C, (b as TestCtorB).C);
                }
                else if (a is TestCtorC cc)
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


        [Fact(DisplayName = "Test_Simple_Object_Ctor")]
        public async Task Test_Simple_Object_Ctor()
        {
            TestCtorB b = new TestCtorB("B", 1);
            b.C = "C";
            TestCtorA a = new TestCtorA("A", 1);

            TestCtorC c = new TestCtorC(b, a);

            await Test(c, checkCtorCProc(c));
            
        }

    }
}
