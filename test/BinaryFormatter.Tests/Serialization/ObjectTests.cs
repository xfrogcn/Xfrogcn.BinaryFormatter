using System;
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

        class ObjTestB : ObjTestA
        {

            public ObjTestB C { get; set; }

            public ObjTestA D { get; set; }
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


        [Fact(DisplayName = "Object-Nest")]
        public async Task Test_NestObj()
        {
            Type type = typeof(ObjTestA);

            ObjTestB a = new ObjTestB()
            {
                A = 1,
                B = null,
                C = null
            };

            await Test<ObjTestB>(null, (obj) => Assert.Null(obj));

            Action<ObjTestB> check = b =>
            {
                Assert.Equal(a.A, b.A);
                Assert.Equal(a.B, b.B);
                if(a.C == null)
                {
                    Assert.Equal(a.C, b.C);
                }
                
                if(a.C != null)
                {
                    Assert.Equal(a.C.A, b.C.A);
                    Assert.Equal(a.C.B, b.C.B);
                }
            };
            await Test(a, check);


            a.B = "AAA";
            a.C = new ObjTestB()
            {
                A = 2,
                B = "222",
            };
            await Test(a, check);

            
        }

        [InlineData(1024 * 10)]
        [InlineData(1024 * 512)]
        [InlineData(1024 * 1024)]
        [Theory(DisplayName = "Object-Nest-Buffer")]
        public async Task Test_NestObjBuffer(int len)
        {
            BinarySerializerOptions options = new BinarySerializerOptions()
            {
                DefaultBufferSize = 1
            };
            ObjTestB a = new ObjTestB()
            {
                A = 1,
                B =new string('A', len),
                C = new ObjTestB()
                {
                    A = 1,
                    B = new string('B', len)
                }
            };

           
            Action<ObjTestB> check = b =>
            {
                Assert.Equal(a.A, b.A);
                Assert.Equal(a.B, b.B);
                if (a.C == null)
                {
                    Assert.Equal(a.C, b.C);
                }

                ObjTestB c1 = a.C;
                ObjTestB c2 = b.C;
                while (c1 != null )
                {
                    Assert.Equal(c1.A, c2.A);
                    Assert.Equal(c1.B, c2.B);
                    c1 = c1.C;
                    c2 = c2.C;
                }

               
            };
            await Test(a, check);

            a.C.C = new ObjTestB()
            {
                A = 2,
                B = new string('C', len),
                C = new ObjTestB()
                {
                    A = 3,
                    B = new string('D', len),
                    C = new ObjTestB()
                    {
                        A = 4,
                        B = new string('E', len)
                    }
                }
            };

            await Test(a, check);

        }
    }
}
