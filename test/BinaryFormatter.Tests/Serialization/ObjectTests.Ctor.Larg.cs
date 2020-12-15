using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    public partial class ObjectTests
    {
        BinarySerializerOptions largCtorOptions = new BinarySerializerOptions()
        {
            IgnoreCtorParameterCountThreshold =true
        };

        [Fact(DisplayName = "Object_Larg_Ctor")]
        public async Task Test_Larg_Ctor()
        {
            TestCtorA a = new TestCtorA("A", 0);
            await Test(a, checkTestCtorA(a), largCtorOptions);

            a.C = "C";
            await Test(a, checkTestCtorA(a), largCtorOptions);
        }

        [InlineData(1024 * 10)]
        [InlineData(1024 * 512)]
        [InlineData(1024 * 1024)]
        [Theory(DisplayName = "Object_Larg_Ctor_Buffer")]
        public async Task Test_Larg_Ctor_Buffer(int len)
        {
            BinarySerializerOptions options = new BinarySerializerOptions()
            {
                DefaultBufferSize = 1,
                IgnoreCtorParameterCountThreshold = true
            };
            TestCtorA a = new TestCtorA(new string('A', len), 0);
            await Test(a, checkTestCtorA(a), options);

            a.C = new string('C', len);
            await Test(a, checkTestCtorA(a), options);
        }


        [Fact(DisplayName = "Object_Larg_Inherit_Ctor")]
        public async Task Test_Larg_Inherit_Ctor()
        {
            TestCtorB a = new TestCtorB("A", 0);
            await Test(a, checkTestCtorA(a), largCtorOptions);

            a.C = "C";
            await Test(a, checkTestCtorA(a), largCtorOptions);
        }




        [Fact(DisplayName = "Object_Larg_Object_Ctor")]
        public async Task Test_Larg_Object_Ctor()
        {
            TestCtorB b = new TestCtorB("B", 1);
            b.C = "C";
            TestCtorA a = new TestCtorA("A", 1);

            TestCtorC c = new TestCtorC(b, a);

            await Test<TestCtorC>(c, checkCtorCProc(c), largCtorOptions);

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

            await Test<TestCtorC>(c, checkCtorCProc(c), largCtorOptions);

        }

        [InlineData(1024 * 10)]
        [InlineData(1024 * 512)]
        [InlineData(1024 * 1024)]
        [Theory(DisplayName = "Object_Larg_Object_Ctor_Buffer")]
        public async Task Test_Larg_Object_Ctor_Buffer(int len)
        {
            BinarySerializerOptions options = new BinarySerializerOptions() 
            { 
                DefaultBufferSize = 1,
                IgnoreCtorParameterCountThreshold = true
            };

            TestCtorB b = new TestCtorB(new string('B', len), 1);
            b.C = new string('C', len);
            TestCtorA a = new TestCtorA(new string('A', len), 1);

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
        [Theory(DisplayName = "Test_Larg_Object_Ctor_With_Polymorphic_Buffer")]
        public async Task Test_Larg_Object_Ctor_With_Polymorphic_Buffer(int len)
        {
            BinarySerializerOptions options = new BinarySerializerOptions()
            { 
                DefaultBufferSize = 1,
                IgnoreCtorParameterCountThreshold = true
            };

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

            await Test<TestCtorA>(a, checkCtorCProc(a), options);

            await Test<TestCtorA>(c, checkCtorCProc(c), options);


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
            await Test<TestCtorA>(c, checkCtorCProc(c), options);

        }
    }
}
