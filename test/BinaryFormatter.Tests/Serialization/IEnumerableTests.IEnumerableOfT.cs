using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Immutable;
using System.Collections.Concurrent;
using System.Collections;

namespace Xfrogcn.BinaryFormatter.Tests
{

    public partial class IEnumerableTests 
    {

        /// <summary>
        /// 如果从IEnumerable<>接口实现集合类型，必须提供Add方法及Count/Length属性/字段
        /// </summary>
        class TestEnumerableOfTA : IEnumerable<TestCtorA>
        {
            private readonly List<TestCtorA> _list = new List<TestCtorA>();

            public void Add(TestCtorA item)
            {
                _list.Add(item);
            }

            public int Count => _list.Count;

            public IEnumerator<TestCtorA> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            public string A { get; set; }
        }

        [InlineData(0)]
        [InlineData(127)]
        [InlineData(512)]
        [Theory(DisplayName = "Test_IEnumerableOfT_Custom_Buffer")]
        public async Task Test_IEnumerableOfT_Custom_Buffer(int len)
        {
            TestEnumerableOfTA a = new TestEnumerableOfTA
            {
                A = new string('A', len)
            };
            a.Add(createComplexCtorC(len));
            a.Add(null);
            

            await Test(a, b=>
            {
                CheckIEnumerableOfIEnumerable(a, (a, b) => Assert.Equal(a, b));
                Assert.Equal(a.A, b.A);
            });

        }



    }
}
