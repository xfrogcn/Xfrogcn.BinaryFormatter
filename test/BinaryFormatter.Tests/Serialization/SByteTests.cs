using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class SByteTests
    {
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-128)]
        [InlineData(127)]
        [InlineData(0x0f)]
        [Theory(DisplayName = "SByte")]
        public async Task Test1(sbyte input)
        {
            MemoryStream ms = new MemoryStream();
            await BinarySerializer.SerializeAsync(ms, input);

            ms.Position = 0;

            sbyte b =  await BinarySerializer.DeserializeAsync<sbyte>(ms);
            Assert.Equal(input, b);

            ms.Position = 0;
            object b1 = await BinarySerializer.DeserializeAsync(ms);
            Assert.Equal(input, (sbyte)b1);

        }

    }
}
