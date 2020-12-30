using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class DateTimeTests : SerializerTestsBase
    {
       

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
