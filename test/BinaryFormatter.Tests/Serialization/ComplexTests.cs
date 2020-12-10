using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    [Trait("", "内置类型")]
    public class ComplexTests
    {

        [InlineData(double.MinValue, double.MinValue)]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(0, 0)]
        [InlineData(3.14, 3.14)]
        [Theory(DisplayName = "Complex")]
        public async Task Test(double real, double imaginary)
        {
            Complex input = new Complex(real, imaginary);
            MemoryStream ms = new MemoryStream();
            await BinarySerializer.SerializeAsync(ms, input);

            ms.Position = 0;

            Complex b =  await BinarySerializer.DeserializeAsync<Complex>(ms);
            Assert.Equal(input, b);

            ms.Position = 0;
            object b1 = await BinarySerializer.DeserializeAsync(ms);
            Assert.Equal(input, (Complex)b1);

        }

      

    }
}
