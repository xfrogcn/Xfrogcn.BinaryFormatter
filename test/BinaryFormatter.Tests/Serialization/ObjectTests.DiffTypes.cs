using System.Threading.Tasks;
using Xunit;
using Xfrogcn.BinaryFormatter;
using System.IO;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "Object")]
    public partial class ObjectTests : SerializerTestsBase
    {
        [Fact(DisplayName = "Diff_Simple")]
        public void Test_Diff_Simple()
        {
            ObjTestA a = new ObjTestA()
            {
                A = 1,
                B = "B"
            };
            byte[] data = BinarySerializer.Serialize(a);
            var b = BinarySerializer.Deserialize<DiffObjTestA>(data);
            Assert.Equal((uint)1, b.A);
            Assert.Equal("B", b.B);
        }

        [Fact(DisplayName = "Diff_Simple_Inherit")]
        public void Test_Diff_Simple_Inherit()
        {
            ObjTestA a = new ObjTestA()
            {
                A = 1,
                B = "B"
            };
            byte[] data = BinarySerializer.Serialize(a);
            var b = BinarySerializer.Deserialize<DiffObjTestB>(data);
            Assert.Equal((uint)1, b.A);
            Assert.Equal("B", b.B);
        }


        [Fact(DisplayName = "Diff_Self_Ref")]
        public void Test_Diff_Self_Ref()
        {
            TestSelfRefA a = new TestSelfRefA();
            a.Self = a;

            byte[] data = BinarySerializer.Serialize(a);
            var b = BinarySerializer.Deserialize<DiffTestSelfRefA>(data);
            Assert.True(object.ReferenceEquals(b, b.Self));
        }

        [Fact(DisplayName = "Diff_Obj_Complex")]
        public void Test_Diff_Obj_Complex()
        {
            ObjTestB a = new ObjTestB()
            {
                A = 1,
                B = "B",
                C = new ObjTestB()
                {
                    A = 2,
                    B = "B"
                }
            };


            byte[] data = BinarySerializer.Serialize(a);
            var b = BinarySerializer.Deserialize<DiffObjTestB>(data);
            Assert.Equal((uint)1, b.A);
            Assert.Equal("B", b.B);
            Assert.Equal("B", b.C.B);
        }

        [Fact(DisplayName = "Diff_Obj_Complex2")]
        public async Task Test_Diff_Obj_Complex2()
        {
            var a = createComplexCtorC(1);

            byte[] data = BinarySerializer.Serialize(a);
            var b = BinarySerializer.Deserialize<DiffObjTestB>(data);
            Assert.NotNull(b);

            MemoryStream ms = new MemoryStream();
            await BinarySerializer.SerializeAsync(ms, a);
            ms.Position = 0;

            b = await BinarySerializer.DeserializeAsync<DiffObjTestB>(ms);
            Assert.NotNull(b);
        }


        [Fact(DisplayName = "Diff_Obj_Ctor_Complex")]
        public async Task Test_Diff_Obj_Ctor_Complex()
        {
            var a = createComplexCtorC(1);

            byte[] data = BinarySerializer.Serialize(a);
            var b = BinarySerializer.Deserialize<DiffTestCtorC>(data);
            Assert.NotNull(b);

            MemoryStream ms = new MemoryStream();
            await BinarySerializer.SerializeAsync(ms, a);
            ms.Position = 0;

            b = await BinarySerializer.DeserializeAsync<DiffTestCtorC>(ms);
            Assert.NotNull(b);
        }
    }
}
