using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class CharTests
    {
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(char.MaxValue)]
        [InlineData(char.MinValue)]
        [Theory(DisplayName = "Char")]
        public async Task Test1(char input)
        {
            MemoryStream ms = new MemoryStream();
            await BinarySerializer.SerializeAsync(ms, input);

            ms.Position = 0;

            char b =  await BinarySerializer.DeserializeAsync<char>(ms);
            Assert.Equal(input, b);

            ms.Position = 0;
            object b1 = await BinarySerializer.DeserializeAsync(ms);
            Assert.Equal(input, (char)b1);

        }

    }
}
