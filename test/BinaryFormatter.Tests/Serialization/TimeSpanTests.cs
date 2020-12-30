using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class TimeSpanTests : SerializerTestsBase
    {
        
        [Fact(DisplayName = "TimeSpan")]
        public  async Task Test2()
        {
            await Test(TimeSpan.MinValue);
            await Test(TimeSpan.MaxValue);
            await Test(TimeSpan.FromTicks(0));
        }
        
    }
}
