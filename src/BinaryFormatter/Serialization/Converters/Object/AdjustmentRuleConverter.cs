using System;
using System.Collections.Generic;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class AdjustmentRuleConverter : ObjectWithCustomerCreatorConverter<TimeZoneInfo.AdjustmentRule>
    {

        protected override object CreateObject(ref ReadStackFrame frame)
        {
            var args = frame.PropertyValueCache;

            return TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
                (DateTime)args[nameof(TimeZoneInfo.AdjustmentRule.DateStart)],
                (DateTime)args[nameof(TimeZoneInfo.AdjustmentRule.DateEnd)],
                (TimeSpan)args[nameof(TimeZoneInfo.AdjustmentRule.DaylightDelta)],
                (TimeZoneInfo.TransitionTime)args[nameof(TimeZoneInfo.AdjustmentRule.DaylightTransitionStart)],
                (TimeZoneInfo.TransitionTime)args[nameof(TimeZoneInfo.AdjustmentRule.DaylightTransitionEnd)]
                );
        }

        

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            base.SetTypeMetadata(typeInfo, typeMap, options);
            typeInfo.Type = TypeEnum.AdjustmentRule;
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
