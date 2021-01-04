using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    /// <summary>
    /// 当有参数构造的构造参数数量大于UnboxedParameterCountThreshold设定值时(默认为4)，使用此转换器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class LargeObjectWithParameterizedConstructorConverter<T> : ObjectWithParameterizedConstructorConverter<T> where T : notnull
    {
        protected override bool ReadAndCacheConstructorArgument(ref ReadStack state, ref BinaryReader reader, BinaryParameterInfo binaryParameterInfo)
        {
            Debug.Assert(binaryParameterInfo.ShouldDeserialize);
            Debug.Assert(binaryParameterInfo.Options != null);

            var converter = binaryParameterInfo.ConverterBase;

            bool success;


            if (state.Current.PropertyState < StackFramePropertyState.TryReadTypeSeq)
            {
                if (!reader.ReadTypeSeq())
                {
                    return false;
                }
                state.Current.PropertyState = StackFramePropertyState.TryReadTypeSeq;
                if (state.Current.PropertyPolymorphicConverter == null && reader.CurrentTypeInfo != null && converter.CanBePolymorphic)
                {
                    var type = state.TypeMap.GetType(reader.CurrentTypeInfo.Seq);
                    if (type != null && type != converter.TypeToConvert && converter.TypeToConvert.IsAssignableFrom(type))
                    {
                        state.Current.PropertyPolymorphicConverter = state.Current.InitializeReEntry(type, state.Options);
                    }
                }
            }

            object arg;
            if (state.Current.PropertyPolymorphicConverter != null)
            {
                success = state.Current.PropertyPolymorphicConverter.TryReadAsObject(ref reader, state.Options, ref state, out arg);

            }
            else
            {
                success = converter.TryReadAsObject(ref reader, binaryParameterInfo.Options!, ref state, out arg);
            }

            if (success && !(arg == null && binaryParameterInfo.IgnoreDefaultValuesOnRead))
            {
                ((object[])state.Current.CtorArgumentState!.Arguments)[binaryParameterInfo.Position] = arg!;
            }

            return success;
        }


        protected bool TryRead<TArg>(
            ref ReadStack state,
            ref BinaryReader reader,
            BinaryParameterInfo binaryParameterInfo,
            out TArg arg)
        {
            Debug.Assert(binaryParameterInfo.ShouldDeserialize);
            Debug.Assert(binaryParameterInfo.Options != null);

            var info = (BinaryParameterInfo<TArg>)binaryParameterInfo;

            var converter = (BinaryConverter<TArg>)binaryParameterInfo.ConverterBase;

            bool success;

            if (state.Current.PropertyState < StackFramePropertyState.TryReadTypeSeq)
            {
                if (!reader.ReadTypeSeq())
                {
                    arg = default;
                    return false;
                }
                state.Current.PropertyState = StackFramePropertyState.TryReadTypeSeq;
                if (state.Current.PropertyPolymorphicConverter == null && reader.CurrentTypeInfo != null && converter.CanBePolymorphic)
                {
                    var type = state.TypeMap.GetType(reader.CurrentTypeInfo.Seq);
                    if (type != null && type != converter.TypeToConvert && converter.TypeToConvert.IsAssignableFrom(type))
                    {
                        state.Current.PropertyPolymorphicConverter = state.Current.InitializeReEntry(type, state.Options);
                    }
                }
            }

            TArg value = default;
            if (state.Current.PropertyPolymorphicConverter != null)
            {
                success = state.Current.PropertyPolymorphicConverter.TryReadAsObject(ref reader, state.Options, ref state, out object tmpValue);
                if (success)
                {
                    value = (TArg)tmpValue;
                }
            }
            else
            {
                success = converter.TryRead(ref reader, info.RuntimePropertyType, info.Options!, ref state, out _, out value);
            }

            arg = value == null && binaryParameterInfo.IgnoreDefaultValuesOnRead
                ? (TArg)info.DefaultValue! // Use default value specified on parameter, if any.
                : value!;

            return success;
        }


        protected override object CreateObject(ref ReadStackFrame frame)
        {
            object[] arguments = (object[])frame.CtorArgumentState!.Arguments;

            var createObject = (BinaryClassInfo.ParameterizedConstructorDelegate<T>)frame.BinaryClassInfo.CreateObjectWithArgs;

            if (createObject == null)
            {
                // This means this constructor has more than 64 parameters.
                ThrowHelper.ThrowNotSupportedException_ConstructorMaxOf64Parameters(ConstructorInfo!, TypeToConvert);
            }

            object obj = createObject(arguments);

            ArrayPool<object>.Shared.Return(arguments, clearArray: true);
            return obj;
        }

        protected override void InitializeConstructorArgumentCaches(ref ReadStack state, BinarySerializerOptions options)
        {
            BinaryClassInfo classInfo = state.Current.BinaryClassInfo;

            if (classInfo.CreateObjectWithArgs == null)
            {
                classInfo.CreateObjectWithArgs = options.MemberAccessorStrategy.CreateParameterizedConstructor<T>(ConstructorInfo!);
            }

            object[] arguments = ArrayPool<object>.Shared.Rent(classInfo.ParameterCount);
            foreach (BinaryParameterInfo binaryParameterInfo in classInfo.ParameterCache!.Values)
            {
                if (binaryParameterInfo.ShouldDeserialize)
                {
                    arguments[binaryParameterInfo.Position] = binaryParameterInfo.DefaultValue!;
                }
            }

            state.Current.CtorArgumentState!.Arguments = arguments;
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            base.SetTypeMetadata(typeInfo, typeMap, options);
            Type t = typeof(T);
            if (t.IsGenericType)
            {
                Type gt = t.GetGenericTypeDefinition();
                if (gt == typeof(Tuple<,,,,>) ||
                    gt == typeof(Tuple<,,,,,>) ||
                    gt == typeof(Tuple<,,,,,,>) ||
                    gt == typeof(Tuple<,,,,,,,>))
                {
                    typeInfo.Type = TypeEnum.Tuple;
                    typeInfo.FullName = null;
                }
                else if (gt == typeof(ValueTuple<,,,,>) ||
                    gt == typeof(ValueTuple<,,,,,>) ||
                    gt == typeof(ValueTuple<,,,,,,>) ||
                    gt == typeof(ValueTuple<,,,,,,,>))
                {
                    typeInfo.Type = TypeEnum.ValueTuple;
                    typeInfo.FullName = null;
                }

            }
        }
    }
}
