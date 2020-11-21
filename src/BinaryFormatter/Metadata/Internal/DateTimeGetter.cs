using System;
using System.Collections.Generic;

namespace Xfrogcn.BinaryFormatter.Metadata.Internal
{
    public class DateTimeGetter : IMetadataGetter
    {
        static readonly Dictionary<Type, TypeEnum> _timeTypeMap = new Dictionary<Type, TypeEnum>()
        {
            { typeof(DateTime),  TypeEnum.DateTime },
            { typeof(DateTimeOffset), TypeEnum.DateTimeOffset },
            { typeof(TimeSpan), TypeEnum.TimeSpan },
            { typeof(TimeZoneInfo), TypeEnum.TimeZoneInfo },
            { typeof(TimeZoneInfo.AdjustmentRule), TypeEnum.AdjustmentRule },
            { typeof(TimeZoneInfo.TransitionTime), TypeEnum.TransitionTime },
        };

        public bool CanProcess(Type type)
        {
            return _timeTypeMap.ContainsKey(type);
        }

        public bool GetTypeInfo(Type type, BinaryTypeInfo typeInfo, MetadataGetterContext context)
        {
            TypeEnum te = _timeTypeMap[type];
            typeInfo.Type = te;
            typeInfo.IsGeneric = false;
            typeInfo.GenericArgumentCount = 0;
            typeInfo.SerializeType = SerializeTypeEnum.SingleValue;
            typeInfo.Members = new BinaryMemberInfo[0];

            if (type == typeof(TimeZoneInfo))
            {
                typeInfo.SerializeType = SerializeTypeEnum.KeyValuePair;
                typeInfo.Members = new BinaryMemberInfo[]
                {
                    new BinaryMemberInfo(){ Seq = 0, IsField =false, Name = nameof(TimeZoneInfo.BaseUtcOffset), TypeSeq = context.GetTypeSeq(typeof(TimeSpan), context) },
                    new BinaryMemberInfo(){ Seq = 1, IsField =false, Name = nameof(TimeZoneInfo.DisplayName), TypeSeq = context.GetTypeSeq(typeof(string), context) },
                    new BinaryMemberInfo(){ Seq = 2, IsField =false, Name = nameof(TimeZoneInfo.StandardName), TypeSeq = context.GetTypeSeq(typeof(string), context) },
                    new BinaryMemberInfo(){ Seq = 3, IsField =false, Name = nameof(TimeZoneInfo.DaylightName), TypeSeq = context.GetTypeSeq(typeof(string), context) },
                    new BinaryMemberInfo(){ Seq = 4, IsField =false, Name = "AdjustmentRules", TypeSeq = context.GetTypeSeq(typeof(TimeZoneInfo.AdjustmentRule[]), context) },
                };
            }
            if(type == typeof(TimeZoneInfo.AdjustmentRule))
            {
                typeInfo.SerializeType = SerializeTypeEnum.KeyValuePair;
                typeInfo.Members = new BinaryMemberInfo[]
                {
                    new BinaryMemberInfo(){ Seq = 0, IsField =false, Name = nameof(TimeZoneInfo.AdjustmentRule.DateStart), TypeSeq = context.GetTypeSeq(typeof(DateTime), context) },
                    new BinaryMemberInfo(){ Seq = 1, IsField =false, Name = nameof(TimeZoneInfo.AdjustmentRule.DateEnd), TypeSeq = context.GetTypeSeq(typeof(DateTime), context) },
                    new BinaryMemberInfo(){ Seq = 2, IsField =false, Name = nameof(TimeZoneInfo.AdjustmentRule.DaylightDelta), TypeSeq = context.GetTypeSeq(typeof(TimeSpan), context) },
                    new BinaryMemberInfo(){ Seq = 3, IsField =false, Name = nameof(TimeZoneInfo.AdjustmentRule.DaylightTransitionStart), TypeSeq = context.GetTypeSeq(typeof(TimeZoneInfo.TransitionTime), context) },
                    new BinaryMemberInfo(){ Seq = 4, IsField =false, Name = nameof(TimeZoneInfo.AdjustmentRule.DaylightTransitionEnd), TypeSeq = context.GetTypeSeq(typeof(TimeZoneInfo.TransitionTime), context) },
                };
            }

            if (type == typeof(TimeZoneInfo.TransitionTime))
            {
                typeInfo.SerializeType = SerializeTypeEnum.KeyValuePair;
                typeInfo.Members = new BinaryMemberInfo[]
                {
                    new BinaryMemberInfo(){ Seq = 0, IsField =false, Name = nameof(TimeZoneInfo.TransitionTime.IsFixedDateRule), TypeSeq = context.GetTypeSeq(typeof(bool), context) },
                    new BinaryMemberInfo(){ Seq = 1, IsField =false, Name = nameof(TimeZoneInfo.TransitionTime.Day), TypeSeq = context.GetTypeSeq(typeof(int), context) },
                    new BinaryMemberInfo(){ Seq = 2, IsField =false, Name = nameof(TimeZoneInfo.TransitionTime.DayOfWeek), TypeSeq = context.GetTypeSeq(typeof(DayOfWeek), context) },
                    new BinaryMemberInfo(){ Seq = 3, IsField =false, Name = nameof(TimeZoneInfo.TransitionTime.Month), TypeSeq = context.GetTypeSeq(typeof(int), context) },
                    new BinaryMemberInfo(){ Seq = 4, IsField =false, Name = nameof(TimeZoneInfo.TransitionTime.TimeOfDay), TypeSeq = context.GetTypeSeq(typeof(DateTime), context) },
                    new BinaryMemberInfo(){ Seq = 5, IsField =false, Name = nameof(TimeZoneInfo.TransitionTime.Week), TypeSeq = context.GetTypeSeq(typeof(int), context) },
                };
            }

            return true;
        }
    }
}
