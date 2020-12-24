using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Xfrogcn.BinaryFormatter.Serialization;

namespace Xfrogcn.BinaryFormatter
{
    internal abstract class BinaryParameterInfo
    {

        public BinaryConverter ConverterBase { get; private set; } = null!;

        // The default value of the parameter. This is `DefaultValue` of the `ParameterInfo`, if specified, or the CLR `default` for the `ParameterType`.
        public object DefaultValue { get; protected set; }

        public bool IgnoreDefaultValuesOnRead { get; private set; }

        // Options can be referenced here since all BinaryPropertyInfos originate from a BinaryClassInfo that is cached on BinarySerializerOptions.
        public BinarySerializerOptions Options { get; set; } // initialized in Init method

        public TypeMap TypeMap { get; set; }

        // The name of the parameter as UTF-8 bytes.
        public byte[] NameAsUtf8Bytes { get; private set; } = null!;

        public string NameAsString { get; internal set; }

        // The zero-based position of the parameter in the formal parameter list.
        public int Position { get; private set; }

        private BinaryClassInfo _runtimeClassInfo;
        public BinaryClassInfo RuntimeClassInfo
        {
            get
            {
                Debug.Assert(ShouldDeserialize);
                if (_runtimeClassInfo == null)
                {
                    Debug.Assert(Options != null);
                    _runtimeClassInfo = Options!.GetOrAddClass(RuntimePropertyType);
                }

                return _runtimeClassInfo;
            }
        }

        internal Type RuntimePropertyType { get; set; } = null!;

        public bool ShouldDeserialize { get; private set; }

        public virtual void Initialize(
            TypeMap typeMap,
            Type runtimePropertyType,
            ParameterInfo parameterInfo,
            BinaryPropertyInfo matchingProperty,
            BinarySerializerOptions options)
        {
            TypeMap = typeMap;
            RuntimePropertyType = runtimePropertyType;
            Position = parameterInfo?.Position ?? 0;
            NameAsUtf8Bytes = matchingProperty.NameAsUtf8Bytes!;
            Options = options;
            ShouldDeserialize = true;
            ConverterBase = matchingProperty.ConverterBase;
            IgnoreDefaultValuesOnRead = matchingProperty.IgnoreDefaultValuesOnRead;
        }

        // Create a parameter that is ignored at run-time. It uses the same type (typeof(sbyte)) to help
        // prevent issues with unsupported types and helps ensure we don't accidently (de)serialize it.
        public static BinaryParameterInfo CreateIgnoredParameterPlaceholder(BinaryPropertyInfo matchingProperty)
        {
            return new BinaryParameterInfo<sbyte>
            {
                RuntimePropertyType = typeof(sbyte),
                NameAsUtf8Bytes = matchingProperty.NameAsUtf8Bytes!,
                TypeMap = matchingProperty.TypeMap
            };
        }
    }
}
