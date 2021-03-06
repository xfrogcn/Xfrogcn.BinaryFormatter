﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Immutable;
using System.Collections.Concurrent;

namespace Xfrogcn.BinaryFormatter.Tests
{

    public partial class IEnumerableTests 
    {

        [InlineData(0)]
        [InlineData(127)]
        [InlineData(512)]
        [Theory(DisplayName = "Test_ConcurrentDictionary_Simple_Number")]
        public async Task Test_ConcurrentDictionary_Simple_Number(int len)
        {
            ConcurrentDictionary<long, long> a = new ConcurrentDictionary<long,long>();
            for (long i = 0; i < len; i++)
            {
                a.TryAdd(i, i);
            }

            

            await Test(a, b=>
            {
                Assert.Equal(a.Count, b.Count);
            });

        }


    }
}
