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
           Type parentClassType,
           Type declaredPropertyType,
           Type? runtimePropertyType,
           ClassType runtimeClassType,
           MemberInfo? memberInfo,
           BinaryConverter converter,
           BinaryIgnoreCondition? ignoreCondition,
           BinarySerializerOptions options)
        {
            base.Initialize(
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

                        MethodInfo? getMethod = propertyInfo.GetMethod;
                        if (getMethod != null && (getMethod.IsPublic || useNonPublicAccessors))
                        {
                            HasGetter = true;
                            Get = options.MemberAccessorStrategy.CreatePropertyGetter<T>(propertyInfo);
                        }

                        MethodInfo? setMethod = propertyInfo.SetMethod;
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

        public override bool GetMemberAndWriteBinary(object obj, ref WriteStack state, BinaryWriter writer)
        {
            //T value = Get!(obj);

            //// Since devirtualization only works in non-shared generics,
            //// the default comparer is used only for value types for now.
            //// For reference types there is a quick check for null.
            //if (IgnoreDefaultValuesOnWrite && (
            //    default(T) == null ? value == null : EqualityComparer<T>.Default.Equals(default, value)))
            //{
            //    return true;
            //}

            //if (value == null)
            //{
            //    Debug.Assert(Converter.CanBeNull);

            //    if (Converter.HandleNullOnWrite)
            //    {
            //        // No object, collection, or re-entrancy converter handles null.
            //        Debug.Assert(Converter.ClassType == ClassType.Value);

            //        if (state.Current.PropertyState < StackFramePropertyState.Name)
            //        {
            //            state.Current.PropertyState = StackFramePropertyState.Name;
            //            writer.WritePropertyNameSection(EscapedNameSection);
            //        }

            //        int originalDepth = writer.CurrentDepth;
            //        Converter.Write(writer, value, Options);
            //        if (originalDepth != writer.CurrentDepth)
            //        {
            //            ThrowHelper.ThrowJsonException_SerializationConverterWrite(Converter);
            //        }
            //    }
            //    else
            //    {
            //        writer.WriteNullSection(EscapedNameSection);
            //    }

            //    return true;
            //}
            //else
            //{
            //    if (state.Current.PropertyState < StackFramePropertyState.Name)
            //    {
            //        state.Current.PropertyState = StackFramePropertyState.Name;
            //        writer.WritePropertyNameSection(EscapedNameSection);
            //    }

            //    return Converter.TryWrite(writer, value, Options, ref state);
            //}
            throw new NotImplementedException();
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

        

        public override void SetExtensionDictionaryAsObject(object obj, object extensionDict)
        {
            Debug.Assert(HasSetter);
            T typedValue = (T)extensionDict!;
            Set!(obj, typedValue);
        }
    }
}
