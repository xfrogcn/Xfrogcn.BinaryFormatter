﻿using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "Object")]
    public partial class ObjectTests : SerializerTestsBase
    {
       

        [Fact(DisplayName = "Object-Simple")]
        public async Task Test_SimpleObj()
        {

            ObjTestA a = new ObjTestA()
            {
                A = 1,
                B = null
            };

            await Test<ObjTestA>(null, (obj) => Assert.Null(obj));

            void check(ObjTestA b)
            {
                Assert.Equal(a.A, b.A);
                Assert.Equal(a.B, b.B);
            }
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

            void check(ObjTestA b)
            {
                Assert.Equal(a.A, b.A);
                Assert.Equal(a.B, b.B);
            }
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
                C = null,
                E = null
            };

            await Test<ObjTestB>(null, (obj) => Assert.Null(obj));

            void check(ObjTestB b)
            {
                Assert.Equal(a.A, b.A);
                Assert.Equal(a.B, b.B);
                if (a.C == null)
                {
                    Assert.Equal(a.C, b.C);
                }

                if (a.C != null)
                {
                    Assert.Equal(a.C.A, b.C.A);
                    Assert.Equal(a.C.B, b.C.B);
                }
            }
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
                },
                E = null
            };


            Action<ObjTestB> check = CheckProc(a);

            await Test(a, check, options);

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
                        B = new string('E', len),
                        E = 2,
                        C = new ObjTestB()
                        {
                            A = 5,
                            B = null,
                            E = null,
                            D = new ObjTestA()
                            {
                                A = 1,
                                B = new string('A', len)
                            }
                        }
                    },
                    E = 1
                },
                E = null
            };

            await Test(a, check, options);

        }

        [Fact(DisplayName = "Object-Polymorphic")]
        public async Task Test_Polymorphic()
        {

            ObjTestA a = new ObjTestB()
            {
                A = 1,
                B = null,
                E = 2
            };



            void check(ObjTestA b)
            {
                Assert.Equal(a.A, b.A);
                Assert.Equal(a.B, b.B);
            }
            await Test<ObjTestA>(a, check);

            //object c = new ObjTestB()
            //{
            //    A = 1
            //};
            //await Test<object>(c, b=>
            //{
            //    Assert.IsType<ObjTestB>(b);
            //    ObjTestB x = b as ObjTestB;
            //    Assert.Equal((uint)1, x.A);
            //});

            //c = new object();
            //await Test<object>(c, b =>
            //{
            //    Assert.True(b.GetType() == typeof(object));
            //});


           
        }

        [Fact(DisplayName = "Object-Polymorphic-Nest")]
        public async Task Test_Polymorphic_Nest()
        {
            ObjTestB n1 = new ObjTestB()
            {
                D = new ObjTestB()
                {
                    B = "D1"
                }
            };
            await Test(n1, CheckProc(n1));


            n1 = new ObjTestB()
            {
                A = 1,
                B = "A",
                C = new ObjTestB()
                {
                    A = 2,
                    D = new ObjTestB()
                    {
                        B = "C1",
                        E = 1,
                        C = new ObjTestB()
                        {

                        },
                        D = new ObjTestA()
                        {
                            A = 3,
                            B = "C2"
                        }
                    }
                },
                D = new ObjTestB()
                {
                    B = "D1",
                    D = new ObjTestA()
                    {
                        B = "A1",
                        A = 1
                    },
                    E = 1
                },
                E = null
            };
            await Test(n1, CheckProc(n1));
        }

        [InlineData(1024 * 10)]
        [InlineData(1024 * 512)]
        [InlineData(1024 * 1024)]
        [Theory(DisplayName = "Object-Polymorphic-Nest-Buffer")]
        public async Task Test_Polymorphic_Nest_Buffer(int len)
        {
            BinarySerializerOptions options = new BinarySerializerOptions() { DefaultBufferSize = 1 };


            ObjTestB n1 = new ObjTestB()
            {
                D = new ObjTestB()
                {
                    B = new string('A', len)
                }
            };
            await Test(n1, CheckProc(n1), options);

            options = new BinarySerializerOptions() { DefaultBufferSize = 30 };
            n1 = new ObjTestB()
            {
                A = 1,
                B = new string('A', len),
                C = new ObjTestB()
                {
                    A = 2,
                    D = new ObjTestB()
                    {
                        B = new string('C', len),
                        E = 1,
                        C = new ObjTestB()
                        {

                        },
                        D = new ObjTestA()
                        {
                            A = 3,
                            B = new string('D', len)
                        }
                    }
                },
                D = new ObjTestB()
                {
                    B = new string('E', len),
                    D = new ObjTestA()
                    {
                        B = new string('F', len),
                        A = 1
                    },
                    E = 1
                },
                E = null
            };
            await Test(n1, CheckProc(n1), options);
        }

        private Action<ObjTestB> CheckProc(ObjTestB a)
        {
            void check(ObjTestB b)
            {
                Assert.Equal(a.A, b.A);
                Assert.Equal(a.B, b.B);
                if (a.C == null)
                {
                    Assert.Equal(a.C, b.C);
                }
                if (a.D == null)
                {
                    Assert.Equal(a.D, b.D);
                }
                else
                {
                    Assert.Equal(a.D.GetType(), b.D.GetType());
                    Assert.Equal(a.D.A, b.D.A);
                    Assert.Equal(a.D.B, b.D.B);

                    if (a.D is ObjTestB)
                    {
                        var subCheck = CheckProc(a.D as ObjTestB);
                        subCheck(b.D as ObjTestB);
                    }
                }

                ObjTestB c1 = a.C;
                ObjTestB c2 = b.C;
                while (c1 != null)
                {
                    var subCheck = CheckProc(c1);
                    subCheck(c2);
                    c1 = c1.C;
                    c2 = c2.C;
                }


            }

            return check;
        }
    }
}
