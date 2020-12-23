using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{

    public partial class IEnumerableTests 
    {

        [InlineData(0)]
        [InlineData(127)]
        [InlineData(512)]
        [Theory(DisplayName = "Test_IDictionary_Dictionary_Simple_Number")]
        public async Task Test_IDictionary_Dictionary_Simple_Number(int len)
        {
            Dictionary<long, long> a = new Dictionary<long, long>();
            for (long i = 0; i < len; i++)
            {
                a.Add(i, i);
            }



            await Test(a,
                CheckIEnumerableOfIIDictionary(a,
                (a, b) => Assert.Equal(a, b),
                (a, b) => Assert.Equal(a, b)));

        }


        [InlineData(500)]
        [InlineData(1024*10)]
        [InlineData(1024*512)]
        [InlineData(1024 * 1024)]
        [Theory(DisplayName = "Test_IDictionary_Dictionary_Complex_Buffer")]
        public async Task Test_IDictionary_Dictionary_Complex_Buffer(int len)
        {
            Dictionary<TestCtorA, TestCtorA> a = new Dictionary<TestCtorA, TestCtorA>();

            var key1 = createComplexCtorC(len);
            key1.B = 1;

            a.Add(key1, createComplexCtorC(len));
            var key2 = createComplexCtorC(len);
            key2.B = 2;
            a.Add(key2, createComplexCtorC(len));
            var key3 = new TestCtorA(new string('A', len), 3);
            a.Add(key3, new TestCtorB(new string('A', len), 3));



            await Test(a,
                CheckIEnumerableOfIIDictionary(a,
                (a, b) => checkCtorCProc(a)(b),
                (a, b) => checkCtorCProc(a)(b)));

        }


        protected virtual Action<IEnumerable<KeyValuePair<TKey, TValue>>> CheckIEnumerableOfIIDictionary<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> a, Action<TKey, TKey> keyChecker, Action<TValue,TValue> valueChecker)
        {
            return (b) =>
            {
                Assert.Equal(a.GetType(), b.GetType());
                var e1 = a.GetEnumerator();
                var e2 = b.GetEnumerator();
                while (e1.MoveNext() && e2.MoveNext())
                {
                    var a1 = e1.Current;
                    var b1 = e2.Current;
                    if (a1.Key == null)
                    {
                        Assert.Null(b1.Key);
                    }
                    else
                    {
                        Assert.Equal(a1.Key.GetType(), b1.Key.GetType());
                        keyChecker(a1.Key, b1.Key);
                    }

                    if (a1.Value == null)
                    {
                        Assert.Null(b1.Value);
                    }
                    else
                    {
                        Assert.Equal(a1.Value.GetType(), b1.Value.GetType());
                        valueChecker(a1.Value, b1.Value);
                    }
                }
            };

        }



    }
}
