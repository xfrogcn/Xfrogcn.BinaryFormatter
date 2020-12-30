using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class BoolTests : SerializerTestsBase
    {
        [InlineData(true)]
        [InlineData(false)]
        [Theory(DisplayName = "Boolean")]
        public async Task Test1(bool input)
        {
            await Test(input);
        }

    }
}
