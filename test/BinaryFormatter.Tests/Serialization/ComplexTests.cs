using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class ComplexTests : SerializerTestsBase
    {

        [InlineData(double.MinValue, double.MinValue)]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(0, 0)]
        [InlineData(3.14, 3.14)]
        [Theory(DisplayName = "Complex")]
        public async Task Test1(double real, double imaginary)
        {
            Complex input = new Complex(real, imaginary);
            await Test(input);

        }

      

    }
}
