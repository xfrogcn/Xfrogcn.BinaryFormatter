using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class DecimalTests : SerializerTestsBase
    {
        [InlineData(0)]
        [InlineData(1.231)]
        [InlineData(0xf00)]
        [InlineData(0x0f)]
        [Theory(DisplayName = "Decimal")]
        public async Task Test1(decimal input)
        {
            await Test(input);
        }

        [Fact(DisplayName = "Decimal-Max")]
        public async Task TestMax()
        {
            await Test(decimal.MaxValue);

        }

        [Fact(DisplayName = "Decimal-Min")]
        public async Task TestMin()
        {
            await Test(decimal.MinValue);

        }
    }
}
