using System;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    public partial class ObjectTests
    {
        [Fact(DisplayName = "Tuple<>")]
        public async Task Tuple1_Test()
        {
            var a = Tuple.Create(1L);

            await Test(a, (b) => Assert.Equal(a, b));
        }

        [Fact(DisplayName = "Tuple<,>")]
        public async Task Tuple2_Test()
        {
            var a = Tuple.Create(1L, 2L);

            await Test(a, (b) => Assert.Equal(a, b));
        }

        [Fact(DisplayName = "Tuple<,,>")]
        public async Task Tuple3_Test()
        {
            var a = Tuple.Create(1L, 2L, 3L);

            await Test(a, (b) => Assert.Equal(a, b));
        }

        [Fact(DisplayName = "Tuple<,,,>")]
        public async Task Tuple4_Test()
        {
            var a = Tuple.Create(1L, 2L, 3L, 4L);

            await Test(a, (b) => Assert.Equal(a, b));
        }


        [Fact(DisplayName = "Tuple<,,,,,>")]
        public async Task Tuple5_Test()
        {
            var a = Tuple.Create(1L, 2L, 3L, 4L, 5L);

            await Test(a, (b) => Assert.Equal(a, b));
        }

        [Fact(DisplayName = "Tuple<,,,,,,>")]
        public async Task Tuple6_Test()
        {
            var a = Tuple.Create(1L, 2L, 3L, 4L, 5L,6M);

            await Test(a, (b) => Assert.Equal(a, b));
        }

        [Fact(DisplayName = "Tuple<,,,,,,,,>")]
        public async Task Tuple7_Test()
        {
            var a = Tuple.Create(1L, 2L, 3L, 4L, 5L, 6M, 7.0d);

            await Test(a, (b) => Assert.Equal(a, b));
        }

        [Fact(DisplayName = "Tuple<,,,,,,,,,>")]
        public async Task Tuple8_Test()
        {
            var a = Tuple.Create(1L, 2L, 3L, 4L, 5L, 6M, 7.0d, 8.0F);

            await Test(a, (b) => Assert.Equal(a, b));
        }


        [InlineData(500)]
        [InlineData(1024*10)]
        [Theory(DisplayName = "Tuple_Complex_Buffer")]
        public async Task Tuple_Complex_Buffer_Test(int len)
        {
            var a = Tuple.Create(1L, 2L, 3L, new object(),createComplexCtorC(len));

            await Test(a, (b) =>
            {
                Assert.Equal(a.Item1, b.Item1);
                Assert.Equal(a.Item2, b.Item2);
                Assert.Equal(a.Item3, b.Item3);
                Assert.Equal(typeof(object), b.Item4.GetType());
                checkCtorCProc(a.Item5)(b.Item5);
            }, new BinarySerializerOptions() { DefaultBufferSize = 1 });
        }
    }
}
