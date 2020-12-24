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


        [Fact(DisplayName = "Object_TimeZoneInfo_AdjustmentRule")]
        public async Task Object_TimeZoneInfo_AdjustmentRule()
        {

            var a = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
                new DateTime(1, 1, 1, 0, 0, 0),
                new DateTime(2, 1, 1, 0, 0, 0),
                TimeSpan.FromHours(1),
                TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, 2, 0, 0), 3, 15),
                TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, 2, 0, 0), 6, 15)
                );
            await Test(a, (b) => Assert.Equal(a, b));
        }

        [Fact(DisplayName = "Object_TimeZoneInfo")]
        public async Task Object_TimeZoneInfo()
        {

            var a = TimeZoneInfo.Local;
            await Test(a, (b) => Assert.Equal(a, b));
        }
    }
}
