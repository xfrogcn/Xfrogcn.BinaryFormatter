using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{

    public partial class IEnumerableTests 
    {
        class TestCustomDictionaryA<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
        {
            protected Dictionary<TKey, TValue> dic = new Dictionary<TKey, TValue>();

            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            {
                return dic.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return dic.GetEnumerator();
            }
        }

        class TestCustomDictionaryB<TKey,TValue>: TestCustomDictionaryA<TKey, TValue>
        {
            public void Add(TKey key,TValue value)
            {
                dic.Add(key, value);
            }
        }

        [InlineData(0)]
        [InlineData(10)]
        [Theory(DisplayName = "Test_Custom_Dictionary_Add")]
        public async Task Test_Custom_Dictionary_Add(int len)
        {
            TestCustomDictionaryB<long, long> a = new TestCustomDictionaryB<long, long>();
            for (long i = 0; i < len; i++)
            {
                a.Add(i, i);
            }


            await Test(a,
                CheckIEnumerableOfIIDictionary(a,
                (a, b) => Assert.Equal(a, b),
                (a, b) => Assert.Equal(a, b)));

        }


        class TestCustomDictionaryC<TKey, TValue> : TestCustomDictionaryA<TKey, TValue>
        {
            public void Add(KeyValuePair<TKey,TValue> item)
            {
                dic.Add(item.Key, item.Value);
            }
        }

        [InlineData(0)]
        [InlineData(10)]
        [Theory(DisplayName = "Test_Custom_Dictionary_Add_KeyValuePair")]
        public async Task Test_Custom_Dictionary_Add_KeyValuePair(int len)
        {
            TestCustomDictionaryC<long, long> a = new TestCustomDictionaryC<long, long>();
            for (long i = 0; i < len; i++)
            {
                a.Add(new KeyValuePair<long, long>(i,i));
            }


            await Test(a,
                CheckIEnumerableOfIIDictionary(a,
                (a, b) => Assert.Equal(a, b),
                (a, b) => Assert.Equal(a, b)));

        }


        class TestCustomDictionaryD<TKey, TValue> : TestCustomDictionaryA<TKey, TValue>
        {
            public TestCustomDictionaryD(IEnumerable<KeyValuePair<TKey,TValue>> dic)
            {
                this.dic = new Dictionary<TKey, TValue>(dic);
            }
        }

        [InlineData(0)]
        [InlineData(10)]
        [Theory(DisplayName = "Test_Custom_Dictionary_Ctor")]
        public async Task Test_Custom_Dictionary_Ctor(int len)
        {
            Dictionary<long, long> dic = new Dictionary<long, long>();
            for (long i = 0; i < len; i++)
            {
                dic.Add(i, i);
            }

            TestCustomDictionaryD<long, long> a =
                new TestCustomDictionaryD<long, long>(dic);

            await Test(a,
                CheckIEnumerableOfIIDictionary(a,
                (a, b) => Assert.Equal(a, b),
                (a, b) => Assert.Equal(a, b)));

        }
    }
}
