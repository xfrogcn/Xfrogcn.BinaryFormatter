using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class Int32Tests
    {
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        [InlineData(0xf00)]
        [InlineData(0x0f)]
        [Theory(DisplayName = "Int32")]
        public async Task Test1(int input)
        {
            MemoryStream ms = new MemoryStream();
            await BinarySerializer.SerializeAsync(ms, input);

            ms.Position = 0;

            int b =  await BinarySerializer.DeserializeAsync<int>(ms);
            Assert.Equal(input, b);

            ms.Position = 0;
            object b1 = await BinarySerializer.DeserializeAsync(ms);
            Assert.Equal(input, (int)b1);

        }

    }
}
