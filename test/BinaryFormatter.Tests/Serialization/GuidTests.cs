using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class GuidTests : SerializerTestsBase
    {
        [Fact(DisplayName = "Guid")]
        public async Task Test1()
        {
            Guid input = Guid.NewGuid();
            await Test(input);
        }

    }
}
