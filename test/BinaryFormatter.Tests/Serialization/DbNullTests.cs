using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class DbNullTests
    {

        [Fact(DisplayName = "DBNull")]
        public async Task Test1()
        {
            DBNull input = DBNull.Value;
            MemoryStream ms = new MemoryStream();
            await BinarySerializer.SerializeAsync(ms, input);

            ms.Position = 0;

            DBNull b =  await BinarySerializer.DeserializeAsync<DBNull>(ms);
            Assert.Equal(input, b);

            ms.Position = 0;
            object b1 = await BinarySerializer.DeserializeAsync(ms);
            Assert.Equal(input, (DBNull)b1);

        }

    }
}
