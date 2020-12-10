using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class GuidTests
    {
        [Fact(DisplayName = "Guid")]
        public async Task Test1()
        {
            Guid input = Guid.NewGuid();
            MemoryStream ms = new MemoryStream();
            await BinarySerializer.SerializeAsync(ms, input);

            ms.Position = 0;

            Guid b = await BinarySerializer.DeserializeAsync<Guid>(ms);
            Assert.Equal(input, b);

            ms.Position = 0;
            object b1 = await BinarySerializer.DeserializeAsync(ms);
            Assert.Equal(input, (Guid)b1);

        }

    }
}
