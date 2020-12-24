using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{

    public partial class IEnumerableTests 
    {

        [InlineData(0)]
        [InlineData(127)]
        [InlineData(512)]
        [Theory(DisplayName = "Test_Immutable_Dictionary_Simple_Number")]
        public async Task Test_Immutable_Dictionary_Simple_Number(int len)
        {
            Dictionary<long, long> dic = new Dictionary<long, long>();
            for (long i = 0; i < len; i++)
            {
                dic.Add(i, i);
            }

            var a = ImmutableDictionary.CreateRange(dic);


            await Test(a,
                CheckIEnumerableOfIIDictionary(a,
                (a, b) => Assert.Equal(a, b),
                (a, b) => Assert.Equal(a, b)));

        }

        [InlineData(0)]
        [InlineData(127)]
        [InlineData(512)]
        [Theory(DisplayName = "Test_Immutable_Sorted_Dictionary_Simple_Number")]
        public async Task Test_Immutable_Sorted_Dictionary_Simple_Number(int len)
        {
            Dictionary<long, long> dic = new Dictionary<long, long>();
            for (long i = 0; i < len; i++)
            {
                dic.Add(i, i);
            }

            var a = ImmutableSortedDictionary.CreateRange(dic);


            await Test(a,
                CheckIEnumerableOfIIDictionary(a,
                (a, b) => Assert.Equal(a, b),
                (a, b) => Assert.Equal(a, b)));

        }


    }
}
