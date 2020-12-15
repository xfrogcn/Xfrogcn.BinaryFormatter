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
       
        [Fact(DisplayName = "Object_Vector2")]
        public async Task Object_Vector2_Test()
        {
            Vector2 v2 = new Vector2(1,2);
            
            await Test(v2, (b)=>
            {
                Assert.Equal(v2.X, b.X);
                Assert.Equal(v2.Y, b.Y);
            });

        }

        [Fact(DisplayName = "Object_Vector3")]
        public async Task Object_Vector3_Test()
        {
            Vector3 v3 = new Vector3(1, 2,3);

            await Test(v3, (b) =>
            {
                Assert.Equal(v3.X, b.X);
                Assert.Equal(v3.Y, b.Y);
                Assert.Equal(v3.Z, b.Z);
            });

        }

        [Fact(DisplayName = "Object_Vector4")]
        public async Task Object_Vector4_Test()
        {
            Vector4 v4 = new Vector4(1, 2, 3,4);

            await Test(v4, (b) =>
            {
                Assert.Equal(v4.X, b.X);
                Assert.Equal(v4.Y, b.Y);
                Assert.Equal(v4.Z, b.Z);
                Assert.Equal(v4.W, b.W);
            });

        }
    }
}
