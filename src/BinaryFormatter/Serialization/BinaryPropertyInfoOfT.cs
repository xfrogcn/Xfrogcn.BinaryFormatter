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
                case MethodInfo methodInfo:
                    {
                        HasGetter = true;
                        HasSetter = false;
                        Get = options.MemberAccessorStrategy.CreateMethodGetter<T>(methodInfo);
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
          
                if (state.Current.PropertyPolymorphicConverter == null && reader.CurrentTypeInfo!=null   )
                {
                    var type = state.TypeMap.GetType(reader.CurrentTypeInfo.Seq);
                    if (type !=null && type != Converter.TypeToConvert )
                    {
                        if (Converter.TypeToConvert.IsAssignableFrom(type))
                        {
                            state.Current.PropertyPolymorphicConverter = state.Current.InitializeReEntry(type, Options);
                        }
                        else if( type.IsValueType || type == typeof(string))
                        {
                            if (!reader.TrySkip(state.Options))
                            {
                                return false;
                            }

                            return true;
                        }

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
            }
            else
            {
                success = Converter.TryRead(ref reader, Converter.TypeToConvert, state.Options, ref state, out _, out value);
            }

            if (!success)
            {
                return false;
            }

            state.Current.PropertyState = StackFramePropertyState.TryRead;
            Set(obj, value);

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
                }


                if (state.Current.PropertyPolymorphicConverter == null && reader.CurrentTypeInfo != null && Converter.CanBePolymorphic)
                {
                    var type = state.TypeMap.GetType(reader.CurrentTypeInfo.Seq);
                    if (type != null && type != Converter.TypeToConvert && Converter.TypeToConvert.IsAssignableFrom(type))
                    {
                        state.Current.PropertyPolymorphicConverter = state.Current.InitializeReEntry(type, Options);
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
                success = Converter.TryRead(ref reader, Converter.TypeToConvert, state.Options, ref state, out ReferenceID refId, out T typedValue);
                value = (object)refId ?? typedValue;
            }

            if (!success)
            {
                return false;
            }

            state.Current.PropertyState = StackFramePropertyState.TryRead;


            return success;
        }


        public override void SetExtensionDictionaryAsObject(ref ReadStack state, object obj, object extensionDict)
        {
            Debug.Assert(HasSetter);
            state.ReferenceResolver.AddReferenceCallback(obj, extensionDict, (ins, prop) =>
             {
                 Set!(ins, (T)prop);
                 return true;
             });
           
            
        }
    }
}
