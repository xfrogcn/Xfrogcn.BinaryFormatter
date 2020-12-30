using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class DoubleTests : SerializerTestsBase
    {
        [InlineData(0)]
        [InlineData(1.231)]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(0xf00)]
        [InlineData(0x0f)]
        [Theory(DisplayName = "Double")]
        public async Task Test1(double input)
        {
            await Test(input);

        }

    }
}
