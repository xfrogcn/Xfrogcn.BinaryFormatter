using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class StringTests
    {
        [InlineData("")]
        [InlineData(null)]
        [InlineData("HELLO 国")]
        [InlineData("😽 😿 😿")]
        [Theory(DisplayName = "String")]
        public async Task Test1(string input)
        {
            MemoryStream ms = new MemoryStream();
            await BinarySerializer.SerializeAsync(ms, input);

            ms.Position = 0;

            string str = await BinarySerializer.DeserializeAsync<string>(ms);
            Assert.Equal(input, str);

            ms.Position = 0;
            str = (await BinarySerializer.DeserializeAsync(ms)) as string;
            Assert.Equal(input, str);

        }


        [Fact(DisplayName = "String_Buffer1")]
        public async Task Test2()
        {
            BinarySerializerOptions options = new BinarySerializerOptions()
            {
                DefaultBufferSize = 1
            };
            string input = new string('A', 1024 * 1024);
            MemoryStream ms = new MemoryStream();
            await BinarySerializer.SerializeAsync(ms, input);

            ms.Position = 0;

            string str = await BinarySerializer.DeserializeAsync<string>(ms, options);
            Assert.Equal(input, str);

            ms.Position = 0;
            str = (await BinarySerializer.DeserializeAsync(ms, options)) as string;
            Assert.Equal(input, str);
        }
    }
}
