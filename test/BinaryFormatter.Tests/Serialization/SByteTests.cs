using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class SByteTests : SerializerTestsBase
    {
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-128)]
        [InlineData(127)]
        [InlineData(0x0f)]
        [Theory(DisplayName = "SByte")]
        public async Task Test1(sbyte input)
        {
            await Test(input);

        }

    }
}
