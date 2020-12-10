using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class UriTests
    {
        [InlineData("https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1&q2=v2#FragmentName")]
        [InlineData("ftp://user:password@www.contoso.com")]
        [Theory(DisplayName = "Uri")]
        public async Task Test1(string url)
        {
            Uri input = new Uri(url);
            MemoryStream ms = new MemoryStream();
            await BinarySerializer.SerializeAsync(ms, input);

            ms.Position = 0;

            Uri str = await BinarySerializer.DeserializeAsync<Uri>(ms);
            Assert.Equal(input, str);

            ms.Position = 0;
            str = (await BinarySerializer.DeserializeAsync(ms)) as Uri;
            Assert.Equal(input, str);

        }

    }
}
