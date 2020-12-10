using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class DateTimeTests
    {
        private async Task Test(DateTime input)
        {
            MemoryStream ms = new MemoryStream();
            await BinarySerializer.SerializeAsync(ms, input);

            ms.Position = 0;

            DateTime b =  await BinarySerializer.DeserializeAsync<DateTime>(ms);
            Assert.Equal(input, b);

            ms.Position = 0;
            object b1 = await BinarySerializer.DeserializeAsync(ms);
            Assert.Equal(input, (DateTime)b1);

        }

        [Fact(DisplayName = "DateTime")]
        public  async Task Test2()
        {
            await Test(DateTime.MinValue);
            await Test(DateTime.MaxValue);
            await Test(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            await Test(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Unspecified));
            await Test(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Local));
        }
        
    }
}
