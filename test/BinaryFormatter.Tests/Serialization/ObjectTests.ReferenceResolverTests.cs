using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "Object")]
    public partial class ObjectTests : SerializerTestsBase
    {

        [InlineData(1)]
        [InlineData(512)]
        [InlineData(1024*10)]
        [Theory(DisplayName = "Object-SelfRef")]
        public async Task Test_SelfRef(int len)
        {

            TestSelfRefA a = new TestSelfRefA();

            a.A = new string('A', len);

            a.Self = a;

            await Test(a, b =>
            {
                Assert.True(Object.ReferenceEquals(b, b.Self));
                Assert.Equal(a.A, b.A);
                Assert.True(Object.ReferenceEquals(b.Self, b.Self.Self));
            }, new BinarySerializerOptions() { DefaultBufferSize = 1 });
           
        }


        [InlineData(1)]
        [InlineData(512)]
        [InlineData(1024 * 10)]
        [Theory(DisplayName = "Object-SelfRef-Array")]
        public async Task Test_SelfRef_Array(int len)
        {

            TestSelfRefA a = new TestSelfRefA();

            a.A = new string('A', len);

            a.Self = a;

            TestSelfRefA[] array = new TestSelfRefA[] { a, a, a };

            await Test(array, b =>
            {
                Assert.True(Object.ReferenceEquals(b[0], b[1]));
                Assert.True(Object.ReferenceEquals(b[0], b[2]));
                Assert.Equal(a.A, b[0].A);
                Assert.True(Object.ReferenceEquals(b[0].Self, b[0].Self.Self));
            }, new BinarySerializerOptions() { DefaultBufferSize = 1 });

        }

        [Fact(DisplayName = "Object-Array-Ref")]
        public async Task Test_Array_Ref()
        {
            int[] array = new int[] { 1, 2, 3 };
            TestRef<int[]> a = new TestRef<int[]>();
            a.A = array;
            a.B = array;

            await Test(a, b =>
            {
                Assert.True(Object.ReferenceEquals(b.A, b.B));
                Assert.Equal(array, b.A);
            }, new BinarySerializerOptions() { DefaultBufferSize = 1 });
        }

        [Fact(DisplayName = "Object-Dictionary-Ref")]
        public async Task Test_Dictionary_Ref()
        {
            Dictionary<int, int> dic = new Dictionary<int, int>()
            {
                {1,1 },{2,2},{3,3}
            };
            TestRef<Dictionary<int, int>> a = new TestRef<Dictionary<int, int>>();
            a.A = dic;
            a.B = dic;

            await Test(a, b =>
            {
                Assert.True(Object.ReferenceEquals(b.A, b.B));
                Assert.Equal(dic, b.A);
            }, new BinarySerializerOptions() { DefaultBufferSize = 1 });
        }

        [Fact(DisplayName = "Object-Ctor-With-Parameter-Simple-Ref")]
        public async Task Test_Ctor_With_Parameter_Simple_Ref()
        {
            TestCtorA item = new TestCtorA("A", 1);
            
            TestRef<TestCtorA> a = new TestRef<TestCtorA>();
            a.A = item;
            a.B = item;

            await Test(a, b =>
            {
                Assert.True(Object.ReferenceEquals(b.A, b.B));
                checkCtorCProc(item)(b.A);
            }, new BinarySerializerOptions() { DefaultBufferSize = 1 });
        }

        //[Fact(DisplayName = "Object-With-Customer-Creator-Simple-Ref")]
        //public async Task Test_With_Customer_Creator_Simple_Ref()
        //{
        //    var item = TimeZoneInfo.Local;

        //    TestRef<TimeZoneInfo> a = new TestRef<TimeZoneInfo>();
        //    a.A = item;
        //    a.B = item;

        //    await Test(a, b =>
        //    {
        //        Assert.True(Object.ReferenceEquals(b.A, b.B));
        //    }, new BinarySerializerOptions() { DefaultBufferSize = 1 });
        //}
    }
}
