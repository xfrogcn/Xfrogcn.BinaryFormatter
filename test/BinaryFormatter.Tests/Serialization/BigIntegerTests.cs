using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class BigIntegerTests : SerializerTestsBase
    {
        [Fact(DisplayName = "BigInteger")]
        public async Task Test1()
        {
            BigInteger input = new BigInteger(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });
            await Test(input);
        }

        
    }
}
