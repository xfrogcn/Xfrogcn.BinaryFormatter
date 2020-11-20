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
        [InlineData(typeof(Byte),0, TypeEnum.Byte, false, 0, 0)]
        [InlineData(typeof(Int16), 0, TypeEnum.Int16, false, 0, 0)]
        [InlineData(typeof(Int32), 0, TypeEnum.Int32, false, 0, 0)]
        [InlineData(typeof(Int64), 0, TypeEnum.Int64, false, 0, 0)]
        [InlineData(typeof(SByte), 0, TypeEnum.SByte, false, 0, 0)]
        [InlineData(typeof(UInt16), 0, TypeEnum.UInt16, false, 0, 0)]
        [InlineData(typeof(UInt32), 0, TypeEnum.UInt32, false, 0, 0)]
        [InlineData(typeof(UInt64), 0, TypeEnum.UInt64, false, 0, 0)]
        [InlineData(typeof(Single), 0, TypeEnum.Single, false, 0, 0)]
        [InlineData(typeof(Double), 0, TypeEnum.Double, false, 0, 0)]
        [InlineData(typeof(Decimal), 0, TypeEnum.Decimal, false, 0, 0)]
        [InlineData(typeof(BigInteger), 0, TypeEnum.BigInteger, false, 0, 0)]
        [Theory(DisplayName = "基础数字类型")]
        public void Test1(Type type, ushort seq, TypeEnum te, bool isGeneric, sbyte genericArgumentsCount, int memberCount)
        {
            var sp  = new ServiceCollection().BuildServiceProvider();

            DefaultMetadataProvider provider = new DefaultMetadataProvider(sp, null);

            var ti = provider.GetTypeMap(type).GetTypeInfo(seq);

            Assert.Equal(te, ti.Type);
            Assert.Equal(isGeneric, ti.IsGeneric);
            Assert.Equal(genericArgumentsCount, ti.GenericArgumentCount);
            Assert.Equal(memberCount, ti.Members.Length);

           
        }

        [InlineData(typeof(Vector<byte>), 0, TypeEnum.VectorT, true, 1, 0)]
        [InlineData(typeof(Vector<float>), 0, TypeEnum.VectorT, true, 1, 0)]
        [Theory(DisplayName = "Vector<T>类型")]
        public void Test2(Type type, ushort seq, TypeEnum te, bool isGeneric, sbyte genericArgumentsCount, int memberCount)
        {
            var sp = new ServiceCollection().BuildServiceProvider();

            DefaultMetadataProvider provider = new DefaultMetadataProvider(sp, null);

            var ti = provider.GetTypeMap(type).GetTypeInfo(seq);

            Assert.Equal(te, ti.Type);
            Assert.Equal(isGeneric, ti.IsGeneric);
            Assert.Equal(genericArgumentsCount, ti.GenericArgumentCount);
            Assert.Equal(memberCount, ti.Members.Length);
            


        }

        [InlineData(typeof(Vector2), 0, TypeEnum.Vector2, false, 0, 2)]
        [InlineData(typeof(Vector3), 0, TypeEnum.Vector3, false, 0, 3)]
        [InlineData(typeof(Vector4), 0, TypeEnum.Vector4, false, 0, 4)]
        [Theory(DisplayName = "Vector类型")]
        public void Test3(Type type, ushort seq, TypeEnum te, bool isGeneric, sbyte genericArgumentsCount, int memberCount)
        {
            var sp = new ServiceCollection().BuildServiceProvider();

            DefaultMetadataProvider provider = new DefaultMetadataProvider(sp, null);

            var ti = provider.GetTypeMap(type).GetTypeInfo(seq);

            Assert.Equal(te, ti.Type);
            Assert.Equal(isGeneric, ti.IsGeneric);
            Assert.Equal(genericArgumentsCount, ti.GenericArgumentCount);
            Assert.Equal(memberCount, ti.Members.Length);
        }

        [InlineData(typeof(Matrix3x2), 0, TypeEnum.Matrix3x2, false, 0, 6)]
        [InlineData(typeof(Matrix4x4), 0, TypeEnum.Matrix4x4, false, 0, 16)]
        [Theory(DisplayName = "Matrix类型")]
        public void Test4(Type type, ushort seq, TypeEnum te, bool isGeneric, sbyte genericArgumentsCount, int memberCount)
        {
            var sp = new ServiceCollection().BuildServiceProvider();

            DefaultMetadataProvider provider = new DefaultMetadataProvider(sp, null);

            var ti = provider.GetTypeMap(type).GetTypeInfo(seq);

            Assert.Equal(te, ti.Type);
            Assert.Equal(isGeneric, ti.IsGeneric);
            Assert.Equal(genericArgumentsCount, ti.GenericArgumentCount);
            Assert.Equal(memberCount, ti.Members.Length);
        }

        [InlineData(typeof(Plane), 0, TypeEnum.Plane, false, 0, 2)]
        [InlineData(typeof(Quaternion), 0, TypeEnum.Quaternion, false, 0, 4)]
        [Theory(DisplayName = "PlanAndQuaternion类型")]
        public void Test5(Type type, ushort seq, TypeEnum te, bool isGeneric, sbyte genericArgumentsCount, int memberCount)
        {
            var sp = new ServiceCollection().BuildServiceProvider();

            DefaultMetadataProvider provider = new DefaultMetadataProvider(sp, null);

            var map = provider.GetTypeMap(type);

            var ti = map.GetTypeInfo(seq);

            Assert.Equal(te, ti.Type);
            Assert.Equal(isGeneric, ti.IsGeneric);
            Assert.Equal(genericArgumentsCount, ti.GenericArgumentCount);
            Assert.Equal(memberCount, ti.Members.Length);
        }
    }
}
