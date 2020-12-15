using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    public partial class ObjectTests
    {

        class TrySkipA
        {
            public TrySkipA(TestCtorA testA)
            {
                TestA = testA;
            }

            public string A { get; internal set; }

            public uint? B { get; set; }

            public string C { get; set; }

            public TestCtorA TestA { get; internal set; }

            public TestCtorA TestB { get; internal set; }

            public TestCtorC TestC { get; set; }
        }
       
        [Fact(DisplayName = "Object_TrySkip")]
        public async Task Object_TrySkip_Test()
        {
            TrySkipA a = new TrySkipA(createComplexCtorC(1))
            {
                A = "A",
                B = 1,
                C = "C"
            };
            a.TestB = createComplexCtorC(1);
            a.TestC = createComplexCtorC(1);

            await Test(a, (b) =>
            {
                Assert.Equal(a.B, b.B);
                Assert.Null(b.A);
                Assert.Equal(a.C, b.C);
                if (b.TestA != null)
                {
                    checkCtorCProc(a.TestA)(b.TestA);
                }
                Assert.Null(b.TestB);

                if (a.TestC != null)
                {
                    checkCtorCProc(a.TestA)(b.TestA);
                }
            });

        }


        [InlineData(1024 * 10)]
        [InlineData(1024 * 512)]
        [InlineData(1024 * 1024)]
        [Theory(DisplayName = "Object_TrySkip_Buffer")]
        public async Task Object_TrySkip_Buffer_Test(int len)
        {
            BinarySerializerOptions options = new BinarySerializerOptions()
            {
                DefaultBufferSize = 1
            };
            TrySkipA a = new TrySkipA(createComplexCtorC(len))
            {
                A = new string('A', len),
                B = 1,
                C = new string('C', len)
            };
            a.TestB = createComplexCtorC(len);
            a.TestC = createComplexCtorC(len);

            await Test(a, (b) =>
            {
                Assert.Equal(a.B, b.B);
                Assert.Null(b.A);
                Assert.Equal(a.C, b.C);
                if (b.TestA != null)
                {
                    checkCtorCProc(a.TestA)(b.TestA);
                }
                Assert.Null(b.TestB);

                if (a.TestC != null)
                {
                    checkCtorCProc(a.TestA)(b.TestA);
                }
            }, options);

        }
    }
}
