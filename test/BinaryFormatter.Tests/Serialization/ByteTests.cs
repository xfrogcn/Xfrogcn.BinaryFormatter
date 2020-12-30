using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class ByteTests : SerializerTestsBase
    {
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(0xff)]
        [InlineData(0xf0)]
        [InlineData(0x0f)]
        [Theory(DisplayName = "Byte")]
        public async Task Test1(byte input)
        {
            await Test(input);

        }

    }
}
