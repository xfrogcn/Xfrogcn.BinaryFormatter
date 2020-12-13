using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xfrogcn.BinaryFormatter.Serialization;

namespace Xfrogcn.BinaryFormatter
{
    [DebuggerDisplay("ClassType.{BinaryClassInfo.ClassType}, {BinaryClassInfo.Type.Name}")]
    internal struct ReadStackFrame
    {
        // Current property values.
        public BinaryPropertyInfo BinaryPropertyInfo;
        public StackFramePropertyState PropertyState;
        public bool UseExtensionProperty;

           public byte[] BinaryPropertyName;
        public string BinaryPropertyNameAsString; 

        // Stores the non-string dictionary keys for continuation.
        public object DictionaryKey;

        // Validation state.
        public int OriginalDepth;
        public BinaryTokenType OriginalTokenType;

        // Current object (POCO or IEnumerable).
        public object ReturnValue; // The current return value used for re-entry.
        public BinaryClassInfo BinaryClassInfo;
        public BinaryTypeInfo BinaryTypeInfo;

        public BinaryClassInfo PolymorphicBinaryClassInfo;
        public BinaryTypeInfo PolymorphicBinaryTypeInfo;

        public StackFrameObjectState ObjectState; // State tracking the current object.

        // Validate EndObject token on array with preserve semantics.
        public bool ValidateEndTokenOnArray;

        // For performance, we order the properties by the first deserialize and PropertyIndex helps find the right slot quicker.
        public int PropertyIndex;
      //  public List<PropertyRef>? PropertyRefCache;

        // Holds relevant state when deserializing objects with parameterized constructors.
        public int CtorArgumentStateIndex;
        //    public ArgumentState? CtorArgumentState;

        // 记录跳转引用前位置
        public ulong OriginPosition;

        


        public void EndConstructorParameter()
        {
        //    CtorArgumentState!.JsonParameterInfo = null;
            BinaryPropertyName = null;
            PropertyState = StackFramePropertyState.None;
        }

        public void EndProperty()
        {
            BinaryPropertyInfo = null!;
            BinaryPropertyName = null;
            BinaryPropertyNameAsString = null;
            PropertyState = StackFramePropertyState.None;
            ValidateEndTokenOnArray = false;

            // No need to clear these since they are overwritten each time:
            //  NumberHandling
            //  UseExtensionProperty
        }

        public void EndElement()
        {
            BinaryPropertyNameAsString = null;
            PropertyState = StackFramePropertyState.None;
        }

        /// <summary>
        /// Is the current object a Dictionary.
        /// </summary>
        public bool IsProcessingDictionary()
        {
            return (BinaryClassInfo.ClassType & ClassType.Dictionary) != 0;
        }

        /// <summary>
        /// Is the current object an Enumerable.
        /// </summary>
        public bool IsProcessingEnumerable()
        {
            return (BinaryClassInfo.ClassType & ClassType.Enumerable) != 0;
        }

        public BinaryConverter InitializeReEntry(Type type, BinarySerializerOptions options, string propertyName = null)
        {
            BinaryClassInfo classInfo = options.GetOrAddClass(type);


            // Set for exception handling calculation of JsonPath.
            BinaryPropertyNameAsString = propertyName;

            PolymorphicBinaryClassInfo = classInfo;
            PolymorphicBinaryTypeInfo = classInfo.TypeMap.GetTypeInfo(classInfo.TypeSeq);
            return classInfo.PropertyInfoForClassInfo.ConverterBase;
        }

        public void Reset()
        {
            CtorArgumentStateIndex = 0;
           // CtorArgumentState = null;
            BinaryClassInfo = null!;
            PolymorphicBinaryClassInfo = null;
            PolymorphicBinaryTypeInfo = null;
            BinaryTypeInfo = null;
            ObjectState = StackFrameObjectState.None;
            OriginalDepth = 0;
         //   OriginalTokenType = JsonTokenType.None;
            PropertyIndex = 0;
            //PropertyRefCache = null;
            ReturnValue = null;

            EndProperty();
        }
    }
}
