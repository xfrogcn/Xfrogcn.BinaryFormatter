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
       
        [Fact(DisplayName = "Object_Plane")]
        public async Task Object_Plane_Test()
        {
            Plane p = new Plane(new Vector3(new Vector2(1,2), 3.33f), 2.33f);
            
            await Test(p, (b)=>
            {
                Assert.Equal(p, b);
            });

        }

    }
}
