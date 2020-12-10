using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class TimeSpanTests
    {
        private async Task Test(TimeSpan input)
        {
            MemoryStream ms = new MemoryStream();
            await BinarySerializer.SerializeAsync(ms, input);

            ms.Position = 0;

            TimeSpan b =  await BinarySerializer.DeserializeAsync<TimeSpan>(ms);
            Assert.Equal(input, b);

            ms.Position = 0;
            object b1 = await BinarySerializer.DeserializeAsync(ms);
            Assert.Equal(input, (TimeSpan)b1);

        }

        [Fact(DisplayName = "TimeSpan")]
        public  async Task Test2()
        {
            await Test(TimeSpan.MinValue);
            await Test(TimeSpan.MaxValue);
            await Test(TimeSpan.FromTicks(0));
        }
        
    }
}
