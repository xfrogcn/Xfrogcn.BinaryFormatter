using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Xfrogcn.BinaryFormatter.Serialization;

namespace Xfrogcn.BinaryFormatter
{
    internal sealed class BinaryPropertyInfo<T> : BinaryPropertyInfo
    {
        private bool _converterIsExternalAndPolymorphic;

        public Func<object, T> Get { get; private set; }
        public Action<object, T> Set { get; private set; }

        public BinaryConverter<T> Converter { get; internal set; } = null!;

      
        public override void Initialize(
            TypeMap typeMap,
           Type parentClassType,
           Type declaredPropertyType,
           Type runtimePropertyType,
           ClassType runtimeClassType,
           MemberInfo memberInfo,
           BinaryConverter converter,
           BinaryIgnoreCondition? ignoreCondition,
           BinarySerializerOptions options)
        {
            base.Initialize(
                typeMap,
                parentClassType,
                declaredPropertyType,
                runtimePropertyType,
                runtimeClassType,
                memberInfo,
                converter,
                ignoreCondition,
                options);

            switch (memberInfo)
            {
                case PropertyInfo propertyInfo:
                    {
                        bool useNonPublicAccessors = GetAttribute<BinaryIncludeAttribute>(propertyInfo) != null;

                        MethodInfo getMethod = propertyInfo.GetMethod;
                        if (getMethod != null && (getMethod.IsPublic || useNonPublicAccessors))
                        {
                            HasGetter = true;
                            Get = options.MemberAccessorStrategy.CreatePropertyGetter<T>(propertyInfo);
                        }

                        MethodInfo setMethod = propertyInfo.SetMethod;
                        if (setMethod != null && (setMethod.IsPublic || useNonPublicAccessors))
                        {
                            HasSetter = true;
                            Set = options.MemberAccessorStrategy.CreatePropertySetter<T>(propertyInfo);
                        }

                        break;
                    }

                case FieldInfo fieldInfo:
                    {
                        Debug.Assert(fieldInfo.IsPublic);

                        HasGetter = true;
                        Get = options.MemberAccessorStrategy.CreateFieldGetter<T>(fieldInfo);

                        if (!fieldInfo.IsInitOnly)
                        {
                            HasSetter = true;
                            Set = options.MemberAccessorStrategy.CreateFieldSetter<T>(fieldInfo);
                        }

                        break;
                    }

                default:
                    {
                        IsForClassInfo = true;
                        HasGetter = true;
                        HasSetter = true;

                        break;
                    }
            }

            _converterIsExternalAndPolymorphic = !converter.IsInternalConverter && DeclaredPropertyType != converter.TypeToConvert;
            GetPolicies(ignoreCondition, defaultValueIsNull: Converter.CanBeNull);
        }

        public override BinaryConverter ConverterBase
        {
            get
            {
                return Converter;
            }
            set
            {
                Debug.Assert(value is BinaryConverter<T>);
                Converter = (BinaryConverter<T>)value;
            }
        }

        public override object GetValueAsObject(object obj)
        {
            if (IsForClassInfo)
            {
                return obj;
            }

            Debug.Assert(HasGetter);
            return Get!(obj);
        }

       


        public override bool ReadBinaryAndSetMember(object obj, ref ReadStack state, ref BinaryReader reader)
        {
            bool success;

            if (state.Current.PropertyState < StackFramePropertyState.TryReadTypeSeq)
            {
                if (!reader.ReadTypeSeq())
                {
                    return false;
                }
                state.Current.PropertyState = StackFramePropertyState.TryReadTypeSeq;
                //if (reader.CurrentTypeInfo != null && Converter.ClassType != ClassType.Value)
                //{
                //    state.Current.BinaryTypeInfo = reader.CurrentTypeInfo;
                //}
                if (state.Current.PropertyPolymorphicConverter == null && reader.CurrentTypeInfo!=null  && Converter.CanBePolymorphic )
                {
                    var type = state.TypeMap.GetType(reader.CurrentTypeInfo.Seq);
                    if (type != Converter.TypeToConvert)
                    {
                        state.Current.PropertyPolymorphicConverter = state.Current.InitializeReEntry(type, Options, NameAsString);
                    }
                }
            }

            T value = default;
            if (state.Current.PropertyPolymorphicConverter != null)
            {
                success = state.Current.PropertyPolymorphicConverter.TryReadAsObject(ref reader, Options, ref state, out object tmpValue);
                if (success)
                {
                    value = (T)tmpValue;
                }

                //if (reader.CurrentTypeInfo.Seq != state.Current.BinaryTypeInfo.Seq)
                //{

                //}
                //var type = state.Current.BinaryPropertyInfo.ConverterBase.TypeToConvert;
                //if (type != Converter.TypeToConvert)
                //{
                //    BinaryConverter binaryConverter = state.Current.InitializeReEntry(type, Options, NameAsString);
                //    success = binaryConverter.TryReadAsObject(ref reader, Options, ref state, out object tmpValue);
                //    if (success)
                //    {
                //        value = (T)tmpValue;
                //    }
                //}
                //else
                //{
                //    success = Converter.TryRead(ref reader, Converter.TypeToConvert, state.Options, ref state, out value);

                //}
            }
            else
            {
                success = Converter.TryRead(ref reader, Converter.TypeToConvert, state.Options, ref state, out value);
            }

            if (!success)
            {
                return false;
            }

            state.Current.PropertyState = StackFramePropertyState.TryRead;
            Set(obj, value);

//            bool isNullToken = reader.TokenType == BinaryTokenType.Null;
//            if (isNullToken && !Converter.HandleNullOnRead && !state.IsContinuation)
//            {
//                if (!PropertyTypeCanBeNull)
//                {
//                    ThrowHelper.ThrowJsonException_DeserializeUnableToConvertValue(Converter.TypeToConvert);
//                }

//                Debug.Assert(default(T) == null);

//                if (!IgnoreDefaultValuesOnRead)
//                {
//                    T? value = default;
//                    Set!(obj, value!);
//                }

//                success = true;
//            }
//            else if (Converter.CanUseDirectReadOrWrite)
//            {
//                // CanUseDirectReadOrWrite == false when using streams
//                Debug.Assert(!state.IsContinuation);

//                if (!isNullToken || !IgnoreDefaultValuesOnRead || !PropertyTypeCanBeNull)
//                {
//                    // Optimize for internal converters by avoiding the extra call to TryRead.
//                    T? fastValue = Converter.Read(ref reader, RuntimePropertyType!, Options);
//                    Set!(obj, fastValue!);
//                }

//                success = true;
//            }
//            else
//            {
//                success = true;
//                if (!isNullToken || !IgnoreDefaultValuesOnRead || !PropertyTypeCanBeNull || state.IsContinuation)
//                {
//                    success = Converter.TryRead(ref reader, RuntimePropertyType!, Options, ref state, out T? value);
//                    if (success)
//                    {
//#if !DEBUG
//                        if (_converterIsExternalAndPolymorphic)
//#endif
//                        {
//                            if (value != null)
//                            {
//                                Type typeOfValue = value.GetType();
//                                if (!DeclaredPropertyType.IsAssignableFrom(typeOfValue))
//                                {
//                                    ThrowHelper.ThrowInvalidCastException_DeserializeUnableToAssignValue(typeOfValue, DeclaredPropertyType);
//                                }
//                            }
//                            else if (!PropertyTypeCanBeNull)
//                            {
//                                ThrowHelper.ThrowInvalidOperationException_DeserializeUnableToAssignNull(DeclaredPropertyType);
//                            }
//                        }

//                        Set!(obj, value!);
//                    }
//                }
//            }

            return success;
        }

        public override bool GetMemberAndWriteBinary(object obj, ref WriteStack state, BinaryWriter writer)
        {
            T value = Get!(obj);

            // Since devirtualization only works in non-shared generics,
            // the default comparer is used only for value types for now.
            // For reference types there is a quick check for null.
            if (IgnoreDefaultValuesOnWrite && (
                default(T) == null ? value == null : EqualityComparer<T>.Default.Equals(default, value)))
            {
                return true;
            }


            if (value == null)
            {
                Debug.Assert(Converter.CanBeNull);

                if (Converter.HandleNullOnWrite)
                {
                    // No object, collection, or re-entrancy converter handles null.
                    Debug.Assert(Converter.ClassType == ClassType.Value);

                    if (state.Current.PropertyState < StackFramePropertyState.Name)
                    {
                        state.Current.PropertyState = StackFramePropertyState.Name;
                        writer.WritePropertySeq(Seq);
                    }

                    int originalDepth = writer.CurrentDepth;
                    Converter.Write(writer, value, Options);
                    if (originalDepth != writer.CurrentDepth)
                    {
                        ThrowHelper.ThrowBinaryException_SerializationConverterWrite(Converter);
                    }
                }
                else
                {
                    writer.WritePropertySeq(Seq);
                    writer.WriteNullValue();
                }

                return true;
            }
            else
            {
                if (state.Current.PropertyState < StackFramePropertyState.Name)
                {
                    state.Current.PropertyState = StackFramePropertyState.Name;
                    writer.WritePropertySeq(Seq);
                }

                return Converter.TryWrite(writer, value, Options, ref state);
            }
        }

        public override bool GetMemberAndWriteBinaryExtensionData(object obj, ref WriteStack state, BinaryWriter writer)
        {
            //bool success;
            //T value = Get!(obj);

            //if (value == null)
            //{
            //    success = true;
            //}
            //else
            //{
            //    success = Converter.TryWriteDataExtensionProperty(writer, value, Options, ref state);
            //}

            //return success;
            throw new NotImplementedException();
        }

        public override bool ReadBinaryAsObject(ref ReadStack state, ref BinaryReader reader, out object value)
        {
            bool success;
            value = default;
            if (state.Current.PropertyState < StackFramePropertyState.TryReadTypeSeq)
            {
                if (!reader.ReadTypeSeq())
                {
                    return false;
                }
                state.Current.PropertyState = StackFramePropertyState.TryReadTypeSeq;

                bool isNullToken = reader.TokenType == BinaryTokenType.Null;
                if (isNullToken && !Converter.HandleNullOnRead && !state.IsContinuation)
                {
                    if (!Converter.CanBeNull)
                    {
                        ThrowHelper.ThrowBinaryException_DeserializeUnableToConvertValue(Converter.TypeToConvert);
                    }
                    state.Current.PropertyState = StackFramePropertyState.TryRead;
                    value = default(T);
                    success = true;
                }


                if (state.Current.PropertyPolymorphicConverter == null && reader.CurrentTypeInfo != null && Converter.CanBePolymorphic)
                {
                    var type = state.TypeMap.GetType(reader.CurrentTypeInfo.Seq);
                    if (type != Converter.TypeToConvert)
                    {
                        state.Current.PropertyPolymorphicConverter = state.Current.InitializeReEntry(type, Options, NameAsString);
                    }
                }
            }

            
            if (state.Current.PropertyPolymorphicConverter != null)
            {
                success = state.Current.PropertyPolymorphicConverter.TryReadAsObject(ref reader, Options, ref state, out object tmpValue);
                value = (T)tmpValue;
            }
            else
            {
                success = Converter.TryRead(ref reader, Converter.TypeToConvert, state.Options, ref state, out T typedValue);
                value = typedValue;
            }

            if (!success)
            {
                return false;
            }

            state.Current.PropertyState = StackFramePropertyState.TryRead;


            return success;
        }


        public override void SetExtensionDictionaryAsObject(object obj, object extensionDict)
        {
            Debug.Assert(HasSetter);
            T typedValue = (T)extensionDict!;
            Set!(obj, typedValue);
        }
    }
}
