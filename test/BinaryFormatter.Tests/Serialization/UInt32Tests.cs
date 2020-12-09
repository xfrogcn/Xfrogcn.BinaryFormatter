using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class UInt32Tests
    {
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(0xffffffff)]
        [InlineData(0xf00)]
        [InlineData(0x0f)]
        [Theory(DisplayName = "UInt32")]
        public async Task Test1(uint input)
        {
            MemoryStream ms = new MemoryStream();
            await BinarySerializer.SerializeAsync(ms, input);

            ms.Position = 0;

            uint b =  await BinarySerializer.DeserializeAsync<uint>(ms);
            Assert.Equal(input, b);

            ms.Position = 0;
            object b1 = await BinarySerializer.DeserializeAsync(ms);
            Assert.Equal(input, (uint)b1);

        }

    }
}
