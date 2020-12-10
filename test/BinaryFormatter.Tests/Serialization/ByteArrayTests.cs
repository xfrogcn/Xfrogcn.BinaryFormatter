using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class ByteArrayTests
    {
        private async Task Test(byte[] input)
        {
            MemoryStream ms = new MemoryStream();
            await BinarySerializer.SerializeAsync(ms, input);

            ms.Position = 0;

            byte[] b =  await BinarySerializer.DeserializeAsync<byte[]>(ms);
            Assert.Equal(input, b);

            ms.Position = 0;
            object b1 = await BinarySerializer.DeserializeAsync(ms);
            Assert.Equal(input, (byte[])b1);
        }

        [Fact(DisplayName = "ByteArray")]
        public  async Task Test2()
        {
            await Test(new byte[0]);
            await Test(new byte[] { 1,2,3 });
          
        }
        
    }
}
