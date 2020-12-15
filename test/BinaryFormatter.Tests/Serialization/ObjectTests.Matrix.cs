using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Xfrogcn.BinaryFormatter.Tests
{
    public partial class ObjectTests
    {
       
        [Fact(DisplayName = "Object_Matrix3x2")]
        public async Task Object_Matrix3x2_Test()
        {
            Matrix3x2 m = new Matrix3x2(
                11,12,13,
                21,22,23
                );
            
            await Test(m, (b)=>
            {
                Assert.Equal(m, b);
            });

        }

        [Fact(DisplayName = "Object_Matrix4x4")]
        public async Task Object_Matrix4x4_Test()
        {
            Matrix4x4 m = new Matrix4x4(
                11,12,13,14,
                21,22,23,24,
                31,32,33,34,
                41,42,43,44
                );

            await Test(m, (b) =>
            {
                Assert.Equal(m, b);
            });

        }

    }
}
