using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class EnumTests
    {
        public enum TestByteEnum : byte
        {
            None=0,
            One = 1,
            Two =2,
            Max = byte.MaxValue
        }

        public enum TestUInt64Enum : ulong
        {
            None = 0,
            One = 1,
            Two = 2,
            Max = ulong.MaxValue
        }

        public enum TestInt64Enum : long
        {
            Min = long.MinValue,
            None = 0,
            One = 1,
            Two = 2,
            Max = long.MaxValue
        }

        private async Task Test<T>(T input)
        {
            MemoryStream ms = new MemoryStream();
            await BinarySerializer.SerializeAsync(ms, input);

            ms.Position = 0;

            T b =  await BinarySerializer.DeserializeAsync<T>(ms);
            Assert.Equal(input, b);

            ms.Position = 0;
            object b1 = await BinarySerializer.DeserializeAsync(ms);
            Assert.Equal(input, (T)b1);
        }

        [InlineData(TestByteEnum.None)]
        [InlineData(TestByteEnum.One)]
        [InlineData(TestByteEnum.Two)]
        [InlineData(TestByteEnum.Max)]
        [Theory(DisplayName = "Enum_Byte")]
        public  async Task ByteEnumTest(TestByteEnum input)
        {
            await Test(input);
        }

        [InlineData(TestUInt64Enum.None)]
        [InlineData(TestUInt64Enum.One)]
        [InlineData(TestUInt64Enum.Two)]
        [InlineData(TestUInt64Enum.Max)]
        [Theory(DisplayName = "Enum_UInt64")]
        public async Task UInt64EnumTest(TestUInt64Enum input)
        {
            await Test(input);
        }

        [InlineData(TestInt64Enum.Min)]
        [InlineData(TestInt64Enum.None)]
        [InlineData(TestInt64Enum.One)]
        [InlineData(TestInt64Enum.Two)]
        [InlineData(TestInt64Enum.Max)]
        [Theory(DisplayName = "Enum_Int64")]
        public async Task Int64EnumTest(TestInt64Enum input)
        {
            await Test(input);
        }
    }
}
