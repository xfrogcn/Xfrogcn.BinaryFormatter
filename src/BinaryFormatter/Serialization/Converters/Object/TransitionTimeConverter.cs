using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class TransitionTimeConverter : ObjectWithCustomerCreatorConverter<TimeZoneInfo.TransitionTime>
    {
        protected override object CreateObject(ref ReadStackFrame frame)
        {
            var args = frame.PropertyValueCache;
            bool isFixedDateRule = (bool)args[nameof(TimeZoneInfo.TransitionTime.IsFixedDateRule)];
            if (isFixedDateRule)
            {
                return TimeZoneInfo.TransitionTime.CreateFixedDateRule(
                    (DateTime)args[nameof(TimeZoneInfo.TransitionTime.TimeOfDay)],
                    (int)args[nameof(TimeZoneInfo.TransitionTime.Month)],
                    (int)args[nameof(TimeZoneInfo.TransitionTime.Day)]
                    );
            }
            return TimeZoneInfo.TransitionTime.CreateFloatingDateRule(
                (DateTime)args[nameof(TimeZoneInfo.TransitionTime.TimeOfDay)],
                    (int)args[nameof(TimeZoneInfo.TransitionTime.Month)],
                    (int)args[nameof(TimeZoneInfo.TransitionTime.Day)],
                    (DayOfWeek)args[nameof(TimeZoneInfo.TransitionTime.DayOfWeek)]
                );
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            base.SetTypeMetadata(typeInfo, typeMap, options);
            typeInfo.Type = TypeEnum.TransitionTime;
            typeInfo.FullName = null;
        }

        protected override void InitializeCreatorArgumentCaches(ref ReadStack state, BinarySerializerOptions options)
        {
            // 设置属性
            BinaryClassInfo classInfo = state.Current.BinaryClassInfo;
            if (state.Current.PropertyValueCache == null)
            {
                for (int i=0; i < classInfo.PropertyCacheArray.Length; i++)
                {
                    classInfo.PropertyCacheArray[i].ShouldDeserialize = true;
                }
                state.Current.PropertyValueCache = new Dictionary<string, object>();
            }
        }

    }
}
