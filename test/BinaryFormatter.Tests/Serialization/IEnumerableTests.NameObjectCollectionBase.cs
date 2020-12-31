using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Immutable;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Web;

namespace Xfrogcn.BinaryFormatter.Tests
{

    public partial class IEnumerableTests 
    {

        [InlineData(1)]
        [InlineData(500)]
        [InlineData(1024)]
        [Theory(DisplayName = "Test_NameValueCollection ")]
        public async Task Test_NameValueCollection(int len)
        {
            NameValueCollection a = new NameValueCollection();
            a[new string('A',len)] = new string('A', len);
            a[new string('B', len)] = new string('B', len);
            a[new string('C', len)] = new string('C', len);

            await Test(a, b=>
            {
                Assert.Equal(a.Count, b.Count);
            }, new BinarySerializerOptions() { DefaultBufferSize = 1 });
            

        }


    }
}
