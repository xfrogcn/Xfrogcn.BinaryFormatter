using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using Xfrogcn.BinaryFormatter.Serialization;

namespace Xfrogcn.BinaryFormatter
{
    [DebuggerDisplay("ClassType.{ClassType}, {Type.Name}")]
    internal sealed partial class BinaryClassInfo
    {
        /// <summary>
        /// Cached typeof(object). It is faster to cache this than to call typeof(object) multiple times.
        /// </summary>
        public static readonly Type ObjectType = typeof(object);

        public  TypeMap TypeMap { get; private set; }

        public ClassType ClassType { get; private set; }

        public delegate object ConstructorDelegate();

        public delegate T ParameterizedConstructorDelegate<T>(object[] arguments);

        public delegate T ParameterizedConstructorDelegate<T, TArg0, TArg1, TArg2, TArg3>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3);

        public ConstructorDelegate CreateObject { get; private set; }

        public object CreateObjectWithArgs { get; set; }

        // Add method delegate for non-generic Stack and Queue; and types that derive from them.
        public object AddMethodDelegate { get; set; }

   
        public BinaryPropertyInfo DataExtensionProperty { get; private set; }

        // If enumerable, the BinaryClassInfo for the element type.
        private BinaryClassInfo _elementClassInfo;

        /// <summary>
        /// Return the BinaryClassInfo for the element type, or null if the type is not an enumerable or dictionary.
        /// </summary>
        /// <remarks>
        /// This should not be called during warm-up (initial creation of BinaryClassInfos) to avoid recursive behavior
        /// which could result in a StackOverflowException.
        /// </remarks>
        public BinaryClassInfo ElementClassInfo
        {
            get
            {
                if (_elementClassInfo == null && ElementType != null)
                {
                    Debug.Assert(ClassType == ClassType.Enumerable ||
                        ClassType == ClassType.Dictionary);

                    _elementClassInfo = Options.GetOrAddClass(ElementType);
                }

                return _elementClassInfo;
            }
        }

        public Type ElementType { get; set; }

        public BinarySerializerOptions Options { get; private set; }

        public Type Type { get; private set; }

      
        public BinaryPropertyInfo PropertyInfoForClassInfo { get; private set; }

        public BinaryClassInfo([NotNull] Type type, [NotNull]TypeMap typeMap, [NotNull] BinarySerializerOptions options)
        {
            TypeMap = typeMap;
            Type = type;
            Options = options;

            BinaryConverter converter = GetConverter(
                Type,
                parentClassType: null, // A ClassInfo never has a "parent" class.
                memberInfo: null, // A ClassInfo never has a "parent" property.
                out Type runtimeType,
                Options);

            //ClassType = converter.ClassType;
            //JsonNumberHandling? typeNumberHandling = GetNumberHandlingForType(Type);

            //PropertyInfoForClassInfo = CreatePropertyInfoForClassInfo(Type, runtimeType, converter, Options);

            //switch (ClassType)
            //{
            //    case ClassType.Object:
            //        {
            //            CreateObject = Options.MemberAccessorStrategy.CreateConstructor(type);
            //            Dictionary<string, JsonPropertyInfo> cache = new Dictionary<string, JsonPropertyInfo>(
            //                Options.PropertyNameCaseInsensitive
            //                    ? StringComparer.OrdinalIgnoreCase
            //                    : StringComparer.Ordinal);

            //            Dictionary<string, MemberInfo>? ignoredMembers = null;

            //            // We start from the most derived type.
            //            for (Type? currentType = type; currentType != null; currentType = currentType.BaseType)
            //            {
            //                const BindingFlags bindingFlags =
            //                    BindingFlags.Instance |
            //                    BindingFlags.Public |
            //                    BindingFlags.NonPublic |
            //                    BindingFlags.DeclaredOnly;

            //                foreach (PropertyInfo propertyInfo in currentType.GetProperties(bindingFlags))
            //                {
            //                    // Ignore indexers and virtual properties that have overrides that were [JsonIgnore]d.
            //                    if (propertyInfo.GetIndexParameters().Length > 0 || PropertyIsOverridenAndIgnored(propertyInfo, ignoredMembers))
            //                    {
            //                        continue;
            //                    }

            //                    // For now we only support public properties (i.e. setter and/or getter is public).
            //                    if (propertyInfo.GetMethod?.IsPublic == true ||
            //                        propertyInfo.SetMethod?.IsPublic == true)
            //                    {
            //                        CacheMember(currentType, propertyInfo.PropertyType, propertyInfo, typeNumberHandling, cache, ref ignoredMembers);
            //                    }
            //                    else
            //                    {
            //                        if (JsonPropertyInfo.GetAttribute<JsonIncludeAttribute>(propertyInfo) != null)
            //                        {
            //                            ThrowHelper.ThrowInvalidOperationException_JsonIncludeOnNonPublicInvalid(propertyInfo, currentType);
            //                        }

            //                        // Non-public properties should not be included for (de)serialization.
            //                    }
            //                }

            //                foreach (FieldInfo fieldInfo in currentType.GetFields(bindingFlags))
            //                {
            //                    if (PropertyIsOverridenAndIgnored(fieldInfo, ignoredMembers))
            //                    {
            //                        continue;
            //                    }

            //                    bool hasJsonInclude = JsonPropertyInfo.GetAttribute<JsonIncludeAttribute>(fieldInfo) != null;

            //                    if (fieldInfo.IsPublic)
            //                    {
            //                        if (hasJsonInclude || Options.IncludeFields)
            //                        {
            //                            CacheMember(currentType, fieldInfo.FieldType, fieldInfo, typeNumberHandling, cache, ref ignoredMembers);
            //                        }
            //                    }
            //                    else
            //                    {
            //                        if (hasJsonInclude)
            //                        {
            //                            ThrowHelper.ThrowInvalidOperationException_JsonIncludeOnNonPublicInvalid(fieldInfo, currentType);
            //                        }

            //                        // Non-public fields should not be included for (de)serialization.
            //                    }
            //                }
            //            }

            //            JsonPropertyInfo[] cacheArray;
            //            if (DetermineExtensionDataProperty(cache))
            //            {
            //                // Remove from cache since it is handled independently.
            //                cache.Remove(DataExtensionProperty!.NameAsString);

            //                cacheArray = new JsonPropertyInfo[cache.Count + 1];

            //                // Set the last element to the extension property.
            //                cacheArray[cache.Count] = DataExtensionProperty;
            //            }
            //            else
            //            {
            //                cacheArray = new JsonPropertyInfo[cache.Count];
            //            }

            //            // Copy the dictionary cache to the array cache.
            //            cache.Values.CopyTo(cacheArray, 0);

            //            // These are not accessed by other threads until the current JsonClassInfo instance
            //            // is finished initializing and added to the cache on JsonSerializerOptions.
            //            PropertyCache = cache;
            //            PropertyCacheArray = cacheArray;

            //            // Allow constructor parameter logic to remove items from the dictionary since the JSON
            //            // property values will be passed to the constructor and do not call a property setter.
            //            if (converter.ConstructorIsParameterized)
            //            {
            //                InitializeConstructorParameters(converter.ConstructorInfo!);
            //            }
            //        }
            //        break;
            //    case ClassType.Enumerable:
            //    case ClassType.Dictionary:
            //        {
            //            ElementType = converter.ElementType;
            //            CreateObject = Options.MemberAccessorStrategy.CreateConstructor(runtimeType);
            //        }
            //        break;
            //    case ClassType.Value:
            //    case ClassType.NewValue:
            //        {
            //            CreateObject = Options.MemberAccessorStrategy.CreateConstructor(type);
            //        }
            //        break;
            //    case ClassType.None:
            //        {
            //            ThrowHelper.ThrowNotSupportedException_SerializationNotSupported(type);
            //        }
            //        break;
            //    default:
            //        Debug.Fail($"Unexpected class type: {ClassType}");
            //        throw new InvalidOperationException();
            //}
        }


        // This method gets the runtime information for a given type or property.
        // The runtime information consists of the following:
        // - class type,
        // - runtime type,
        // - element type (if the type is a collection),
        // - the converter (either native or custom), if one exists.
        public static BinaryConverter GetConverter(
            Type type,
            Type parentClassType,
            MemberInfo memberInfo,
            out Type runtimeType,
            BinarySerializerOptions options)
        {
            Debug.Assert(type != null);
            ValidateType(type, parentClassType, memberInfo, options);

            BinaryConverter converter = options.DetermineConverter(parentClassType, type, memberInfo);

            // The runtimeType is the actual value being assigned to the property.
            // There are three types to consider for the runtimeType:
            // 1) The declared type (the actual property type).
            // 2) The converter.TypeToConvert (the T value that the converter supports).
            // 3) The converter.RuntimeType (used with interfaces such as IList).

            Type converterRuntimeType = converter.RuntimeType;
            if (type == converterRuntimeType)
            {
                runtimeType = type;
            }
            else
            {
                if (type.IsInterface)
                {
                    runtimeType = converterRuntimeType;
                }
                else if (converterRuntimeType.IsInterface)
                {
                    runtimeType = type;
                }
                else
                {
                    // Use the most derived version from the converter.RuntimeType or converter.TypeToConvert.
                    if (type.IsAssignableFrom(converterRuntimeType))
                    {
                        runtimeType = converterRuntimeType;
                    }
                    else if (converterRuntimeType.IsAssignableFrom(type) || converter.TypeToConvert.IsAssignableFrom(type))
                    {
                        runtimeType = type;
                    }
                    else
                    {
                        runtimeType = default!;
                        ThrowHelper.ThrowNotSupportedException_SerializationNotSupported(type);
                    }
                }
            }

            Debug.Assert(!IsInvalidForSerialization(runtimeType));

            return converter;
        }

        private static void ValidateType(Type type, Type parentClassType, MemberInfo memberInfo, BinarySerializerOptions options)
        {
            if (!options.TypeIsCached(type) && IsInvalidForSerialization(type))
            {
                ThrowHelper.ThrowInvalidOperationException_CannotSerializeInvalidType(type, parentClassType, memberInfo);
            }
        }

        private static bool IsInvalidForSerialization(Type type)
        {
            return type.IsPointer || IsByRefLike(type) || type.ContainsGenericParameters;
        }

        private static bool IsByRefLike(Type type)
        {
            return type.IsByRefLike;
        }

    }
}
