﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Xfrogcn.BinaryFormatter.Resources {
    using System;
    
    
    /// <summary>
    ///   一个强类型的资源类，用于查找本地化的字符串等。
    /// </summary>
    // 此类是由 StronglyTypedResourceBuilder
    // 类通过类似于 ResGen 或 Visual Studio 的工具自动生成的。
    // 若要添加或移除成员，请编辑 .ResX 文件，然后重新运行 ResGen
    // (以 /str 作为命令选项)，或重新生成 VS 项目。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   返回此类使用的缓存的 ResourceManager 实例。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Xfrogcn.BinaryFormatter.Resources.Strings", typeof(Strings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   重写当前线程的 CurrentUICulture 属性，对
        ///   使用此强类型资源类的所有资源查找执行重写。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   查找类似 The non-public property &apos;{0}&apos; on type &apos;{1}&apos; is annotated with &apos;BinaryIncludeAttribute&apos; which is invalid. 的本地化字符串。
        /// </summary>
        internal static string BinaryIncludeOnNonPublicInvalid {
            get {
                return ResourceManager.GetString("BinaryIncludeOnNonPublicInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The type &apos;{0}&apos; of property &apos;{1}&apos; on type &apos;{2}&apos; is invalid for serialization or deserialization because it is a pointer type, is a ref struct, or contains generic parameters that have not been replaced by specific types. 的本地化字符串。
        /// </summary>
        internal static string CannotSerializeInvalidMember {
            get {
                return ResourceManager.GetString("CannotSerializeInvalidMember", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The type &apos;{0}&apos; is invalid for serialization or deserialization because it is a pointer type, is a ref struct, or contains generic parameters that have not been replaced by specific types. 的本地化字符串。
        /// </summary>
        internal static string CannotSerializeInvalidType {
            get {
                return ResourceManager.GetString("CannotSerializeInvalidType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The converter &apos;{0}&apos; handles type &apos;{1}&apos; but is being asked to convert type &apos;{2}&apos;. Either create a separate converter for type &apos;{2}&apos; or change the converter&apos;s &apos;CanConvert&apos; method to only return &apos;true&apos; for a single type. 的本地化字符串。
        /// </summary>
        internal static string ConverterCanConvertNullableRedundant {
            get {
                return ResourceManager.GetString("ConverterCanConvertNullableRedundant", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 &apos;IgnoreNullValues&apos; and &apos;DefaultIgnoreCondition&apos; cannot both be set to non-default values. 的本地化字符串。
        /// </summary>
        internal static string DefaultIgnoreConditionAlreadySpecified {
            get {
                return ResourceManager.GetString("DefaultIgnoreConditionAlreadySpecified", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The value cannot be &apos;BinaryIgnoreCondition.Always&apos;. 的本地化字符串。
        /// </summary>
        internal static string DefaultIgnoreConditionInvalid {
            get {
                return ResourceManager.GetString("DefaultIgnoreConditionInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The Binary value could not be converted to {0}. 的本地化字符串。
        /// </summary>
        internal static string DeserializeUnableToConvertValue {
            get {
                return ResourceManager.GetString("DeserializeUnableToConvertValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The extension data property &apos;{0}&apos; on type &apos;{1}&apos; cannot bind with a parameter in constructor &apos;{2}&apos;. 的本地化字符串。
        /// </summary>
        internal static string ExtensionDataCannotBindToCtorParam {
            get {
                return ResourceManager.GetString("ExtensionDataCannotBindToCtorParam", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The &apos;IBufferWriter&apos; could not provide an output buffer that is large enough to continue writing. 的本地化字符串。
        /// </summary>
        internal static string FailedToGetLargerSpan {
            get {
                return ResourceManager.GetString("FailedToGetLargerSpan", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The ignore condition &apos;BinaryIgnoreCondition.WhenWritingNull&apos; is not valid on value-type member &apos;{0}&apos; on type &apos;{1}&apos;. Consider using &apos;BinaryIgnoreCondition.WhenWritingDefault&apos;. 的本地化字符串。
        /// </summary>
        internal static string IgnoreConditionOnValueTypeInvalid {
            get {
                return ResourceManager.GetString("IgnoreConditionOnValueTypeInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Members &apos;{0}&apos; and &apos;{1}&apos; on type &apos;{2}&apos; cannot both bind with parameter &apos;{3}&apos; in constructor &apos;{4}&apos; on deserialization. 的本地化字符串。
        /// </summary>
        internal static string MultipleMembersBindWithConstructorParameter {
            get {
                return ResourceManager.GetString("MultipleMembersBindWithConstructorParameter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The converter &apos;{0}&apos; is not compatible with the type &apos;{1}&apos;. 的本地化字符串。
        /// </summary>
        internal static string SerializationConverterNotCompatible {
            get {
                return ResourceManager.GetString("SerializationConverterNotCompatible", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The converter specified on &apos;{0}&apos; does not derive from BinaryConverter or have a public parameterless constructor. 的本地化字符串。
        /// </summary>
        internal static string SerializationConverterOnAttributeInvalid {
            get {
                return ResourceManager.GetString("SerializationConverterOnAttributeInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The converter specified on &apos;{0}&apos; is not compatible with the type &apos;{1}&apos;. 的本地化字符串。
        /// </summary>
        internal static string SerializationConverterOnAttributeNotCompatible {
            get {
                return ResourceManager.GetString("SerializationConverterOnAttributeNotCompatible", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The data extension property &apos;{0}.{1}&apos; does not match the required signature of IDictionary&lt;string, BinaryElement&gt; or IDictionary&lt;string, object&gt;. 的本地化字符串。
        /// </summary>
        internal static string SerializationDataExtensionPropertyInvalid {
            get {
                return ResourceManager.GetString("SerializationDataExtensionPropertyInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The attribute &apos;{0}&apos; cannot exist more than once on &apos;{1}&apos;. 的本地化字符串。
        /// </summary>
        internal static string SerializationDuplicateAttribute {
            get {
                return ResourceManager.GetString("SerializationDuplicateAttribute", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The type &apos;{0}&apos; cannot have more than one property that has the attribute &apos;{1}&apos;. 的本地化字符串。
        /// </summary>
        internal static string SerializationDuplicateTypeAttribute {
            get {
                return ResourceManager.GetString("SerializationDuplicateTypeAttribute", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The unsupported member type is located on type &apos;{0}&apos;. 的本地化字符串。
        /// </summary>
        internal static string SerializationNotSupportedParentType {
            get {
                return ResourceManager.GetString("SerializationNotSupportedParentType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The type &apos;{0}&apos; is not supported. 的本地化字符串。
        /// </summary>
        internal static string SerializationNotSupportedType {
            get {
                return ResourceManager.GetString("SerializationNotSupportedType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The converter &apos;{0}&apos; cannot return a null value. 的本地化字符串。
        /// </summary>
        internal static string SerializerConverterFactoryReturnsNull {
            get {
                return ResourceManager.GetString("SerializerConverterFactoryReturnsNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The Binary property name for &apos;{0}.{1}&apos; collides with another property. 的本地化字符串。
        /// </summary>
        internal static string SerializerPropertyNameConflict {
            get {
                return ResourceManager.GetString("SerializerPropertyNameConflict", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The Binary property name for &apos;{0}.{1}&apos; cannot be null. 的本地化字符串。
        /// </summary>
        internal static string SerializerPropertyNameNull {
            get {
                return ResourceManager.GetString("SerializerPropertyNameNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The object or value could not be serialized. 的本地化字符串。
        /// </summary>
        internal static string SerializeUnableToSerialize {
            get {
                return ResourceManager.GetString("SerializeUnableToSerialize", resourceCulture);
            }
        }
    }
}
