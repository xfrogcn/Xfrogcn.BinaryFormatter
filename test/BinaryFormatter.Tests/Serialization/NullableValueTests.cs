using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class NullableValueTests
    {
        public async Task Test1<T>(T? input)
            where T : struct
        {
            MemoryStream ms = new MemoryStream();
            await BinarySerializer.SerializeAsync(ms, input);

            ms.Position = 0;

            T? b = await BinarySerializer.DeserializeAsync<T?>(ms);
            Assert.Equal(input, b);

            ms.Position = 0;
            object b1 = await BinarySerializer.DeserializeAsync(ms);
            Assert.Equal(input, (T?)b1);

        }

        [Fact(DisplayName = "Nullable.Value")]
        public async Task Test()
        {
            await Test1<int>(null);
            await Test1<int>(1);
            await Test1<int>(int.MaxValue);

            await Test1<BigInteger>(new BigInteger(new byte[] { 1,2,3,4,5}));
        }
    }
}
