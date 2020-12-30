using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class StringTests : SerializerTestsBase
    {
        [InlineData("")]
        [InlineData(null)]
        [InlineData("HELLO 国")]
        [InlineData("😽 😿 😿")]
        [Theory(DisplayName = "String")]
        public async Task Test1(string input)
        {
            await Test(input);

        }

        [InlineData(1)]
        [InlineData(10)]
        [InlineData(512)]
        [InlineData(1024)]
        [InlineData(1024*10)]
        [Theory(DisplayName = "String_Buffer1")]
        public async Task Test2(int len)
        {
            BinarySerializerOptions options = new BinarySerializerOptions()
            {
                DefaultBufferSize = 1
            };
            string input = new string('A', 1024 * len);
            await Test(input, options);
        }
    }
}
