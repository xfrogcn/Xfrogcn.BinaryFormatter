using Microsoft.Extensions.DependencyInjection;
using System;
using System.Numerics;
using Xfrogcn.BinaryFormatter;
using Xfrogcn.BinaryFormatter.Metadata;
using Xunit;

namespace BinaryFormatter.Tests
{
    [Trait("", "Numberic")]
    public class NumbericTests
    {
        [InlineData(typeof(Byte),0, TypeEnum.Byte, false, 0, null)]
        [InlineData(typeof(Int16), 0, TypeEnum.Int16, false, 0, null)]
        [InlineData(typeof(Int32), 0, TypeEnum.Int32, false, 0, null)]
        [InlineData(typeof(Int64), 0, TypeEnum.Int64, false, 0, null)]
        [InlineData(typeof(SByte), 0, TypeEnum.SByte, false, 0, null)]
        [InlineData(typeof(UInt16), 0, TypeEnum.UInt16, false, 0, null)]
        [InlineData(typeof(UInt32), 0, TypeEnum.UInt32, false, 0, null)]
        [InlineData(typeof(UInt64), 0, TypeEnum.UInt64, false, 0, null)]
        [InlineData(typeof(Single), 0, TypeEnum.Single, false, 0, null)]
        [InlineData(typeof(Double), 0, TypeEnum.Double, false, 0, null)]
        [InlineData(typeof(Decimal), 0, TypeEnum.Decimal, false, 0, null)]
        [Theory(DisplayName = "基础数字类型")]
        public void Test1(Type type, ushort seq, TypeEnum te, bool isGeneric, sbyte genericArgumentsCount, BinaryMemberInfo[] member)
        {
            var sp  = new ServiceCollection().BuildServiceProvider();

            DefaultMetadataProvider provider = new DefaultMetadataProvider(sp, null);

            var ti = provider.GetTypeMap(type).GetTypeInfo(seq);

            Assert.Equal(te, ti.Type);
            Assert.Equal(isGeneric, ti.IsGeneric);
            Assert.Equal(genericArgumentsCount, ti.GenericArgumentCount);
            Assert.Equal(member, ti.Members);

            Vector<Vector2> v = new Vector<Vector2>();
            int c = Vector<Vector2>.Count;
        }
    }
}
