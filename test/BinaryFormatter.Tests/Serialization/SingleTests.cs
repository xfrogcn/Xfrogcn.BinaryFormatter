using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class SingleTests : SerializerTestsBase
    {
        [InlineData(0)]
        [InlineData(1.231)]
        [InlineData(float.MinValue)]
        [InlineData(float.MaxValue)]
        [InlineData(0xf00)]
        [InlineData(0x0f)]
        [Theory(DisplayName = "Single")]
        public async Task Test1(float input)
        {
            await Test(input);

        }

    }
}
