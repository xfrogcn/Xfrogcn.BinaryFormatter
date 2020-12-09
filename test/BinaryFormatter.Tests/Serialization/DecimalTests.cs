using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class DecimalTests
    {
        [InlineData(0)]
        [InlineData(1.231)]
        [InlineData(0xf00)]
        [InlineData(0x0f)]
        [Theory(DisplayName = "Decimal")]
        public async Task Test1(decimal input)
        {
            MemoryStream ms = new MemoryStream();
            await BinarySerializer.SerializeAsync(ms, input);

            ms.Position = 0;

            decimal b =  await BinarySerializer.DeserializeAsync<decimal>(ms);
            Assert.Equal(input, b);

            ms.Position = 0;
            object b1 = await BinarySerializer.DeserializeAsync(ms);
            Assert.Equal(input, (decimal)b1);

        }

        [Fact(DisplayName = "Decimal-Max")]
        public async Task TestMax()
        {
            await Test1(decimal.MaxValue);

        }

        [Fact(DisplayName = "Decimal-Min")]
        public async Task TestMin()
        {
            await Test1(decimal.MinValue);

        }
    }
}
