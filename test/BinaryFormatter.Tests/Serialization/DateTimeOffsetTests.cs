using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class DateTimeOffsetTests
    {
        private async Task Test(DateTimeOffset input)
        {
            MemoryStream ms = new MemoryStream();
            await BinarySerializer.SerializeAsync(ms, input);

            ms.Position = 0;

            DateTimeOffset b =  await BinarySerializer.DeserializeAsync<DateTimeOffset>(ms);
            Assert.Equal(input, b);

            ms.Position = 0;
            object b1 = await BinarySerializer.DeserializeAsync(ms);
            Assert.Equal(input, (DateTimeOffset)b1);

        }

        [Fact(DisplayName = "DateTimeOffset")]
        public  async Task Test2()
        {
            await Test(DateTimeOffset.MinValue);
            await Test(DateTimeOffset.MaxValue);
            await Test(DateTimeOffset.Now);
            await Test(DateTimeOffset.UtcNow);
        }
        
    }
}
