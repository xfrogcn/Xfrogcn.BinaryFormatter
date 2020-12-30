using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class DbNullTests : SerializerTestsBase
    {

        [Fact(DisplayName = "DBNull")]
        public async Task Test1()
        {
            DBNull input = DBNull.Value;
            await Test(input);

        }

    }
}
