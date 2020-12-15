using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    public partial class ObjectTests
    {
       
        [Fact(DisplayName = "Object_Simple_KeyValuePair_Ctor")]
        public async Task Test_Simple_KeyValuePair_Ctor()
        {
            KeyValuePair<string,string> kv = new KeyValuePair<string, string>("KEY","VALUE");

            await Test(kv, (b)=>
            {
                Assert.Equal(kv.Key, b.Key);
                Assert.Equal(kv.Value, b.Value);
            }, largCtorOptions);

        }

        [InlineData(1024 * 10)]
        [InlineData(1024 * 512)]
        [InlineData(1024 * 1024)]
        [Theory(DisplayName = "Object_Complex_KeyValuePair_Ctor_Buffer")]
        public async Task Test_Complex_KeyValuePair_Ctor_Buffer(int len)
        {
            BinarySerializerOptions options = new BinarySerializerOptions() { DefaultBufferSize = 1 };

            TestCtorC cKey = createComplexCtorC(len);
            TestCtorC cValue = createComplexCtorC(len);

            var kv = new KeyValuePair<TestCtorA, TestCtorA>(cKey, cValue);

            await Test(kv, (b) =>
            {
                checkCtorCProc(kv.Key)(b.Key);
                checkCtorCProc(kv.Value)(b.Value);
            }, options);

        }

    }
}
