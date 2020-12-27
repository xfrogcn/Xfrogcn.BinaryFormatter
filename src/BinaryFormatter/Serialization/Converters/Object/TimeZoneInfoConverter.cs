using System;
using System.Collections.Generic;
using System.Reflection;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class TimeZoneInfoConverter : ObjectWithCustomerCreatorConverter<TimeZoneInfo>
    {

        public override MethodInfo[] GetAdditionalDataMethod()
        {
            var mi = TypeToConvert.GetMethod(nameof(TimeZoneInfo.GetAdjustmentRules));
            return new MethodInfo[] { mi };
        }



        protected override object CreateObject(ref ReadStackFrame frame)
        {
            var args = frame.PropertyValueCache;
            string id = (string)args[nameof(TimeZoneInfo.Id)];

            try
            {
                TimeZoneInfo ti = TimeZoneInfo.FindSystemTimeZoneById(id);
                if (ti != null)
                {
                    return ti;
                }
            }
            catch (TimeZoneNotFoundException)
            {

            }
            return TimeZoneInfo.CreateCustomTimeZone(
                id,
                (TimeSpan)args[nameof(TimeZoneInfo.BaseUtcOffset)],
                (string)args[nameof(TimeZoneInfo.DisplayName)],
                (string)args[nameof(TimeZoneInfo.StandardName)],
                (string)args[nameof(TimeZoneInfo.DaylightName)],
                (TimeZoneInfo.AdjustmentRule[])args[nameof(TimeZoneInfo.GetAdjustmentRules)]
                );
        }

    
        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            base.SetTypeMetadata(typeInfo, typeMap, options);
            typeInfo.Type = TypeEnum.TimeZoneInfo;
            typeInfo.FullName = null;
        }

        protected override void InitializeCreatorArgumentCaches(ref ReadStack state, BinarySerializerOptions options)
        {
            // 设置属性
            BinaryClassInfo classInfo = state.Current.BinaryClassInfo;
            if (state.Current.PropertyValueCache == null)
            {
                for (int i = 0; i < classInfo.PropertyCacheArray.Length; i++)
                {
                    classInfo.PropertyCacheArray[i].ShouldDeserialize = true;
                }
                state.Current.PropertyValueCache = new Dictionary<string, object>();
            }
        }
    }
}
