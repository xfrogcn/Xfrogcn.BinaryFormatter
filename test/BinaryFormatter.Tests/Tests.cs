using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xfrogcn.BinaryFormatter.Serialization;
using System.IO;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "Test")]
    public class Tests
    {
        [Fact]
        public async Task Test1()
        {
            MemoryStream ms = new MemoryStream();
            await BinarySerializer.SerializeAsync(ms, true);

            ms.Position = 0;
            byte[] data = ms.GetBuffer();

            ms.Position = 0;

            await BinarySerializer.DeserializeAsync(ms);
        }
    }
}
