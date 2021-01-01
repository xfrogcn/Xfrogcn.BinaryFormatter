using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xfrogcn.BinaryFormatter.Serialization;

namespace Xfrogcn.BinaryFormatter
{
    [DebuggerDisplay("ClassType.{BinaryClassInfo.ClassType}, {BinaryClassInfo.Type.Name}")]
    internal struct WriteStackFrame
    {
        /// <summary>
        /// The enumerator for resumable collections.
        /// </summary>
        public IEnumerator CollectionEnumerator;

        /// <summary>
        /// The original BinaryPropertyInfo that is not changed. It contains all properties.
        /// </summary>
        /// <remarks>
        /// For objects, it is either the actual (real) BinaryPropertyInfo or the <see cref="BinaryClassInfo.PropertyInfoForClassInfo"/> for the class.
        /// For collections, it is the <see cref="BinaryClassInfo.PropertyInfoForClassInfo"/> for the class and current element.
        /// </remarks>
        public BinaryPropertyInfo DeclaredBinaryPropertyInfo;

        /// <summary>
        /// Used when processing extension data dictionaries.
        /// </summary>
        public bool IgnoreDictionaryKeyPolicy;

        /// <summary>
        /// The class (POCO or IEnumerable) that is being populated.
        /// </summary>
        public BinaryClassInfo BinaryClassInfo;

        public BinaryTypeInfo BinaryTypeInfo;

        //public BinaryClassInfo PolymorphicBinaryClassInfo;

        //public BinaryTypeInfo PolymorphicBinaryTypeInfo;

        /// <summary>
        /// Validation state for a class.
        /// </summary>
        public int OriginalDepth;

        // Class-level state for collections.
        public bool ProcessedStartToken;
        public bool ProcessedEndToken;
        public bool ProcessedEnumerableIndex;
        public byte EnumerableIndexBytes;
        public StackFrameWriteObjectState ObjectState;
        /// <summary>
        /// Property or Element state.
        /// </summary>
        public StackFramePropertyState PropertyState;

        /// <summary>
        /// The enumerator index for resumable collections.
        /// </summary>
        public int EnumeratorIndex;

        // This is used for re-entry cases for exception handling.
        public string BinaryPropertyNameAsString;

        // Preserve Reference
        //public MetadataPropertyName MetadataPropertyName;

        /// <summary>
        /// The run-time BinaryPropertyInfo that contains the ClassInfo and ConverterBase for polymorphic scenarios.
        /// </summary>
        /// <remarks>
        /// For objects, it is the <see cref="BinaryClassInfo.PropertyInfoForClassInfo"/> for the class and current property.
        /// For collections, it is the <see cref="BinaryClassInfo.PropertyInfoForClassInfo"/> for the class and current element.
        /// </remarks>
        public BinaryPropertyInfo PolymorphicBinaryPropertyInfo;

       
        public void EndDictionaryElement()
        {
            PropertyState = StackFramePropertyState.None;
            PolymorphicBinaryPropertyInfo = null;
        }

        public void EndProperty()
        {
            DeclaredBinaryPropertyInfo = null!;
            BinaryPropertyNameAsString = null;
            PolymorphicBinaryPropertyInfo = null;
            PropertyState = StackFramePropertyState.None;
        }


        public void WriteEnumerableIndex(long idx, BinaryWriter writer)
        {
            if (EnumerableIndexBytes == 1)
            {
                writer.WriteByteValue((byte)idx);
            }
            else if (EnumerableIndexBytes == 2)
            {
                writer.WriteUInt16Value((ushort)idx);
            }
            else if (EnumerableIndexBytes == 4)
            {
                writer.WriteUInt32Value((UInt32)idx);
            }
            else if (EnumerableIndexBytes == 8)
            {
                writer.WriteUInt64Value((ulong)idx);
            }
            else
            {
                ThrowHelper.ThrowBinaryException();
            }
        } 

        /// <summary>
        /// Return the property that contains the correct polymorphic properties including
        /// the ClassType and ConverterBase.
        /// </summary>
        public BinaryPropertyInfo GetPolymorphicBinaryPropertyInfo()
        {
            return PolymorphicBinaryPropertyInfo ?? DeclaredBinaryPropertyInfo!;
        }

        /// <summary>
        /// Initializes the state for polymorphic or re-entry cases.
        /// </summary>
        public BinaryConverter InitializeReEntry(Type type, BinarySerializerOptions options, string propertyName = null)
        {
            BinaryClassInfo classInfo = options.GetOrAddClass(type);
            

            // Set for exception handling calculation of BinaryPath.
            BinaryPropertyNameAsString = propertyName;

            PolymorphicBinaryPropertyInfo = classInfo.PropertyInfoForClassInfo;
            //PolymorphicBinaryClassInfo = classInfo;
            //PolymorphicBinaryTypeInfo = classInfo.TypeMap.GetTypeInfo(classInfo.TypeSeq);
            return PolymorphicBinaryPropertyInfo.ConverterBase;
        }

        public void Reset()
        {
            CollectionEnumerator = null;
            EnumeratorIndex = 0;
            IgnoreDictionaryKeyPolicy = false;
            BinaryClassInfo = null!;
            EnumerableIndexBytes = 8;
            //PolymorphicBinaryClassInfo = null;
            PolymorphicBinaryPropertyInfo = null;
            OriginalDepth = 0;
            ProcessedStartToken = false;
            ProcessedEndToken = false;
            ProcessedEnumerableIndex = false;
            ObjectState = StackFrameWriteObjectState.None;
            //ProcessedArrayLength = false;

            EndProperty();
        }
    }
}
