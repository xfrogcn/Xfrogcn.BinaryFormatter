using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class Int64Tests : SerializerTestsBase
    {
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(long.MinValue)]
        [InlineData(long.MaxValue)]
        [InlineData(0xf00)]
        [InlineData(0x0f)]
        [Theory(DisplayName = "Int64")]
        public async Task Test1(long input)
        {
            await Test(input);
        }

    }
}
