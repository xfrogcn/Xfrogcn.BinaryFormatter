using System;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;
using static System.TimeZoneInfo;

namespace Xfrogcn.BinaryFormatter.Tests
{
    public partial class ObjectTests
    {
       
        [Fact(DisplayName = "Object_TimeZoneInfo_TransitionTime")]
        public async Task Object_TimeZoneInfo_TransitionTime()
        {

            var a = TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, 2, 0, 0), 3, 15);
            await Test(a, (b) => Assert.Equal(a, b));
        }

    }
}
