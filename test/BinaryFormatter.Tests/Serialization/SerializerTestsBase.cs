using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "Object")]
    public class SerializerTestsBase
    {


        public async Task Test<T>(T input, Action<T> check, BinarySerializerOptions options = null)
        {
            MemoryStream ms = new MemoryStream();
            await BinarySerializer.SerializeAsync<T>(ms, input, options);

            ms.Position = 0;

            T b = await BinarySerializer.DeserializeAsync<T>(ms, options);
            check(b);

            ms.Position = 0;
            object b1 = await BinarySerializer.DeserializeAsync(ms, options);
            check((T)b1);

            byte[] bytes = BinarySerializer.SerializeToBytes<T>(input, options);

        }


        public async Task Test<T>(T input, BinarySerializerOptions options = null)
        {
            await Test<T>(input, b => Assert.Equal(input, b), options);
        }

        internal Action<TestCtorA> checkCtorCProc(TestCtorA a)
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
                    Assert.Equal(cb.EnumA, cb2.EnumA);
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


        internal Action<ObjTestA> checkTestAProc(ObjTestA a)
        {
            return (b) =>
            {
                Assert.Equal(a.GetType(), b.GetType());
                Assert.Equal(a.A, b.A);
                Assert.Equal(a.B, b.B);

                if( a is ObjTestB ab)
                {
                    ObjTestB bb = b as ObjTestB;
                    Assert.Equal(ab.E, bb.E);
                    if( ab.D == null)
                    {
                        Assert.Null(bb.D);
                    }
                    else
                    {
                        checkTestAProc(ab.D)(bb.D);
                    }
                    if(ab.C == null)
                    {
                        Assert.Null(bb.C);
                    }
                    else
                    {
                        checkTestAProc(ab.C)(bb.C);
                    }
                }
            };
        }


        internal TestCtorC createComplexCtorC(int len)
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
                    B = new string('B', len),
                    
                }
            },
            new TestCtorB(new string('A', len), 1)
            {
                TestA = new ObjTestA()
                {
                    B = new string('B', len),
                    A = 1
                },
                C = new string('C', len),
                EnumA = TestEnumA.A
            });


            TestCtorC c = new TestCtorC(b, a);

            return c;
        }
    }
}