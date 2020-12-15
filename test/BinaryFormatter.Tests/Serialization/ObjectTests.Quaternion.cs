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
       
        [Fact(DisplayName = "Object_Quaternion")]
        public async Task Object_Quaternion_Test()
        {
            Quaternion p = new Quaternion(new Vector3(new Vector2(1,2), 3.33f), 2.33f);
            
            await Test(p, (b)=>
            {
                Assert.Equal(p, b);
            });

        }

    }
}
