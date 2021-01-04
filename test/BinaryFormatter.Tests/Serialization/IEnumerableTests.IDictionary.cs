using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Immutable;
using System.Collections;

namespace Xfrogcn.BinaryFormatter.Tests
{

    public partial class IEnumerableTests 
    {

        [InlineData(1)]
        [InlineData(127)]
        [InlineData(512)]
        [Theory(DisplayName = "Test_IDictionary_Hashtable")]
        public async Task Test_IDictionary_Hashtable(int len)
        {
            Hashtable a = new Hashtable
            {
                [0] = 0,
                [createComplexCtorC(len)] = createComplexCtorC(len)
            };
            await Test(a, b=>
            {
                Assert.Equal(a.Count, b.Count);
            }, new BinarySerializerOptions() { DefaultBufferSize = 1 });

        }


    }
}
