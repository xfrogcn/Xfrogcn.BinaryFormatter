using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class KeyValuePairConverter<TKey, TValue> :
        SmallObjectWithParameterizedConstructorConverter<KeyValuePair<TKey, TValue>, TKey, TValue, object, object>
    {
        private const string KeyNameCLR = "Key";
        private const string ValueNameCLR = "Value";

        private const int NumProperties = 2;

        // Property name for "Key" and "Value" with Options.PropertyNamingPolicy applied.
        private string _keyName = null!;
        private string _valueName = null!;

        private static readonly ConstructorInfo s_constructorInfo =
            typeof(KeyValuePair<TKey, TValue>).GetConstructor(new[] { typeof(TKey), typeof(TValue) })!;

        internal override void Initialize(BinarySerializerOptions options)
        {
            _keyName = KeyNameCLR;
            _valueName = ValueNameCLR;

            ConstructorInfo = s_constructorInfo;
            Debug.Assert(ConstructorInfo != null);
        }

        /// <summary>
        /// Lookup the constructor parameter given its name in the reader.
        /// </summary>
        protected override bool TryLookupConstructorParameter(
            ref ReadStack state,
            ref BinaryReader reader,
            BinaryMemberInfo mi,
            BinarySerializerOptions options,
            out BinaryParameterInfo binaryParameterInfo)
        {


            BinaryClassInfo classInfo = state.Current.BinaryClassInfo;
            ArgumentState argState = state.Current.CtorArgumentState;

            Debug.Assert(classInfo.ClassType == ClassType.Object);
            Debug.Assert(argState != null);
            Debug.Assert(_keyName != null);
            Debug.Assert(_valueName != null);

            bool caseInsensitiveMatch = options.PropertyNameCaseInsensitive;

            string propertyName = mi.NameAsString;
            state.Current.BinaryPropertyNameAsString = propertyName;

            if (!argState.FoundKey &&
                FoundKeyProperty(propertyName, caseInsensitiveMatch))
            {
                binaryParameterInfo = classInfo.ParameterCache![_keyName];
                argState.FoundKey = true;
            }
            else if (!argState.FoundValue &&
                FoundValueProperty(propertyName, caseInsensitiveMatch))
            {
                binaryParameterInfo = classInfo.ParameterCache![_valueName];
                argState.FoundValue = true;
            }
            else
            {
                ThrowHelper.ThrowBinaryException();
                binaryParameterInfo = null;
                return false;
            }

            Debug.Assert(binaryParameterInfo != null);
            argState.ParameterIndex++;
            argState.BinaryParameterInfo = binaryParameterInfo;
            return true;
        }

        protected override void EndRead(ref ReadStack state)
        {
            Debug.Assert(state.Current.PropertyIndex == 0);

            if (state.Current.CtorArgumentState!.ParameterIndex != NumProperties)
            {
                ThrowHelper.ThrowBinaryException();
            }
        }

        private bool FoundKeyProperty(string propertyName, bool caseInsensitiveMatch)
        {
            return propertyName == _keyName ||
                (caseInsensitiveMatch && string.Equals(propertyName, _keyName, StringComparison.OrdinalIgnoreCase)) ||
                propertyName == KeyNameCLR;
        }

        private bool FoundValueProperty(string propertyName, bool caseInsensitiveMatch)
        {
            return propertyName == _valueName ||
                (caseInsensitiveMatch && string.Equals(propertyName, _valueName, StringComparison.OrdinalIgnoreCase)) ||
                propertyName == ValueNameCLR;
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            typeInfo.Type = TypeEnum.KeyValuePair;
            typeInfo.SerializeType = ClassType.Object;
        }
    }
}
