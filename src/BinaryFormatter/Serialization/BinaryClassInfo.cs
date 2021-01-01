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

        internal ushort TypeSeq { get; set; }

        public delegate object ConstructorDelegate();

        public delegate T ParameterizedConstructorDelegate<T>(object[] arguments);

        public delegate T ParameterizedConstructorDelegate<T, TArg0, TArg1, TArg2, TArg3>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3);

        public ConstructorDelegate CreateObject { get; private set; }

        public object CreateObjectWithArgs { get; set; }

        // Add method delegate for non-generic Stack and Queue; and types that derive from them.
        public object AddMethodDelegate { get; set; }

        /// <summary>
        /// 获取集合数量的方法委托
        /// </summary>
        public object CountMethodDelegate { get; set; }

   
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

            ClassType = converter.ClassType;

            PropertyInfoForClassInfo = CreatePropertyInfoForClassInfo(TypeMap, Type, runtimeType, converter, Options);
            TypeSeq = converter.GetTypeSeq(typeMap, options);

            switch (ClassType)
            {
                case ClassType.Object:
                    {
                        CreateObject = Options.MemberAccessorStrategy.CreateConstructor(type);
                        GetObjectMemberInfos(type, typeMap, converter);
                    }
                    break;
                case ClassType.Enumerable:
                case ClassType.Dictionary:
                    {
                        ElementType = converter.ElementType;
                        CreateObject = Options.MemberAccessorStrategy.CreateConstructor(runtimeType);
                        GetObjectMemberInfos(type, typeMap, converter, true);
                    }
                    break;
                case ClassType.Value:
                case ClassType.NewValue:
                    {
                        CreateObject = Options.MemberAccessorStrategy.CreateConstructor(type);
                    }
                    break;
                case ClassType.None:
                    {
                        ThrowHelper.ThrowNotSupportedException_SerializationNotSupported(type);
                    }
                    break;
                default:
                    Debug.Fail($"Unexpected class type: {ClassType}");
                    throw new InvalidOperationException();
            }
        }

        private void GetObjectMemberInfos(Type type, TypeMap typeMap, BinaryConverter converter, bool ignoreReadOnly = false)
        {
            Dictionary<string, BinaryPropertyInfo> cache = new Dictionary<string, BinaryPropertyInfo>(
                                        Options.PropertyNameCaseInsensitive
                                            ? StringComparer.OrdinalIgnoreCase
                                            : StringComparer.Ordinal);

            Dictionary<string, MemberInfo> ignoredMembers = null;

            // We start from the most derived type.
            for (Type currentType = type; currentType != null; currentType = currentType.BaseType)
            {
                const BindingFlags bindingFlags =
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly;

                foreach (PropertyInfo propertyInfo in currentType.GetProperties(bindingFlags))
                {
                    // Ignore indexers and virtual properties that have overrides that were [BinaryIgnore]d.
                    if (propertyInfo.GetIndexParameters().Length > 0 || PropertyIsOverridenAndIgnored(propertyInfo, ignoredMembers))
                    {
                        continue;
                    }

                    // For now we only support public properties (i.e. setter and/or getter is public).
                    if (propertyInfo.GetMethod?.IsPublic == true ||
                        propertyInfo.SetMethod?.IsPublic == true)
                    {
                        if(!propertyInfo.CanWrite && ignoreReadOnly)
                        {
                            continue;
                        }
                        CacheMember(currentType, propertyInfo.PropertyType, propertyInfo, cache, ref ignoredMembers);
                    }
                    else
                    {
                        if (BinaryPropertyInfo.GetAttribute<BinaryIncludeAttribute>(propertyInfo) != null)
                        {
                            ThrowHelper.ThrowInvalidOperationException_BinaryIncludeOnNonPublicInvalid(propertyInfo, currentType);
                        }

                        // Non-public properties should not be included for (de)serialization.
                    }
                }

                foreach (FieldInfo fieldInfo in currentType.GetFields(bindingFlags))
                {
                    if (PropertyIsOverridenAndIgnored(fieldInfo, ignoredMembers))
                    {
                        continue;
                    }

                    bool hasBinaryInclude = BinaryPropertyInfo.GetAttribute<BinaryIncludeAttribute>(fieldInfo) != null;

                    if (fieldInfo.IsPublic)
                    {
                        if (hasBinaryInclude || Options.IncludeFields || converter.IncludeFields)
                        {
                            CacheMember(currentType, fieldInfo.FieldType, fieldInfo, cache, ref ignoredMembers);
                        }
                    }
                    else
                    {
                        if (hasBinaryInclude)
                        {
                            ThrowHelper.ThrowInvalidOperationException_BinaryIncludeOnNonPublicInvalid(fieldInfo, currentType);
                        }

                        // Non-public fields should not be included for (de)serialization.
                    }
                }

                var methods = converter.GetAdditionalDataMethod();
                if (methods != null)
                {
                    foreach(var m in methods)
                    {
                        CacheMember(currentType, m.ReturnType, m, cache, ref ignoredMembers);
                    }
                }
            }

            BinaryPropertyInfo[] cacheArray;
            if (DetermineExtensionDataProperty(cache))
            {
                // Remove from cache since it is handled independently.
                cache.Remove(DataExtensionProperty!.NameAsString);

                cacheArray = new BinaryPropertyInfo[cache.Count + 1];

                // Set the last element to the extension property.
                cacheArray[cache.Count] = DataExtensionProperty;
            }
            else
            {
                cacheArray = new BinaryPropertyInfo[cache.Count];
            }

            // Copy the dictionary cache to the array cache.
            cache.Values.CopyTo(cacheArray, 0);

            // These are not accessed by other threads until the current BinaryClassInfo instance
            // is finished initializing and added to the cache on BinarySerializerOptions.
            PropertyCache = cache;
            PropertyCacheArray = cacheArray;

            if (converter.ConstructorIsParameterized)
            {
                InitializeConstructorParameters(converter.ConstructorInfo!);
            }

            typeMap.TrySetTypeMemberInfos(TypeSeq, this.GetMemberInfos);
        }

        private int _propertySeq = 0;
        private void CacheMember(
            Type declaringType,
            Type memberType,
            MemberInfo memberInfo,
            Dictionary<string, BinaryPropertyInfo> cache,
            ref Dictionary<string, MemberInfo> ignoredMembers)
        {
            BinaryPropertyInfo binaryPropertyInfo = AddProperty(TypeMap, memberInfo, memberType, declaringType, Options);
            Debug.Assert(binaryPropertyInfo.NameAsString != null);

            string memberName = memberInfo.Name;

            // The BinaryPropertyNameAttribute or naming policy resulted in a collision.
            if (!cache.TryAdd(binaryPropertyInfo.NameAsString, binaryPropertyInfo))
            {
                BinaryPropertyInfo other = cache[binaryPropertyInfo.NameAsString];

                if (other.IsIgnored)
                {
                    // Overwrite previously cached property since it has [BinaryIgnore].
                    cache[binaryPropertyInfo.NameAsString] = binaryPropertyInfo;
                }
                else if (
                    // Does the current property have `BinaryIgnoreAttribute`?
                    !binaryPropertyInfo.IsIgnored &&
                    // Is the current property hidden by the previously cached property
                    // (with `new` keyword, or by overriding)?
                    other.MemberInfo!.Name != memberName &&
                    // Was a property with the same CLR name was ignored? That property hid the current property,
                    // thus, if it was ignored, the current property should be ignored too.
                    ignoredMembers?.ContainsKey(memberName) != true)
                {
                    // We throw if we have two public properties that have the same property name, and neither have been ignored.
                    ThrowHelper.ThrowInvalidOperationException_SerializerPropertyNameConflict(Type, binaryPropertyInfo);
                }
                // Ignore the current property.
            }
            else
            {
                int seq =  System.Threading.Interlocked.Increment(ref _propertySeq);
                binaryPropertyInfo.Seq = (ushort)seq;
            }

            if (binaryPropertyInfo.IsIgnored)
            {
                (ignoredMembers ??= new Dictionary<string, MemberInfo>()).Add(memberName, memberInfo);
            }
        }

        /// <summary>
        /// 检查是否具有扩展数据字段
        /// </summary>
        /// <param name="cache"></param>
        /// <returns></returns>
        public bool DetermineExtensionDataProperty(Dictionary<string, BinaryPropertyInfo> cache)
        {
            // 一个类型中只能由一个扩展字段(由BinaryExtensionDataAttribute标注)
            BinaryPropertyInfo binaryPropertyInfo = GetPropertyWithUniqueAttribute(Type, typeof(BinaryExtensionDataAttribute), cache);
            if (binaryPropertyInfo != null)
            {
                Type declaredPropertyType = binaryPropertyInfo.DeclaredPropertyType;
                if (typeof(IDictionary<string, object>).IsAssignableFrom(declaredPropertyType))
                {
                    BinaryConverter converter = Options.GetConverter(declaredPropertyType);
                    Debug.Assert(converter != null);
                }
                else
                {
                    ThrowHelper.ThrowInvalidOperationException_SerializationDataExtensionPropertyInvalid(Type, binaryPropertyInfo);
                }

                DataExtensionProperty = binaryPropertyInfo;
                return true;
            }

            return false;
        }

        private static BinaryPropertyInfo GetPropertyWithUniqueAttribute(Type classType, Type attributeType, Dictionary<string, BinaryPropertyInfo> cache)
        {
            BinaryPropertyInfo property = null;

            foreach (BinaryPropertyInfo binaryPropertyInfo in cache.Values)
            {
                Debug.Assert(binaryPropertyInfo.MemberInfo != null);
                Attribute attribute = binaryPropertyInfo.MemberInfo.GetCustomAttribute(attributeType);
                if (attribute != null)
                {
                    if (property != null)
                    {
                        ThrowHelper.ThrowInvalidOperationException_SerializationDuplicateTypeAttribute(classType, attributeType);
                    }

                    property = binaryPropertyInfo;
                }
            }

            return property;
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

        internal static BinaryPropertyInfo CreatePropertyInfoForClassInfo(
            TypeMap typeMap,
            Type declaredPropertyType,
            Type runtimePropertyType,
            BinaryConverter converter,
            BinarySerializerOptions options)
        {
       
            BinaryPropertyInfo binaryPropertyInfo = CreateProperty(
                typeMap,
                declaredPropertyType: declaredPropertyType,
                runtimePropertyType: runtimePropertyType,
                memberInfo: null, // Not a real property so this is null.
                parentClassType: BinaryClassInfo.ObjectType, // a dummy value (not used)
                converter: converter,
                options);

            Debug.Assert(binaryPropertyInfo.IsForClassInfo);

            return binaryPropertyInfo;
        }

        #region
        private sealed class ParameterLookupKey
        {
            public ParameterLookupKey(string name, Type type)
            {
                Name = name;
                Type = type;
            }

            public string Name { get; }
            public Type Type { get; }

            public override int GetHashCode()
            {
                return StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
            }

            public override bool Equals(object obj)
            {
                Debug.Assert(obj is ParameterLookupKey);

                ParameterLookupKey other = (ParameterLookupKey)obj;
                return Type == other.Type && string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
            }
        }

        private sealed class ParameterLookupValue
        {
            public ParameterLookupValue(BinaryPropertyInfo binaryPropertyInfo)
            {
                BinaryPropertyInfo = binaryPropertyInfo;
            }

            public string DuplicateName { get; set; }
            public BinaryPropertyInfo BinaryPropertyInfo { get; }
        }

        #endregion
        /// <summary>
        /// 初始化构造函数参数
        /// </summary>
        /// <param name="constructorInfo"></param>
        private void InitializeConstructorParameters(ConstructorInfo constructorInfo)
        {
            ParameterInfo[] parameters = constructorInfo.GetParameters();
            var parameterCache = new Dictionary<string, BinaryParameterInfo>(
                parameters.Length, Options.PropertyNameCaseInsensitive ? StringComparer.OrdinalIgnoreCase : null);

            static Type GetMemberType(MemberInfo memberInfo)
            {
                Debug.Assert(memberInfo is PropertyInfo || memberInfo is FieldInfo);

                return memberInfo is PropertyInfo propertyInfo
                    ? propertyInfo.PropertyType
                    : (memberInfo as FieldInfo).FieldType;
            }

            // 注意此处ParameterLookupKey使用不区分大小写比对方式
            var nameLookup = new Dictionary<ParameterLookupKey, ParameterLookupValue>(PropertyCacheArray!.Length);

            foreach (BinaryPropertyInfo binaryProperty in PropertyCacheArray!)
            {
                string propertyName = binaryProperty.MemberInfo!.Name;
                var key = new ParameterLookupKey(propertyName, GetMemberType(binaryProperty.MemberInfo));
                var value = new ParameterLookupValue(binaryProperty);
                if (!nameLookup.TryAdd(key, value))
                {
                    // More than one property has the same case-insensitive name and Type.
                    // Remember so we can throw a nice exception if this property is used as a parameter name.
                    ParameterLookupValue existing = nameLookup[key];
                    existing!.DuplicateName = propertyName;
                }
            }

            foreach (ParameterInfo parameterInfo in parameters)
            {
                var paramToCheck = new ParameterLookupKey(parameterInfo.Name!, parameterInfo.ParameterType);

                if (nameLookup.TryGetValue(paramToCheck, out ParameterLookupValue matchingEntry))
                {
                    if (matchingEntry.DuplicateName != null)
                    {
                        // Multiple object properties cannot bind to the same constructor parameter.
                        ThrowHelper.ThrowInvalidOperationException_MultiplePropertiesBindToConstructorParameters(
                            Type,
                            parameterInfo.Name!,
                            matchingEntry.BinaryPropertyInfo.NameAsString,
                            matchingEntry.DuplicateName,
                            constructorInfo);
                    }

                    Debug.Assert(matchingEntry.BinaryPropertyInfo != null);
                    BinaryPropertyInfo binaryPropertyInfo = matchingEntry.BinaryPropertyInfo;
                    BinaryParameterInfo binaryParameterInfo = AddConstructorParameter(TypeMap, parameterInfo, binaryPropertyInfo, Options);
                    parameterCache.Add(binaryPropertyInfo.NameAsString, binaryParameterInfo);

                    // Remove property from deserialization cache to reduce the number of BinaryPropertyInfos considered during Binary matching.
                    PropertyCache!.Remove(binaryPropertyInfo.NameAsString);
                }
            }

            // It is invalid for the extension data property to bind with a constructor argument.
            if (DataExtensionProperty != null &&
                parameterCache.ContainsKey(DataExtensionProperty.NameAsString))
            {
                ThrowHelper.ThrowInvalidOperationException_ExtensionDataCannotBindToCtorParam(DataExtensionProperty.MemberInfo!, Type, constructorInfo);
            }

            ParameterCache = parameterCache;
            ParameterCount = parameters.Length;
        }

        private static BinaryParameterInfo AddConstructorParameter(
            TypeMap typeMap,
            ParameterInfo parameterInfo,
            BinaryPropertyInfo binaryPropertyInfo,
            BinarySerializerOptions options)
        {
            if (binaryPropertyInfo.IsIgnored)
            {
                return BinaryParameterInfo.CreateIgnoredParameterPlaceholder(binaryPropertyInfo);
            }

            BinaryConverter converter = binaryPropertyInfo.ConverterBase;
            BinaryParameterInfo binaryParameterInfo = converter.CreateBinaryParameterInfo();

            binaryParameterInfo.Initialize(
                typeMap,
                binaryPropertyInfo.RuntimePropertyType!,
                parameterInfo,
                binaryPropertyInfo,
                options);

            return binaryParameterInfo;
        }


        private static bool PropertyIsOverridenAndIgnored(MemberInfo currentMember, Dictionary<string, MemberInfo> ignoredMembers)
        {
            if (ignoredMembers == null || !ignoredMembers.TryGetValue(currentMember.Name, out MemberInfo ignoredProperty))
            {
                return false;
            }

            Debug.Assert(currentMember is PropertyInfo || currentMember is FieldInfo);
            PropertyInfo currentPropertyInfo = currentMember as PropertyInfo;
            Type currentMemberType = currentPropertyInfo == null
                ? (currentMember as FieldInfo).FieldType
                : currentPropertyInfo.PropertyType;

            Debug.Assert(ignoredProperty is PropertyInfo || ignoredProperty is FieldInfo);
            PropertyInfo ignoredPropertyInfo = ignoredProperty as PropertyInfo;
            Type ignoredPropertyType = ignoredPropertyInfo == null
                ? (ignoredProperty as FieldInfo).FieldType
                : ignoredPropertyInfo.PropertyType;

            return currentMemberType == ignoredPropertyType &&
                PropertyIsVirtual(currentPropertyInfo) &&
                PropertyIsVirtual(ignoredPropertyInfo);
        }

        private static bool PropertyIsVirtual(PropertyInfo propertyInfo)
        {
            return propertyInfo != null && (propertyInfo.GetMethod?.IsVirtual == true || propertyInfo.SetMethod?.IsVirtual == true);
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
