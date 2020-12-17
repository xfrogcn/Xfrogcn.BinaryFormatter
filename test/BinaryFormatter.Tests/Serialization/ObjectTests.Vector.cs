using System.Numerics;
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

        [Fact(DisplayName = "Object_VectorOfT_byte_Test")]
        public async Task Object_VectorOfT_byte_Test()
        {
            byte[] data = new byte[Vector<byte>.Count];
            data[1] = 1;
            Vector<byte> a = new Vector<byte>(data);
           

            await Test(a, (b) =>
            {
                Assert.Equal(a, b);
            });
        }

        [Fact(DisplayName = "Object_VectorOfT_double_Test")]
        public async Task Object_VectorOfT_double_Test()
        {
            double[] data = new double[Vector<double>.Count];
            data[1] = 1.23;
            Vector<double> a = new Vector<double>(data);


            await Test(a, (b) =>
            {
                Assert.Equal(a, b);
            });
        }
    }
}
