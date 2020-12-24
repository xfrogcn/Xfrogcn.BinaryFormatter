using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class TransitionTimeConverter : LargeObjectWithParameterizedConstructorConverter<TimeZoneInfo.TransitionTime>
    {
        internal override bool ConstructorIsParameterized => false;
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(TimeZoneInfo.TransitionTime);
        }

        protected override void SetCtorArguments(ref ReadStack state, BinaryParameterInfo binaryParameterInfo, ref object arg)
        {
            ((Dictionary<string, object>)state.Current.CtorArgumentState!.Arguments)[binaryParameterInfo.NameAsString] = arg!;
        }

        protected override object CreateObject(ref ReadStackFrame frame)
        {
            var args = (Dictionary<string, object>)frame.CtorArgumentState!.Arguments;
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

        protected override void InitializeConstructorArgumentCaches(ref ReadStack state, BinarySerializerOptions options)
        {
            BinaryClassInfo classInfo = state.Current.BinaryClassInfo;

            if (classInfo.ParameterCache == null)
            {
                var parameterCache = new Dictionary<string, BinaryParameterInfo>(
                6, StringComparer.OrdinalIgnoreCase);

                foreach (BinaryPropertyInfo pi in classInfo.PropertyCacheArray)
                {
                    var par = pi.ConverterBase.CreateBinaryParameterInfo();
                    par.Initialize(
                        pi.TypeMap,
                        pi.RuntimePropertyType,
                        null,
                        pi,
                        state.Options
                        );
                    par.NameAsString = pi.NameAsString;

                    parameterCache.Add(pi.NameAsString, par);
                }

                if (classInfo.ParameterCache == null)
                {
                    classInfo.ParameterCache = parameterCache;
                    classInfo.ParameterCount = parameterCache.Count;
                }
            }

            

            if(state.Current.CtorArgumentState == null)
            {
                state.Current.CtorArgumentState = new ArgumentState();
            }

            state.Current.CtorArgumentState!.Arguments =new  Dictionary<string,object>();
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            base.SetTypeMetadata(typeInfo, typeMap, options);
            typeInfo.Type = TypeEnum.TransitionTime;
            typeInfo.FullName = null;
        }
    }
}
