using System;
using System.Collections.Generic;
using System.Reflection;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class TimeZoneInfoConverter : LargeObjectWithParameterizedConstructorConverter<TimeZoneInfo>
    {
        internal override bool ConstructorIsParameterized => false;
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(TimeZoneInfo);
        }

        public override MethodInfo[] GetAdditionalDataMethod()
        {
            var mi = TypeToConvert.GetMethod(nameof(TimeZoneInfo.GetAdjustmentRules));
            return new MethodInfo[] { mi };
        }

        protected override void SetCtorArguments(ref ReadStack state, BinaryParameterInfo binaryParameterInfo, ref object arg)
        {
            state.Current.PropertyValueCache[binaryParameterInfo.NameAsString] = arg!;
        }

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



            if (state.Current.CtorArgumentState == null)
            {
                state.Current.CtorArgumentState = new ArgumentState();
            }

            state.Current.PropertyValueCache = new Dictionary<string, object>();
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            base.SetTypeMetadata(typeInfo, typeMap, options);
            typeInfo.Type = TypeEnum.TimeZoneInfo;
            typeInfo.FullName = null;
        }
    }
}
