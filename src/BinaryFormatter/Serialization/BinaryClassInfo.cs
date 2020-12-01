using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    [DebuggerDisplay("ClassType.{ClassType}, {Type.Name}")]
    internal sealed partial class BinaryClassInfo
    {
        /// <summary>
        /// Cached typeof(object). It is faster to cache this than to call typeof(object) multiple times.
        /// </summary>
        public static readonly Type ObjectType = typeof(object);


        public BinaryClassInfo(Type type, BinarySerializerOptions options)
        {
            //Type = type;
            //Options = options;

            //JsonConverter converter = GetConverter(
            //    Type,
            //    parentClassType: null, // A ClassInfo never has a "parent" class.
            //    memberInfo: null, // A ClassInfo never has a "parent" property.
            //    out Type runtimeType,
            //    Options);

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

    }
}
