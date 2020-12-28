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
    }
}
