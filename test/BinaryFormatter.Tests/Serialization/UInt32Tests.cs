using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class UInt32Tests : SerializerTestsBase
    {
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(0xffffffff)]
        [InlineData(0xf00)]
        [InlineData(0x0f)]
        [Theory(DisplayName = "UInt32")]
        public async Task Test1(uint input)
        {
            await Test(input);

        }

    }
}
