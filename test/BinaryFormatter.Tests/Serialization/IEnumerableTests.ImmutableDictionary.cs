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

        //[Fact(DisplayName = "Test_IDictionary_Dictionary_String_Buffer")]
        //public async Task Test_IDictionary_Dictionary_String_Buffer()
        //{
        //    Dictionary<string, string> a = new Dictionary<string, string>();
        //    a.Add(new string('A', 500), new string('A', 500));
        //    a.Add(new string('B', 500), new string('B', 500));
        //    a.Add(new string('C', 500), new string('C', 500));

        //    await Test(a,
        //       CheckIEnumerableOfIIDictionary(a,
        //       (a, b) => Assert.Equal(a, b),
        //       (a, b) => Assert.Equal(a, b)), new BinarySerializerOptions() { DefaultBufferSize = 1 });
        //}


        //[InlineData(500)]
        //[InlineData(1024*10)]
        //[InlineData(1024*512)]
        //[InlineData(1024 * 1024)]
        //[Theory(DisplayName = "Test_IDictionary_Dictionary_Complex_Buffer")]
        //public async Task Test_IDictionary_Dictionary_Complex_Buffer(int len)
        //{
        //    Dictionary<TestCtorA, TestCtorA> a = new Dictionary<TestCtorA, TestCtorA>();

        //    var key1 = createComplexCtorC(len);
        //    key1.B = 1;

        //    a.Add(key1, createComplexCtorC(len));
        //    var key2 = createComplexCtorC(len);
        //    key2.B = 2;
        //    a.Add(key2, createComplexCtorC(len));
        //    var key3 = new TestCtorA(new string('A', len), 3);
        //    a.Add(key3, new TestCtorB(new string('A', len), 3));



        //    await Test(a,
        //        CheckIEnumerableOfIIDictionary(a,
        //        (a, b) => checkCtorCProc(a)(b),
        //        (a, b) => checkCtorCProc(a)(b)), new BinarySerializerOptions() { DefaultBufferSize = 1 });

        //}

        //[InlineData(500)]
        //[InlineData(1024 * 10)]
        //[InlineData(1024 * 512)]
        //[InlineData(1024 * 1024)]
        //[Theory(DisplayName = "Test_IDIctionary_Dictionary_Nest_Buffer")]
        //public async Task Test_IDIctionary_Dictionary_Nest_Buffer(int len)
        //{
        //    Dictionary<string, Dictionary<string, string>> a
        //        = new Dictionary<string, Dictionary<string, string>>();
        //    a.Add(new string('A', len), new Dictionary<string, string>()
        //    {
        //        { new string('A',len), new string('A',len) },
        //        { new string('B',len), new string('B',len) }
        //    });
        //    a.Add(new string('B', len), new Dictionary<string, string>()
        //    {
        //        { new string('A',len), new string('A',len) },
        //        { new string('B',len), new string('B',len) }
        //    });
        //    a.Add(new string('C', len), new Dictionary<string, string>()
        //    {
        //        { new string('A',len), new string('A',len) },
        //        { new string('B',len), new string('B',len) }
        //    });

        //    await Test(a,
        //        CheckIEnumerableOfIIDictionary(a,
        //            (a, b) => Assert.Equal(a, b),
        //            (a, b) =>
        //            {
        //                CheckIEnumerableOfIIDictionary(a, 
        //                    (a,b)=>Assert.Equal(a,b), 
        //                    (a, b) => Assert.Equal(a, b))(b);
        //            }), new BinarySerializerOptions() { DefaultBufferSize = 1 });
        //}

        //class TestCustomDictioanry<Tkey, TValue> : IDictionary<Tkey, TValue>
        //{
        //    private readonly Dictionary<Tkey, TValue> _dic = new Dictionary<Tkey, TValue>();
        //    public string A { get; set; }
        //    public TValue this[Tkey key] { get => _dic[key]; set => _dic[key]=value; }

        //    public ICollection<Tkey> Keys => _dic.Keys;

        //    public ICollection<TValue> Values => _dic.Values;

        //    public int Count => _dic.Count;

        //    public bool IsReadOnly => false;

        //    public void Add(Tkey key, TValue value)
        //    {
        //        _dic.Add(key, value);
        //    }

        //    public void Add(KeyValuePair<Tkey, TValue> item)
        //    {
        //        _dic.Add(item.Key, item.Value);
        //    }

        //    public void Clear()
        //    {
        //        _dic.Clear();
        //    }

        //    public bool Contains(KeyValuePair<Tkey, TValue> item)
        //    {
        //        return ((IDictionary<Tkey, TValue>)_dic).Contains(item);
        //    }

        //    public bool ContainsKey(Tkey key)
        //    {
        //        return _dic.ContainsKey(key);
        //    }

        //    public void CopyTo(KeyValuePair<Tkey, TValue>[] array, int arrayIndex)
        //    {
        //         ((IDictionary<Tkey, TValue>)_dic).CopyTo(array, arrayIndex);
        //    }

        //    public IEnumerator<KeyValuePair<Tkey, TValue>> GetEnumerator()
        //    {
        //        return _dic.GetEnumerator();
        //    }

        //    public bool Remove(Tkey key)
        //    {
        //        return _dic.Remove(key);
        //    }

        //    public bool Remove(KeyValuePair<Tkey, TValue> item)
        //    {
        //        return ((IDictionary<Tkey, TValue>)_dic).Remove(item);
        //    }

        //    public bool TryGetValue(Tkey key, [MaybeNullWhen(false)] out TValue value)
        //    {
        //        return _dic.TryGetValue(key, out value);
        //    }

        //    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        //    {
        //        return _dic.GetEnumerator();
        //    }
        //}

        //[InlineData(500)]
        //[InlineData(1024 * 10)]

        //[Theory(DisplayName = "Test_IDictionary_Custom_Complex_Buffer")]
        //public async Task Test_IDictionary_Custom_Complex_Buffer(int len)
        //{
        //    TestCustomDictioanry<TestCtorA, TestCtorA> a = new TestCustomDictioanry<TestCtorA, TestCtorA>();

        //    var key1 = createComplexCtorC(len);
        //    key1.B = 1;

        //    a.Add(key1, createComplexCtorC(len));
        //    var key2 = createComplexCtorC(len);
        //    key2.B = 2;
        //    a.Add(key2, createComplexCtorC(len));
        //    var key3 = new TestCtorA(new string('A', len), 3);
        //    a.Add(key3, new TestCtorB(new string('A', len), 3));

        //    a.A = new string('A', len);

        //    await Test(a,
        //        b =>
        //        {
        //            CheckIEnumerableOfIIDictionary(a,
        //               (a, b) => checkCtorCProc(a)(b),
        //               (a, b) => checkCtorCProc(a)(b))(b);
        //            Assert.Equal(a.A, b.A);
        //        }
        //       , new BinarySerializerOptions() { DefaultBufferSize = 1 });

        //}

        //class TestWithIDictionary
        //{
        //    public string B { get; set; }
        //    public TestWithIDictionary A { get; set; }

        //    public IDictionary<string, IDictionary<string,string>> Skip { get; internal set; }

        //    public IDictionary<string,string> Dic { get; set; }
        //}

        //[InlineData(500)]
        //[InlineData(1024 * 10)]

        //[Theory(DisplayName = "Test_IDictionary_WithSkip_Complex_Buffer")]
        //public async Task Test_IDictionary_WithSkip_Complex_Buffer(int len)
        //{
        //    TestWithIDictionary a = new TestWithIDictionary();
        //    a.A = new TestWithIDictionary()
        //    {
        //        B = new string('A', len)
        //    };
        //    a.Skip = new Dictionary<string, IDictionary<string, string>>();
        //    a.Skip.Add(new string('A', len), new TestCustomDictioanry<string, string>()
        //    {
        //        A = new string('A', len)
        //    });
        //    a.Dic = new TestCustomDictioanry<string, string>()
        //    {
        //        A = new string('A', len)
        //    };

        //    await Test(a, CheckTestWithIDictionary(a), new BinarySerializerOptions() { DefaultBufferSize = 1 });

        //    a.Skip[new string('A', len)].Add(new string('B', len), new string('B', len));
        //    a.Dic.Add(new string('B', len), new string('B', len));

        //    await Test(a, CheckTestWithIDictionary(a), new BinarySerializerOptions() { DefaultBufferSize = 1 });

        //}

        //private Action<TestWithIDictionary> CheckTestWithIDictionary(TestWithIDictionary a)
        //{
        //    return b =>
        //    {
        //        Assert.Equal(a.B, b.B);
        //        if (a.A == null)
        //        {
        //            Assert.Null(b.A);
        //        }
        //        else
        //        {
        //            CheckTestWithIDictionary(a.A)(b.A);
        //        }

        //        Assert.Null(b.Skip);

        //        if(a.Dic == null)
        //        {
        //            Assert.Null(b.Dic);
        //        }
        //        else
        //        {
        //            Assert.Equal(a.Dic.GetType(), b.Dic.GetType());
        //            if(a.Dic is TestCustomDictioanry<string,string> cdic)
        //            {
        //                TestCustomDictioanry<string, string> cdic2 = b.Dic as TestCustomDictioanry<string, string>;
        //                Assert.Equal(cdic.A, cdic2.A);
        //            }
        //            CheckIEnumerableOfIIDictionary(a.Dic, (a, b) => Assert.Equal(a, b), (a, b) => Assert.Equal(a, b))(b.Dic);
        //        }
        //    };
        //}


        //protected virtual Action<IEnumerable<KeyValuePair<TKey, TValue>>> CheckIEnumerableOfIIDictionary<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> a, Action<TKey, TKey> keyChecker, Action<TValue,TValue> valueChecker)
        //{
        //    return (b) =>
        //    {
        //        Assert.Equal(a.GetType(), b.GetType());
        //        var e1 = a.GetEnumerator();
        //        var e2 = b.GetEnumerator();
        //        while (e1.MoveNext() && e2.MoveNext())
        //        {
        //            var a1 = e1.Current;
        //            var b1 = e2.Current;
        //            if (a1.Key == null)
        //            {
        //                Assert.Null(b1.Key);
        //            }
        //            else
        //            {
        //                Assert.Equal(a1.Key.GetType(), b1.Key.GetType());
        //                keyChecker(a1.Key, b1.Key);
        //            }

        //            if (a1.Value == null)
        //            {
        //                Assert.Null(b1.Value);
        //            }
        //            else
        //            {
        //                Assert.Equal(a1.Value.GetType(), b1.Value.GetType());
        //                valueChecker(a1.Value, b1.Value);
        //            }
        //        }
        //    };

        //}



    }
}
