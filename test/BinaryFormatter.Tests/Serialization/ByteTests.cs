using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class ByteTests
    {
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(0xff)]
        [InlineData(0xf0)]
        [InlineData(0x0f)]
        [Theory(DisplayName = "Byte")]
        public async Task Test1(byte input)
        {
            MemoryStream ms = new MemoryStream();
            await BinarySerializer.SerializeAsync(ms, input);

            ms.Position = 0;

            byte b =  await BinarySerializer.DeserializeAsync<byte>(ms);
            Assert.Equal(input, b);

            ms.Position = 0;
            object b1 = await BinarySerializer.DeserializeAsync(ms);
            Assert.Equal(input, (byte)b1);

        }

    }
}
