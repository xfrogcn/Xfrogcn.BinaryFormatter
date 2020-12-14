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
    }
}
