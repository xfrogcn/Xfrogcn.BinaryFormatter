using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    public partial class ObjectTests
    {
       
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

       
        [Fact(DisplayName = "Object_Simple_Inherit_Ctor")]
        public async Task Test_Simple_Inherit_Ctor()
        {
            TestCtorB a = new TestCtorB("A", 0);
            await Test(a, checkTestCtorA(a));

            a.C = "C";
            await Test(a, checkTestCtorA(a));
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
