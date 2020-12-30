using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class UInt64Tests : SerializerTestsBase
    {
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(0xffffffffffffffff)]
        [InlineData(0xf00)]
        [InlineData(0x0f)]
        [Theory(DisplayName = "UInt64")]
        public async Task Test1(ulong input)
        {
            await Test(input);

        }

    }
}
