using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class Int16Tests : SerializerTestsBase
    {
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-32768)]
        [InlineData(32767)]
        [InlineData(0xf00)]
        [InlineData(0x0f)]
        [Theory(DisplayName = "Int16")]
        public async Task Test1(short input)
        {
            await Test(input);

        }

    }
}
