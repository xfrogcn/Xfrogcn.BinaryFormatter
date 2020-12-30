using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class UriTests : SerializerTestsBase
    {
        [InlineData("https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1&q2=v2#FragmentName")]
        [InlineData("ftp://user:password@www.contoso.com")]
        [Theory(DisplayName = "Uri")]
        public async Task Test1(string url)
        {
            Uri input = new Uri(url);
            await Test(input);

        }

    }
}
