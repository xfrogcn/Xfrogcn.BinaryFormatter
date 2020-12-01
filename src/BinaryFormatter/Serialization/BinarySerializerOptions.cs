using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Xfrogcn.BinaryFormatter.Metadata;
using Xfrogcn.BinaryFormatter.Resources;
using Xfrogcn.BinaryFormatter.Serialization;

namespace Xfrogcn.BinaryFormatter
{
    public sealed partial class BinarySerializerOptions
    {
        internal const int BufferSizeDefault = 16 * 1024;
        private int _defaultBufferSize = BufferSizeDefault;
        internal static readonly BinarySerializerOptions s_defaultOptions = new BinarySerializerOptions();

        private readonly ConcurrentDictionary<Type, TypeMap> _typeMapCache = new ConcurrentDictionary<Type, TypeMap>();
        private readonly ConcurrentDictionary<Type, BinaryClassInfo> _classes = new ConcurrentDictionary<Type, BinaryClassInfo>();

        private MemberAccessor _memberAccessorStrategy;
        private BinaryIgnoreCondition _defaultIgnoreCondition;

        private bool _haveTypesBeenCreated;

        private bool _ignoreNullValues;
        private bool _ignoreReadOnlyProperties;
        private bool _ignoreReadonlyFields;

       
        public IMetadataProvider MetadataProvider { get; }

        private MetadataGetterList _metadataGetterList = null;
        public IList<IMetadataGetter> MetadataGetterList => _metadataGetterList;

        public BinarySerializerOptions()
        {
            _metadataGetterList = new MetadataGetterList(this);
            MetadataProvider = new DefaultMetadataProvider(_metadataGetterList);
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
                result = _classes.GetOrAdd(type, new BinaryClassInfo(type, this));
            }

            return result;
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

        internal BinarySerializationContext GetSerializationContext(Type type)
        {
            if (_haveTypesBeenCreated == false)
            {
                // 初始化MetadataGetter
                _metadataGetterList.InitMetadataGetterList();
            }
            _haveTypesBeenCreated = true;

            var map = _typeMapCache.GetOrAdd(type, (t) =>
            {
                return MetadataProvider.GetTypeMap(t);
            });
            // 需要拷贝，因为在序列化过程中会动态插入实际类型
            var typeMap = new TypeMap(map);

            return new BinarySerializationContext(typeMap, MetadataProvider);
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

    }
}
