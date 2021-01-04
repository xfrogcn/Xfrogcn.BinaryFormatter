using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal class SmallObjectWithParameterizedConstructorConverter<T, TArg0, TArg1, TArg2, TArg3> : ObjectWithParameterizedConstructorConverter<T> where T : notnull
    {
        protected override object CreateObject(ref ReadStackFrame frame)
        {
            var createObject = (BinaryClassInfo.ParameterizedConstructorDelegate<T, TArg0, TArg1, TArg2, TArg3>)
                frame.BinaryClassInfo.CreateObjectWithArgs!;
            var arguments = (Arguments<TArg0, TArg1, TArg2, TArg3>)frame.CtorArgumentState!.Arguments;
            return createObject!(arguments.Arg0, arguments.Arg1, arguments.Arg2, arguments.Arg3);
        }

        protected override bool ReadAndCacheConstructorArgument(
            ref ReadStack state,
            ref BinaryReader reader,
            BinaryParameterInfo binaryParameterInfo)
        {
            Debug.Assert(state.Current.CtorArgumentState!.Arguments != null);
            var arguments = (Arguments<TArg0, TArg1, TArg2, TArg3>)state.Current.CtorArgumentState.Arguments;

            bool success;

            switch (binaryParameterInfo.Position)
            {
                case 0:
                    success = TryRead<TArg0>(ref state, ref reader, binaryParameterInfo, out arguments.Arg0);
                    break;
                case 1:
                    success = TryRead<TArg1>(ref state, ref reader, binaryParameterInfo, out arguments.Arg1);
                    break;
                case 2:
                    success = TryRead<TArg2>(ref state, ref reader, binaryParameterInfo, out arguments.Arg2);
                    break;
                case 3:
                    success = TryRead<TArg3>(ref state, ref reader, binaryParameterInfo, out arguments.Arg3);
                    break;
                default:
                    Debug.Fail("More than 4 params: we should be in override for LargeObjectWithParameterizedConstructorConverter.");
                    throw new InvalidOperationException();
            }

            return success;
        }

        private bool TryRead<TArg>(
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

        protected override void InitializeConstructorArgumentCaches(ref ReadStack state, BinarySerializerOptions options)
        {
            BinaryClassInfo classInfo = state.Current.BinaryClassInfo;

            if (classInfo.CreateObjectWithArgs == null)
            {
                classInfo.CreateObjectWithArgs =
                    options.MemberAccessorStrategy.CreateParameterizedConstructor<T, TArg0, TArg1, TArg2, TArg3>(ConstructorInfo!);
            }

            var arguments = new Arguments<TArg0, TArg1, TArg2, TArg3>();

            foreach (BinaryParameterInfo parameterInfo in classInfo.ParameterCache!.Values)
            {
                if (parameterInfo.ShouldDeserialize)
                {
                    int position = parameterInfo.Position;

                    switch (position)
                    {
                        case 0:
                            arguments.Arg0 = ((BinaryParameterInfo<TArg0>)parameterInfo).TypedDefaultValue!;
                            break;
                        case 1:
                            arguments.Arg1 = ((BinaryParameterInfo<TArg1>)parameterInfo).TypedDefaultValue!;
                            break;
                        case 2:
                            arguments.Arg2 = ((BinaryParameterInfo<TArg2>)parameterInfo).TypedDefaultValue!;
                            break;
                        case 3:
                            arguments.Arg3 = ((BinaryParameterInfo<TArg3>)parameterInfo).TypedDefaultValue!;
                            break;
                        default:
                            Debug.Fail("More than 4 params: we should be in override for LargeObjectWithParameterizedConstructorConverter.");
                            throw new InvalidOperationException();
                    }
                }
            }

            state.Current.CtorArgumentState!.Arguments = arguments;
        }

        //public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        //{
        //    base.SetTypeMetadata(typeInfo, typeMap, options);
        //    Type t = typeof(T);
        //    if (t.IsGenericType)
        //    {
        //        Type gt = t.GetGenericTypeDefinition();
        //        if (gt == typeof(Tuple<>) ||
        //            gt == typeof(Tuple<,>) ||
        //            gt == typeof(Tuple<,,>) ||
        //            gt == typeof(Tuple<,,,>))
        //        {
        //            typeInfo.Type = TypeEnum.Tuple;
        //            typeInfo.FullName = null;
        //        }
        //        else if (gt == typeof(ValueTuple<>) ||
        //            gt == typeof(ValueTuple<,>) ||
        //            gt == typeof(ValueTuple<,,>) ||
        //            gt == typeof(ValueTuple<,,,>))
        //        {
        //            typeInfo.Type = TypeEnum.ValueTuple;
        //            typeInfo.FullName = null;
        //        }

        //    }
            
        //}
    }
}
