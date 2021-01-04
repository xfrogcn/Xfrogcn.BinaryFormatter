using System;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "Object")]
    public partial class ObjectTests : SerializerTestsBase
    {

        
        [Fact(DisplayName = "Object-Ctor-With-Self-Ref")]
        public async Task Test_Ctor_With_Self_Ref()
        {
            SelfRefWithCtorA a = new SelfRefWithCtorA("A");
            a.Self = a;

            await Test(a, b =>
            {
                Assert.True(Object.ReferenceEquals(b, b.Self));
            }, new BinarySerializerOptions() { DefaultBufferSize = 1 });
        }

        [Fact(DisplayName = "Object-Ctor-With-Parent-Child-Ref")]
        public async Task Test_Ctor_With_Parent_Child_Ref()
        {
            
            TestRefWithCtor p = new TestRefWithCtor(null) { A = "A" };
            TestRefWithCtor a = new TestRefWithCtor(p) { A = "B" };
           

            await Test(a, b =>
            {
                Assert.True(Object.ReferenceEquals(b.Parent.Child, b));
                Assert.True(Object.ReferenceEquals(b.Parent.Child.Parent, b.Parent));
                Assert.Equal("A", b.Parent.A);
                Assert.Equal("B", b.A);
            }, new BinarySerializerOptions() { DefaultBufferSize = 1 });
        }
    }
}
