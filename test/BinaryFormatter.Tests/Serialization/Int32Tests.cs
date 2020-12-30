using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class Int32Tests : SerializerTestsBase
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
            await Test(input);
        }

    }
}
