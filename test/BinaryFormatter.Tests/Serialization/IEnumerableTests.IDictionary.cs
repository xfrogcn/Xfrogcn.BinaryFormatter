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
            Hashtable a = new Hashtable();
            a[0] = 0;
            a[createComplexCtorC(len)] = createComplexCtorC(len);
            //CheckIEnumerableOfIEnumerable(a, (a, b) =>
            //{
            //    DictionaryEntry d1 = (DictionaryEntry)a;
            //    DictionaryEntry d2 = (DictionaryEntry)b;
            //    if (d1.Key is TestCtorA k1)
            //    {
            //        checkCtorCProc(k1)(d2.Key as TestCtorA);
            //    }
            //    else
            //    {
            //        Assert.Equal(d1.Key, d2.Key);
            //    }
            //    if (d1.Value is TestCtorA v1)
            //    {
            //        checkCtorCProc(v1)(d2.Value as TestCtorA);
            //    }
            //    else
            //    {
            //        Assert.Equal(d1.Value, d2.Value);
            //    }
            //}

            await Test(a, b=>
            {
                Assert.Equal(a.Count, b.Count);
            }, new BinarySerializerOptions() { DefaultBufferSize = 1 });

        }


    }
}
