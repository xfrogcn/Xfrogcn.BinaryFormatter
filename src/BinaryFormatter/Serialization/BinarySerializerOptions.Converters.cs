using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Xfrogcn.BinaryFormatter.Serialization;
using Xfrogcn.BinaryFormatter.Serialization.Converters;

namespace Xfrogcn.BinaryFormatter
{
    public sealed partial class BinarySerializerOptions
    {
        // 转换器缓存
        private readonly ConcurrentDictionary<Type, BinaryConverter> _converters = new ConcurrentDictionary<Type, BinaryConverter>();
        private static readonly Dictionary<Type, BinaryConverter> s_defaultSimpleConverters = GetDefaultSimpleConverters();

        //private ConcurrentDictionary<Type, BinaryConverter> _dictionaryKeyConverters;

        // The global list of built-in converters that override CanConvert().
        private static readonly BinaryConverter[] s_defaultFactoryConverters = new BinaryConverter[]
        {
            // Nullable converter should always be first since it forwards to any nullable type.
            new NullableConverterFactory(),
            new EnumConverterFactory(),
            new VectorOfTConverterFactory(),
            //// IEnumerable should always be second to last since they can convert any IEnumerable.
            new IEnumerableConverterFactory(),
            //// Object should always be last since it converts any type.
            new ObjectConverterFactory()
        };

        internal bool IgnoreCtorParameterCountThreshold = false;

        public IList<BinaryConverter> Converters { get; }

        private static Dictionary<Type, BinaryConverter> GetDefaultSimpleConverters()
        {
            const int NumberOfSimpleConverters = 34;
            var converters = new Dictionary<Type, BinaryConverter>(NumberOfSimpleConverters);

            // Use a dictionary for simple converters.
            // When adding to this, update NumberOfSimpleConverters above.
            Add(new BooleanConverter());
            Add(new ByteConverter());
            Add(new ByteArrayConverter());
            Add(new CharConverter());
            Add(new DateTimeConverter());
            Add(new DateTimeOffsetConverter());
            Add(new TimeSpanConverter());
            Add(new DoubleConverter());
            Add(new DecimalConverter());
            Add(new BigIntegerConverter());
            Add(new ComplexConverter());
            Add(new GuidConverter());
            Add(new Int16Converter());
            Add(new Int32Converter());
            Add(new Int64Converter());
            Add(new SingleConverter());
            //Add(new ObjectConverter());
            Add(new SByteConverter());
            Add(new StringConverter());
            //Add(new TypeConverter());
            Add(new UInt16Converter());
            Add(new UInt32Converter());
            Add(new UInt64Converter());
            Add(new DBNullConverter());
            Add(new UriConverter());
            Add(new Vector2Converter());
            Add(new Vector3Converter());
            Add(new Vector4Converter());
            Add(new Matrix3x2Converter());
            Add(new Matrix4x4Converter());
            Add(new PlaneConverter());
            Add(new QuaternionConverter());
            Add(new TransitionTimeConverter());
            Add(new AdjustmentRuleConverter());
            Add(new TimeZoneInfoConverter());
            Add(new NameValueCollectionConverter());

            Debug.Assert(NumberOfSimpleConverters == converters.Count);

            return converters;

            void Add(BinaryConverter converter) =>
                converters.Add(converter.TypeToConvert, converter);
        }


        internal BinaryConverter DetermineConverter(Type parentClassType, Type runtimePropertyType, MemberInfo memberInfo)
        {
            BinaryConverter converter = null!;

            // Priority 1: attempt to get converter from BinaryConverterAttribute on property.
            if (memberInfo != null)
            {
                Debug.Assert(parentClassType != null);

                BinaryConverterAttribute converterAttribute = (BinaryConverterAttribute)
                    GetAttributeThatCanHaveMultiple(parentClassType!, typeof(BinaryConverterAttribute), memberInfo);

                if (converterAttribute != null)
                {
                    converter = GetConverterFromAttribute(converterAttribute, typeToConvert: runtimePropertyType, classTypeAttributeIsOn: parentClassType!, memberInfo);
                }
            }

            if (converter == null)
            {
                converter = GetConverter(runtimePropertyType);
                Debug.Assert(converter != null);
            }

            if (converter is BinaryConverterFactory factory)
            {
                converter = factory.GetConverterInternal(runtimePropertyType, this);

                // A factory cannot return null; GetConverterInternal checked for that.
                Debug.Assert(converter != null);
            }

            // 非空类型转换器处理可空类型，引发异常
            if (runtimePropertyType.IsNullableType() && !converter.TypeToConvert.IsNullableType())
            {
                ThrowHelper.ThrowInvalidOperationException_ConverterCanConvertNullableRedundant(runtimePropertyType, converter);
            }

            return converter;
        }

        /// <summary>
        /// 获取类型转换器
        /// </summary>
        /// <param name="typeToConvert"></param>
        /// <returns></returns>
        public BinaryConverter GetConverter(Type typeToConvert)
        {
            // 从缓存获取
            if (_converters.TryGetValue(typeToConvert, out BinaryConverter converter))
            {
                Debug.Assert(converter != null);
                return converter;
            }

            // Priority 2: Attempt to get custom converter added at runtime.
            // Currently there is not a way at runtime to overide the [BinaryConverter] when applied to a property.
            foreach (BinaryConverter item in Converters)
            {
                if (item.CanConvert(typeToConvert))
                {
                    converter = item;
                    break;
                }
            }

            // Priority 3: Attempt to get converter from [BinaryConverter] on the type being converted.
            if (converter == null)
            {
                BinaryConverterAttribute converterAttribute = (BinaryConverterAttribute)
                    GetAttributeThatCanHaveMultiple(typeToConvert, typeof(BinaryConverterAttribute));

                if (converterAttribute != null)
                {
                    converter = GetConverterFromAttribute(converterAttribute, typeToConvert: typeToConvert, classTypeAttributeIsOn: typeToConvert, memberInfo: null);
                }
            }

            // Priority 4: Attempt to get built-in converter.
            if (converter == null)
            {
                if (s_defaultSimpleConverters.TryGetValue(typeToConvert, out BinaryConverter foundConverter))
                {
                    Debug.Assert(foundConverter != null);
                    converter = foundConverter;
                }
                else
                {
                    foreach (BinaryConverter item in s_defaultFactoryConverters)
                    {
                        if (item.CanConvert(typeToConvert))
                        {
                            converter = item;
                            break;
                        }
                    }

                    // Since the object and IEnumerable converters cover all types, we should have a converter.
                    Debug.Assert(converter != null);
                }
            }

            // Allow redirection for generic types or the enum converter.
            if (converter is BinaryConverterFactory factory)
            {
                converter = factory.GetConverterInternal(typeToConvert, this);

                // A factory cannot return null; GetConverterInternal checked for that.
                Debug.Assert(converter != null);
            }

            Type converterTypeToConvert = converter.TypeToConvert;

            if (!converterTypeToConvert.IsAssignableFromInternal(typeToConvert)
                && !typeToConvert.IsAssignableFromInternal(converterTypeToConvert))
            {
                ThrowHelper.ThrowInvalidOperationException_SerializationConverterNotCompatible(converter.GetType(), typeToConvert);
            }

            // Only cache the value once (de)serialization has occurred since new converters can be added that may change the result.
            if (_haveTypesBeenCreated)
            {
                // A null converter is allowed here and cached.

                // Ignore failure case here in multi-threaded cases since the cached item will be equivalent.
                _converters.TryAdd(typeToConvert, converter);
            }

            return converter;
        }


        /// <summary>
        /// 从特性中获取转换器
        /// </summary>
        /// <param name="converterAttribute">特性</param>
        /// <param name="typeToConvert">待转换类型</param>
        /// <param name="classTypeAttributeIsOn">所属类型</param>
        /// <param name="memberInfo">成员信息</param>
        /// <returns></returns>
        private BinaryConverter GetConverterFromAttribute(BinaryConverterAttribute converterAttribute, Type typeToConvert, Type classTypeAttributeIsOn, MemberInfo memberInfo)
        {
            BinaryConverter converter;

            // 如果转换器特性的ConverterType为空，则尝试使用特性的CreateConverter方法来创建
            Type type = converterAttribute.ConverterType;
            if (type == null)
            {
                // Allow the attribute to create the converter.
                converter = converterAttribute.CreateConverter(typeToConvert);
                if (converter == null)
                {
                    ThrowHelper.ThrowInvalidOperationException_SerializationConverterOnAttributeNotCompatible(classTypeAttributeIsOn, memberInfo, typeToConvert);
                }
            }
            else
            {
                ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes);
                // ConvertType必须是BinaryConverter的子类，并且具有无参数公共构造
                if (!typeof(BinaryConverter).IsAssignableFrom(type) || ctor == null || !ctor.IsPublic)
                {
                    ThrowHelper.ThrowInvalidOperationException_SerializationConverterOnAttributeInvalid(classTypeAttributeIsOn, memberInfo);
                }

                converter = (BinaryConverter)Activator.CreateInstance(type)!;
            }

            Debug.Assert(converter != null);
            if (!converter.CanConvert(typeToConvert))
            {
                Type underlyingType = Nullable.GetUnderlyingType(typeToConvert);
                if (underlyingType != null && converter.CanConvert(underlyingType))
                {
                    if (converter is BinaryConverterFactory converterFactory)
                    {
                        converter = converterFactory.GetConverterInternal(underlyingType, this);
                    }

                    // Allow nullable handling to forward to the underlying type's converter.
                    return NullableConverterFactory.CreateValueConverter(underlyingType, converter);
                }

                ThrowHelper.ThrowInvalidOperationException_SerializationConverterOnAttributeNotCompatible(classTypeAttributeIsOn, memberInfo, typeToConvert);
            }

            return converter;
        }


        /// <summary>
        /// 多个特性
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="attributeType"></param>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        private static Attribute GetAttributeThatCanHaveMultiple(Type classType, Type attributeType, MemberInfo memberInfo)
        {
            object[] attributes = memberInfo.GetCustomAttributes(attributeType, inherit: false);
            return GetAttributeThatCanHaveMultiple(attributeType, classType, memberInfo, attributes);
        }

        private static Attribute GetAttributeThatCanHaveMultiple(Type attributeType, Type classType, MemberInfo memberInfo, object[] attributes)
        {
            if (attributes.Length == 0)
            {
                return null;
            }

            if (attributes.Length == 1)
            {
                return (Attribute)attributes[0];
            }

            ThrowHelper.ThrowInvalidOperationException_SerializationDuplicateAttribute(attributeType, classType, memberInfo);
            return default;
        }

        internal static Attribute GetAttributeThatCanHaveMultiple(Type classType, Type attributeType)
        {
            object[] attributes = classType.GetCustomAttributes(attributeType, inherit: false);
            return GetAttributeThatCanHaveMultiple(attributeType, classType, null, attributes);
        }

    }
}
