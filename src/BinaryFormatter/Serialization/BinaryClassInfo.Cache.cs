using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Xfrogcn.BinaryFormatter.Serialization;

namespace Xfrogcn.BinaryFormatter
{
    [DebuggerDisplay("ClassType.{ClassType}, {Type.Name}")]
    internal sealed partial class BinaryClassInfo
    {
        // The number of parameters the deserialization constructor has. If this is not equal to ParameterCache.Count, this means
        // that not all parameters are bound to object properties, and an exception will be thrown if deserialization is attempted.
        public int ParameterCount { get; private set; }

        // All of the serializable parameters on a POCO constructor keyed on parameter name.
        // Only paramaters which bind to properties are cached.
        public Dictionary<string, BinaryParameterInfo> ParameterCache;

        // All of the serializable properties on a POCO (except the optional extension property) keyed on property name.
        public Dictionary<string, BinaryPropertyInfo> PropertyCache;

        // All of the serializable properties on a POCO including the optional extension property.
        // Used for performance during serialization instead of 'PropertyCache' above.
        public BinaryPropertyInfo[] PropertyCacheArray;


        public BinaryMemberInfo[] GetMemberInfos()
        {
            if (PropertyCacheArray == null || PropertyCacheArray.Length == 0)
            {
                return new BinaryMemberInfo[0];
            }

            return PropertyCacheArray.Select(p => p.GetBinaryMemberInfo()).ToArray();
        }

        public static BinaryPropertyInfo AddProperty(
            TypeMap typeMap,
            MemberInfo memberInfo,
            Type memberType,
            Type parentClassType,
            BinarySerializerOptions options)
        {
            BinaryIgnoreCondition? ignoreCondition = BinaryPropertyInfo.GetAttribute<BinaryIgnoreAttribute>(memberInfo)?.Condition;
            if (ignoreCondition == BinaryIgnoreCondition.Always)
            {
                return BinaryPropertyInfo.CreateIgnoredPropertyPlaceholder(typeMap, memberInfo, options);
            }

            BinaryConverter converter = GetConverter(
                memberType,
                parentClassType,
                memberInfo,
                out Type runtimeType,
                options);

            return CreateProperty(
                typeMap,
                declaredPropertyType: memberType,
                runtimePropertyType: runtimeType,
                memberInfo,
                parentClassType,
                converter,
                options,
                ignoreCondition);
        }
        internal static BinaryPropertyInfo CreateProperty(
            TypeMap typeMap,
            Type declaredPropertyType,
            Type runtimePropertyType,
            MemberInfo memberInfo,
            Type parentClassType,
            BinaryConverter converter,
            BinarySerializerOptions options,
            BinaryIgnoreCondition? ignoreCondition = null)
        {
            // Create the BinaryPropertyInfo instance.
            BinaryPropertyInfo binaryPropertyInfo = converter.CreateBinaryPropertyInfo();

            binaryPropertyInfo.Initialize(
                typeMap,
                parentClassType,
                declaredPropertyType,
                runtimePropertyType,
                runtimeClassType: converter.ClassType,
                memberInfo,
                converter,
                ignoreCondition,
                options);

            return binaryPropertyInfo;
        }
    }
}
