using System;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;
using static System.TimeZoneInfo;
using System.Linq;

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
           
   
            //await Test(a, (b) =>
            //{
            //    Assert.Equal(a.Id, b.Id);
            //    Assert.Equal(a.BaseUtcOffset, b.BaseUtcOffset);
            //    Assert.Equal(a.DisplayName, b.DisplayName);
            //    Assert.Equal(a.StandardName, b.StandardName);
            //    Assert.Equal(a.DaylightName, b.DaylightName);
            //    Assert.Equal(a.SupportsDaylightSavingTime, b.SupportsDaylightSavingTime);
            //    Assert.Equal(a.GetAdjustmentRules().Length, b.GetAdjustmentRules().Length);
            //});

            //a = TimeZoneInfo.Utc;
            //await Test(a, (b) =>
            //{
            //    Assert.Equal(a.Id, b.Id);
            //    Assert.Equal(a.BaseUtcOffset, b.BaseUtcOffset);
            //    Assert.Equal(a.DisplayName, b.DisplayName);
            //    Assert.Equal(a.StandardName, b.StandardName);
            //    Assert.Equal(a.DaylightName, b.DaylightName);
            //    Assert.Equal(a.SupportsDaylightSavingTime, b.SupportsDaylightSavingTime);
            //    Assert.Equal(a.GetAdjustmentRules().Length, b.GetAdjustmentRules().Length);
            //});

            var zones = TimeZoneInfo.GetSystemTimeZones();
            a = zones.OrderByDescending(z => z.GetAdjustmentRules().Length).First();
            var rules = a.GetAdjustmentRules();
            await Test(a, (b) =>
            {
                Assert.Equal(a.Id, b.Id);
                Assert.Equal(a.BaseUtcOffset, b.BaseUtcOffset);
                Assert.Equal(a.DisplayName, b.DisplayName);
                Assert.Equal(a.StandardName, b.StandardName);
                Assert.Equal(a.DaylightName, b.DaylightName);
                Assert.Equal(a.SupportsDaylightSavingTime, b.SupportsDaylightSavingTime);
                Assert.Equal(a.GetAdjustmentRules().Length, b.GetAdjustmentRules().Length);
            });
        }
    }
}
