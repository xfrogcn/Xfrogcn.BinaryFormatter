using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class DateTimeOffsetTests : SerializerTestsBase
    {
       

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
