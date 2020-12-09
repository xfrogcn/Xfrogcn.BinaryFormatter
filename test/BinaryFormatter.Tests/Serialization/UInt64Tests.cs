using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class UInt64Tests
    {
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(0xffffffffffffffff)]
        [InlineData(0xf00)]
        [InlineData(0x0f)]
        [Theory(DisplayName = "UInt64")]
        public async Task Test1(ulong input)
        {
            MemoryStream ms = new MemoryStream();
            await BinarySerializer.SerializeAsync(ms, input);

            ms.Position = 0;

            ulong b =  await BinarySerializer.DeserializeAsync<ulong>(ms);
            Assert.Equal(input, b);

            ms.Position = 0;
            object b1 = await BinarySerializer.DeserializeAsync(ms);
            Assert.Equal(input, (ulong)b1);

        }

    }
}
