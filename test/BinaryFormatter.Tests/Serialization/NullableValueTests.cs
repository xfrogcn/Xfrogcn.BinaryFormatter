using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class NullableValueTests : SerializerTestsBase
    {

        [Fact(DisplayName = "Nullable_Value")]
        public async Task Test_Nullable_Value()
        {
            await Test<int?>(null, (b)=>Assert.Null(b));
            await Test<int?>(1, b=>Assert.Equal(1,b));
            await Test<int?>(int.MaxValue, b => Assert.Equal(int.MaxValue, b));

            var bi = new BigInteger(new byte[] { 1, 2, 3, 4, 5 });
            await Test<BigInteger?>(bi, b => Assert.Equal(bi, b));
        }


        [Fact(DisplayName = "Nullable_Struct")]
        public async Task Test_Nullable_Struct()
        {
            StructA? a = new StructA()
            {
                A = 1,
                B =2,
                C = "A",
                EA= TestEnumA.B,
                EB = null
            };

            await Test(a, b =>
            {
                Assert.Equal(a.Value.A, b.Value.A);
                Assert.Equal(a.Value.C, b.Value.C);
                Assert.Equal(a.Value.EA, b.Value.EA);
                Assert.Equal(a.Value.EB, b.Value.EB);
                Assert.Equal(0, b.Value.B);
            });
        }

        [Fact(DisplayName = "Nullable_Struct_IncludeFileds")]
        public async Task Test_Nullable_Struct_IncludeFileds()
        {
            StructA? a = new StructA()
            {
                A = 1,
                B = 2,
                C = "A",
                EA = TestEnumA.B,
                EB = null
            };

            await Test(a, b =>
            {
                Assert.Equal(a.Value.A, b.Value.A);
                Assert.Equal(a.Value.C, b.Value.C);
                Assert.Equal(a.Value.EA, b.Value.EA);
                Assert.Equal(a.Value.EB, b.Value.EB);
                Assert.Equal(a.Value.B, b.Value.B);
            }, new BinarySerializerOptions() { IncludeFields = true });
        }

        [InlineData(1024 * 10)]
        [InlineData(1024 * 512)]
        [InlineData(1024 * 1024)]
        [Theory(DisplayName = "Nullable_Struct_IncludeFileds_Buffer")]
        public async Task Test_Nullable_Struct_IncludeFileds_Buffer(int len)
        {
            StructA? a = new StructA()
            {
                A = 1,
                B = 2,
                C = new string('A',len),
                EA = TestEnumA.B,
                EB = null
            };

            await Test(a, b =>
            {
                Assert.Equal(a.Value.A, b.Value.A);
                Assert.Equal(a.Value.C, b.Value.C);
                Assert.Equal(a.Value.EA, b.Value.EA);
                Assert.Equal(a.Value.EB, b.Value.EB);
                Assert.Equal(a.Value.B, b.Value.B);
            }, new BinarySerializerOptions() { IncludeFields = true });
        }
    }
}
