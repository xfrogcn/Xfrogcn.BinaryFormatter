using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class UInt16Tests : SerializerTestsBase
    {
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(0xffff)]
        [InlineData(0xf00)]
        [InlineData(0x0f)]
        [Theory(DisplayName = "UInt16")]
        public async Task Test1(ushort input)
        {
            await Test(input);

        }

    }
}
