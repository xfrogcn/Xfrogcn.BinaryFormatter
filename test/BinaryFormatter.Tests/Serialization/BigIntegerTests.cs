using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class BigIntegerTests
    {
        [Fact(DisplayName = "BigInteger")]
        public async Task Test1()
        {
            BigInteger input = new BigInteger(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });
            MemoryStream ms = new MemoryStream();
            await BinarySerializer.SerializeAsync(ms, input);

            ms.Position = 0;

            BigInteger b =  await BinarySerializer.DeserializeAsync<BigInteger>(ms);
            Assert.Equal(input, b);

            ms.Position = 0;
            object b1 = await BinarySerializer.DeserializeAsync(ms);
            Assert.Equal(input, (BigInteger)b1);

        }

        
    }
}
