﻿using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class BoolTests
    {
        [Fact(DisplayName = "Boolean")]
        public async Task Test1()
        {
            MemoryStream ms = new MemoryStream();
            await BinarySerializer.SerializeAsync(ms, true);

            ms.Position = 0;
            byte[] data = ms.GetBuffer();

            ms.Position = 0;

            bool b =  await BinarySerializer.DeserializeAsync<bool>(ms);
            Assert.True(b);

            ms.Position = 0;
            object b1 = await BinarySerializer.DeserializeAsync(ms);
            Assert.True((bool)b1);

            ms = new MemoryStream();
            await BinarySerializer.SerializeAsync(ms, false);

            ms.Position = 0;
            b = await BinarySerializer.DeserializeAsync<bool>(ms);
            Assert.False(b);

            ms.Position = 0;
            b1 = await BinarySerializer.DeserializeAsync(ms);
            Assert.False((bool)b1);

        }


        [Fact(DisplayName = "Boolean-Buffer1")]
        public async Task Test2()
        {
            var options = new BinarySerializerOptions() { DefaultBufferSize = 1 };
            MemoryStream ms = new MemoryStream();
            await BinarySerializer.SerializeAsync(ms, true, options);

            ms.Position = 0;
            byte[] data = ms.GetBuffer();

            ms.Position = 0;

            bool b = await BinarySerializer.DeserializeAsync<bool>(ms, options);
            Assert.True(b);

            ms.Position = 0;
            object b1 = await BinarySerializer.DeserializeAsync(ms, options);
            Assert.True((bool)b1);

            ms = new MemoryStream();
            await BinarySerializer.SerializeAsync(ms, false, options);

            ms.Position = 0;
            b = await BinarySerializer.DeserializeAsync<bool>(ms, options);
            Assert.False(b);

            ms.Position = 0;
            b1 = await BinarySerializer.DeserializeAsync(ms, options);
            Assert.False((bool)b1);

        }
    }
}