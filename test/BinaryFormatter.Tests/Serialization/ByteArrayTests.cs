using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class ByteArrayTests : SerializerTestsBase
    {

        [Fact(DisplayName = "ByteArray")]
        public  async Task Test2()
        {
            await Test(new byte[0]);
            await Test(new byte[] { 1,2,3 });
          
        }
        
    }
}
