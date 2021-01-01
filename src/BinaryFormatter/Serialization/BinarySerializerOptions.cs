using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Xfrogcn.BinaryFormatter.Resources;
using Xfrogcn.BinaryFormatter.Serialization;

[assembly: InternalsVisibleTo("Xfrogcn.BinaryFormatter.Tests")]

namespace Xfrogcn.BinaryFormatter
{
    public sealed partial class BinarySerializerOptions
    {
        internal const int BufferSizeDefault = 16 * 1024;
        private int _defaultBufferSize = BufferSizeDefault;
        internal const int DefaultMaxDepth = 64;
        internal static readonly BinarySerializerOptions s_defaultOptions = new BinarySerializerOptions();

        private readonly TypeMap _typeMap = new TypeMap();
        private readonly ConcurrentDictionary<Type, BinaryClassInfo> _classes = new ConcurrentDictionary<Type, BinaryClassInfo>();

        private MemberAccessor _memberAccessorStrategy;
        private BinaryIgnoreCondition _defaultIgnoreCondition;
        public TypeHandler TypeHandler { get; set; }

        private bool _haveTypesBeenCreated;

        private int _maxDepth;
        private ReferenceHandler _referenceHandler;

        private bool _ignoreNullValues;
        private bool _ignoreReadOnlyProperties;
        private bool _ignoreReadonlyFields;
        private bool _propertyNameCaseInsensitive;
        private bool _includeFields;

        internal TypeMap TypeMap => _typeMap;

        public BinarySerializerOptions()
        {
            Converters = new ConverterList(this);
        }

        public BinarySerializerOptions(BinarySerializerOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _memberAccessorStrategy = options._memberAccessorStrategy;
           
            _defaultIgnoreCondition = options._defaultIgnoreCondition;
           

            _defaultBufferSize = options._defaultBufferSize;
            
            _ignoreNullValues = options._ignoreNullValues;
            _ignoreReadOnlyProperties = options._ignoreReadOnlyProperties;
            _ignoreReadonlyFields = options._ignoreReadonlyFields;
            _includeFields = options._includeFields;
            _propertyNameCaseInsensitive = options._propertyNameCaseInsensitive;
        

            Converters = new ConverterList(this, (ConverterList)options.Converters);
            EffectiveMaxDepth = options.EffectiveMaxDepth;

        }

        internal BinaryReaderOptions GetReaderOptions()
        {
            return new BinaryReaderOptions
            {
                MaxDepth = MaxDepth
            };
        }

        public int MaxDepth
        {
            get => _maxDepth;
            set
            {
                VerifyMutable();

                if (value < 0)
                {
                    throw ThrowHelper.GetArgumentOutOfRangeException_MaxDepthMustBePositive(nameof(value));
                }

                _maxDepth = value;
                EffectiveMaxDepth = (value == 0 ? BinaryReaderOptions.DefaultMaxDepth : value);
            }
        }

        internal MemberAccessor MemberAccessorStrategy
        {
            get
            {
                if (_memberAccessorStrategy == null)
                {
                    _memberAccessorStrategy = new ReflectionEmitMemberAccessor();
                }

                return _memberAccessorStrategy;
            }
        }


        internal BinaryClassInfo GetOrAddClass(Type type)
        {
            _haveTypesBeenCreated = true;

            // todo: for performance and reduced instances, consider using the converters and BinaryClassInfo from s_defaultOptions by cloning (or reference directly if no changes).
            // https://github.com/dotnet/runtime/issues/32357
            if (!_classes.TryGetValue(type, out BinaryClassInfo result))
            {
                result = _classes.GetOrAdd(type, new BinaryClassInfo(type, _typeMap, this));
            }

            return result;
        }

        /// <summary>
        /// 获取类型的名称
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetTypeFullName(Type type)
        {
            return (TypeHandler?.CreateResolver() ?? TypeHandler.DefaultTypeResolver).TryGetTypeFullName(type);
        }

        internal bool TypeIsCached(Type type)
        {
            return _classes.ContainsKey(type);
        }

        public int DefaultBufferSize
        {
            get
            {
                return _defaultBufferSize;
            }
            set
            {
                VerifyMutable();

                if (value < 1)
                {
                    throw new ArgumentException("缓冲区值过小");
                }

                _defaultBufferSize = value;
            }
        }

        //internal BinarySerializationContext GetSerializationContext(Type type)
        //{
        //    if (_haveTypesBeenCreated == false)
        //    {
        //        // 初始化MetadataGetter
        //        _metadataGetterList.InitMetadataGetterList();
        //    }
        //    _haveTypesBeenCreated = true;

        //    //var map = _typeMapCache.GetOrAdd(type, (t) =>
        //    //{
        //    //    return MetadataProvider.GetTypeMap(t);
        //    //});
        //    // 需要拷贝，因为在序列化过程中会动态插入实际类型
        //    var typeMap = new TypeMap();

        //    return new BinarySerializationContext(typeMap, MetadataProvider);
        //}

        /// <summary>
        /// Configures how object references are handled when reading and writing Binary.
        /// </summary>
        public ReferenceHandler ReferenceHandler
        {
            get => _referenceHandler;
            set
            {
                VerifyMutable();
                _referenceHandler = value;
            }
        }

        internal void VerifyMutable()
        {
          
            Debug.Assert(this != s_defaultOptions);

            if (_haveTypesBeenCreated)
            {
                throw new InvalidOperationException("序列化已经开始，不可再修改配置");
            }
        }


        /// <summary>
        /// Determines whether read-only properties are ignored during serialization.
        /// A property is read-only if it contains a public getter but not a public setter.
        /// The default value is false.
        /// </summary>
        /// <remarks>
        /// Read-only properties are not deserialized regardless of this setting.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this property is set after serialization or deserialization has occurred.
        /// </exception>
        public bool IgnoreReadOnlyProperties
        {
            get
            {
                return _ignoreReadOnlyProperties;
            }
            set
            {
                VerifyMutable();
                _ignoreReadOnlyProperties = value;
            }
        }

        /// <summary>
        /// Determines whether read-only fields are ignored during serialization.
        /// A property is read-only if it isn't marked with the <c>readonly</c> keyword.
        /// The default value is false.
        /// </summary>
        /// <remarks>
        /// Read-only fields are not deserialized regardless of this setting.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this property is set after serialization or deserialization has occurred.
        /// </exception>
        public bool IgnoreReadOnlyFields
        {
            get
            {
                return _ignoreReadonlyFields;
            }
            set
            {
                VerifyMutable();
                _ignoreReadonlyFields = value;
            }
        }

        /// <summary>
        /// Specifies a condition to determine when properties with default values are ignored during serialization or deserialization.
        /// The default value is <see cref="BinaryIgnoreCondition.Never" />.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Thrown if this property is set to <see cref="BinaryIgnoreCondition.Always"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this property is set after serialization or deserialization has occurred,
        /// or <see cref="IgnoreNullValues"/> has been set to <see langword="true"/>. These properties cannot be used together.
        /// </exception>
        public BinaryIgnoreCondition DefaultIgnoreCondition
        {
            get
            {
                return _defaultIgnoreCondition;
            }
            set
            {
                VerifyMutable();

                if (value == BinaryIgnoreCondition.Always)
                {
                    throw new ArgumentException(Strings.DefaultIgnoreConditionInvalid);
                }

                if (value != BinaryIgnoreCondition.Never && _ignoreNullValues)
                {
                    throw new InvalidOperationException(Strings.DefaultIgnoreConditionAlreadySpecified);
                }

                _defaultIgnoreCondition = value;
            }
        }


        /// <summary>
        /// Determines whether null values are ignored during serialization and deserialization.
        /// The default value is false.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this property is set after serialization or deserialization has occurred.
        /// or <see cref="DefaultIgnoreCondition"/> has been set to a non-default value. These properties cannot be used together.
        /// </exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IgnoreNullValues
        {
            get
            {
                return _ignoreNullValues;
            }
            set
            {
                VerifyMutable();

                if (value && _defaultIgnoreCondition != BinaryIgnoreCondition.Never)
                {
                    throw new InvalidOperationException(Strings.DefaultIgnoreConditionAlreadySpecified);
                }

                _ignoreNullValues = value;
            }
        }

        public bool PropertyNameCaseInsensitive
        {
            get
            {
                return _propertyNameCaseInsensitive;
            }
            set
            {
                VerifyMutable();
                _propertyNameCaseInsensitive = value;
            }
        }

        /// <summary>
        /// Determines whether fields are handled serialization and deserialization.
        /// The default value is false.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this property is set after serialization or deserialization has occurred.
        /// </exception>
        public bool IncludeFields
        {
            get
            {
                return _includeFields;
            }
            set
            {
                VerifyMutable();
                _includeFields = value;
            }
        }

        internal int EffectiveMaxDepth { get; private set; } = DefaultMaxDepth;

       
    }
}
