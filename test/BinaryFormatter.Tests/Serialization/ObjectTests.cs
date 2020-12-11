using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "Object")]
    public class ObjectTests
    {
        
       
        public async Task Test<T>(T input, Action<T> check)
        {
            MemoryStream ms = new MemoryStream();
            await BinarySerializer.SerializeAsync(ms, input);

            ms.Position = 0;

            T b =  await BinarySerializer.DeserializeAsync<T>(ms);
            check(b);

            ms.Position = 0;
            object b1 = await BinarySerializer.DeserializeAsync(ms);
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
            ObjTestA a = new ObjTestA()
            {
                A = 1,
                B = null
            };

            Action<ObjTestA> check = b =>
            {
                Assert.Equal(a.A, b.A);
                Assert.Equal(a.B, b.B);
            };
            await Test(a, check);

        }

        
    }
}
