using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Xfrogcn.BinaryFormatter.Serialization;

namespace Xfrogcn.BinaryFormatter
{
    [DebuggerDisplay("MemberInfo={MemberInfo}")]
    internal abstract class BinaryPropertyInfo
    {
        public static readonly BinaryPropertyInfo s_missingProperty = GetPropertyPlaceholder();

        private BinaryClassInfo _runtimeClassInfo;

        public ClassType ClassType;

        internal ushort TypeSeq { get; set; }

        internal ushort Seq { get; set; }

        public TypeMap TypeMap { get; private set; }

        public abstract BinaryConverter ConverterBase { get; set; }

        public virtual BinaryMemberInfo GetBinaryMemberInfo()
        {
            return new BinaryMemberInfo()
            {
                TypeSeq = TypeSeq,
                Seq = Seq,
                NameAsUtf8Bytes = NameAsUtf8Bytes,
                NameAsString = NameAsString
            };
        }

        public static BinaryPropertyInfo GetPropertyPlaceholder()
        {
            BinaryPropertyInfo info = new BinaryPropertyInfo<object>();

            Debug.Assert(!info.IsForClassInfo);
            Debug.Assert(!info.ShouldDeserialize);
            Debug.Assert(!info.ShouldSerialize);

            info.NameAsString = string.Empty;


            return info;
        }

        // Create a property that is ignored at run-time. It uses the same type (typeof(sbyte)) to help
        // prevent issues with unsupported types and helps ensure we don't accidently (de)serialize it.
        public static BinaryPropertyInfo CreateIgnoredPropertyPlaceholder(TypeMap typeMap, MemberInfo memberInfo, BinarySerializerOptions options)
        {
            BinaryPropertyInfo binaryPropertyInfo = new BinaryPropertyInfo<sbyte>
            {
                Options = options,
                MemberInfo = memberInfo
            };
            binaryPropertyInfo.DeterminePropertyName();
            binaryPropertyInfo.IsIgnored = true;
            binaryPropertyInfo.TypeMap = typeMap;

            Debug.Assert(!binaryPropertyInfo.ShouldDeserialize);
            Debug.Assert(!binaryPropertyInfo.ShouldSerialize);

            return binaryPropertyInfo;
        }

        public Type DeclaredPropertyType { get; private set; } = null!;

        public virtual void GetPolicies(BinaryIgnoreCondition? ignoreCondition, bool defaultValueIsNull)
        {
            DetermineSerializationCapabilities(ignoreCondition);
            DeterminePropertyName();
            DetermineIgnoreCondition(ignoreCondition, defaultValueIsNull);
        }

        private void DeterminePropertyName()
        {
            if (MemberInfo == null)
            {
                return;
            }

            BinaryPropertyNameAttribute nameAttribute = GetAttribute<BinaryPropertyNameAttribute>(MemberInfo);
            if (nameAttribute != null)
            {
                string name = nameAttribute.Name;
                if (name == null)
                {
                    ThrowHelper.ThrowInvalidOperationException_SerializerPropertyNameNull(ParentClassType, this);
                }

                NameAsString = name;
            }
            else
            {
                NameAsString = MemberInfo.Name;
            }

            Debug.Assert(NameAsString != null);

            NameAsUtf8Bytes = Encoding.UTF8.GetBytes(NameAsString);
        }

        private void DetermineSerializationCapabilities(BinaryIgnoreCondition? ignoreCondition)
        {
            if ((ClassType & (ClassType.Enumerable | ClassType.Dictionary)) == 0)
            {
                Debug.Assert(ignoreCondition != BinaryIgnoreCondition.Always);

                // Three possible values for ignoreCondition:
                // null = BinaryIgnore was not placed on this property, global IgnoreReadOnlyProperties/Fields wins
                // WhenNull = only ignore when null, global IgnoreReadOnlyProperties/Fields loses
                // Never = never ignore (always include), global IgnoreReadOnlyProperties/Fields loses
                bool serializeReadOnlyProperty = ignoreCondition != null || (MemberInfo is PropertyInfo
                    ? !Options.IgnoreReadOnlyProperties
                    : !Options.IgnoreReadOnlyFields);

                // We serialize if there is a getter + not ignoring readonly properties.
                ShouldSerialize = HasGetter && (HasSetter || serializeReadOnlyProperty);

                // We deserialize if there is a setter.
                ShouldDeserialize = HasSetter;
            }
            else
            {
                if (HasGetter)
                {
                    Debug.Assert(ConverterBase != null);

                    ShouldSerialize = true;

                    if (HasSetter)
                    {
                        ShouldDeserialize = true;
                    }
                }
            }
        }

        private void DetermineIgnoreCondition(BinaryIgnoreCondition? ignoreCondition, bool defaultValueIsNull)
        {
            if (ignoreCondition != null)
            {
                Debug.Assert(MemberInfo != null);
                Debug.Assert(ignoreCondition != BinaryIgnoreCondition.Always);

                if (ignoreCondition == BinaryIgnoreCondition.WhenWritingDefault)
                {
                    IgnoreDefaultValuesOnWrite = true;
                }
                else if (ignoreCondition == BinaryIgnoreCondition.WhenWritingNull)
                {
                    if (defaultValueIsNull)
                    {
                        IgnoreDefaultValuesOnWrite = true;
                    }
                    else
                    {
                        ThrowHelper.ThrowInvalidOperationException_IgnoreConditionOnValueTypeInvalid(this);
                    }
                }
            }
            else if (Options.IgnoreNullValues)
            {
                Debug.Assert(Options.DefaultIgnoreCondition == BinaryIgnoreCondition.Never);
                if (defaultValueIsNull)
                {
                    IgnoreDefaultValuesOnRead = true;
                    IgnoreDefaultValuesOnWrite = true;
                }
            }
            else if (Options.DefaultIgnoreCondition == BinaryIgnoreCondition.WhenWritingNull)
            {
                Debug.Assert(!Options.IgnoreNullValues);
                if (defaultValueIsNull)
                {
                    IgnoreDefaultValuesOnWrite = true;
                }
            }
            else if (Options.DefaultIgnoreCondition == BinaryIgnoreCondition.WhenWritingDefault)
            {
                Debug.Assert(!Options.IgnoreNullValues);
                IgnoreDefaultValuesOnWrite = true;
            }
        }

    
        public static TAttribute GetAttribute<TAttribute>(MemberInfo memberInfo) where TAttribute : Attribute
        {
            return (TAttribute)memberInfo.GetCustomAttribute(typeof(TAttribute), inherit: false);
        }

        public abstract bool GetMemberAndWriteBinary(object obj, ref WriteStack state, BinaryWriter writer);
        public abstract bool GetMemberAndWriteBinaryExtensionData(object obj, ref WriteStack state, BinaryWriter writer);

        public abstract object GetValueAsObject(object obj);

        public abstract bool ReadBinaryAsObject(ref ReadStack state, ref BinaryReader reader, out object value);

        public bool HasGetter { get; set; }
        public bool HasSetter { get; set; }

        public virtual void Initialize(
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
            Debug.Assert(converter != null);
            TypeMap = typeMap;
            ParentClassType = parentClassType;
            DeclaredPropertyType = declaredPropertyType;
            RuntimePropertyType = runtimePropertyType;
            ClassType = runtimeClassType;
            MemberInfo = memberInfo;
            ConverterBase = converter;
            Options = options;
            TypeSeq = converter.GetTypeSeq(typeMap, options);
        }

        public bool IgnoreDefaultValuesOnRead { get; private set; }
        public bool IgnoreDefaultValuesOnWrite { get; private set; }

        /// <summary>
        /// True if the corresponding cref="BinaryClassInfo.PropertyInfoForClassInfo"/> is this instance.
        /// </summary>
        public bool IsForClassInfo { get; protected set; }


        public string NameAsString { get; private set; } = null!;

        /// <summary>
        /// Utf8 version of NameAsString.
        /// </summary>
        public byte[] NameAsUtf8Bytes = null!;

        /// <summary>
        /// The escaped name passed to the writer.
        /// </summary>
        public byte[] EscapedNameSection = null!;

        // Options can be referenced here since all BinaryPropertyInfos originate from a BinaryClassInfo that is cached on BinarySerializerOptions.
        protected BinarySerializerOptions Options { get; set; } = null!; // initialized in Init method


        public abstract bool ReadBinaryAndSetMember(object obj, ref ReadStack state, ref BinaryReader reader);

        //public abstract bool ReadBinaryAsObject(ref ReadStack state, ref BinaryReader reader, out object value);

        //public bool ReadBinaryExtensionDataValue(ref ReadStack state, ref BinaryReader reader, out object value)
        //{
        //    //Debug.Assert(this == state.Current.BinaryClassInfo.DataExtensionProperty);

        //    //if (RuntimeClassInfo.ElementType == BinaryClassInfo.ObjectType && reader.TokenType == BinaryTokenType.Null)
        //    //{
        //    //    value = null;
        //    //    return true;
        //    //}

        //    //BinaryConverter<BinaryElement> converter = (BinaryConverter<BinaryElement>)Options.GetConverter(typeof(BinaryElement));
        //    //if (!converter.TryRead(ref reader, typeof(BinaryElement), Options, ref state, out BinaryElement BinaryElement))
        //    //{
        //    //    // BinaryElement is a struct that must be read in full.
        //    //    value = null;
        //    //    return false;
        //    //}

        //    //value = binaryElement;
        //    value = null;
        //    return true;
        //}

        public Type ParentClassType { get; private set; } = null!;

        public MemberInfo MemberInfo { get; private set; }

        public BinaryClassInfo RuntimeClassInfo
        {
            get
            {
                if (_runtimeClassInfo == null)
                {
                    _runtimeClassInfo = Options.GetOrAddClass(RuntimePropertyType!);
                }

                return _runtimeClassInfo;
            }
        }

        public Type RuntimePropertyType { get; private set; }

        public abstract void SetExtensionDictionaryAsObject(ref ReadStack state, object obj, object extensionDict);

        public bool ShouldSerialize { get; private set; }
        public bool ShouldDeserialize { get; internal set; }
        public bool IsIgnored { get; private set; }



    }
}
