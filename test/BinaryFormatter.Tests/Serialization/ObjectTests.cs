﻿using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "Object")]
    public class ObjectTests
    {
        
       
        public async Task Test<T>(T input, Action<T> check, BinarySerializerOptions options = null)
        {
            MemoryStream ms = new MemoryStream();
            await BinarySerializer.SerializeAsync(ms, input);

            ms.Position = 0;

            T b =  await BinarySerializer.DeserializeAsync<T>(ms, options);
            check(b);

            ms.Position = 0;
            object b1 = await BinarySerializer.DeserializeAsync(ms, options);
            check((T)b1);

        }

        class ObjTestA
        {
            public uint A { get; set; }

            public string B { get; set; }
        }

        [Fact(DisplayName = "Object-Simple")]
        public async Task Test_SimpleObj()
        {
            Type type = typeof(ObjTestA);

            ObjTestA a = new ObjTestA()
            {
                A = 1,
                B = null
            };

            await Test<ObjTestA>(null, (obj) => Assert.Null(obj));

            Action<ObjTestA> check = b =>
            {
                Assert.Equal(a.A, b.A);
                Assert.Equal(a.B, b.B);
            };
            await Test(a, check);

            a.B = "AAAA";
            a.A = uint.MaxValue;
            await Test(a, check);


        }

        [InlineData(1024*10)]
        [InlineData(1024 * 512)]
        [InlineData(1024 * 1024)]
        [Theory(DisplayName = "Object-Simple-Buffer")]
        public async Task Test_SimpleObjAndBufferLength(int len)
        {
            Type type = typeof(ObjTestA);

            ObjTestA a = new ObjTestA()
            {
                A = 1,
                B = new string('A', len)
            };

            BinarySerializerOptions options = new BinarySerializerOptions() { DefaultBufferSize = 1 };
        
            Action<ObjTestA> check = b =>
            {
                Assert.Equal(a.A, b.A);
                Assert.Equal(a.B, b.B);
            };
            await Test(a, check, options);

        }
    }
}
