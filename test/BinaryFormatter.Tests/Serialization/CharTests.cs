using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class CharTests : SerializerTestsBase
    {
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(char.MaxValue)]
        [InlineData(char.MinValue)]
        [Theory(DisplayName = "Char")]
        public async Task Test1(char input)
        {
            await Test(input);
        }

    }
}
