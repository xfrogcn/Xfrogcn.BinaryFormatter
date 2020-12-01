using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xfrogcn.BinaryFormatter
{
    //[DebuggerDisplay("ClassType.{BinaryClassInfo.ClassType}, {BinaryClassInfo.Type.Name}")]
    //internal struct ReadStackFrame
    //{
    //    // Current property values.
    //    public BinaryPropertyInfo? JsonPropertyInfo;
    //    public StackFramePropertyState PropertyState;
    //    public bool UseExtensionProperty;

    //    // Support JSON Path on exceptions and non-string Dictionary keys.
    //    // This is Utf8 since we don't want to convert to string until an exception is thown.
    //    // For dictionary keys we don't want to convert to TKey until we have both key and value when parsing the dictionary elements on stream cases.
    //    public byte[]? JsonPropertyName;
    //    public string? JsonPropertyNameAsString; // This is used for string dictionary keys and re-entry cases that specify a property name.

    //    // Stores the non-string dictionary keys for continuation.
    //    public object? DictionaryKey;

    //    // Validation state.
    //    public int OriginalDepth;
    //    public JsonTokenType OriginalTokenType;

    //    // Current object (POCO or IEnumerable).
    //    public object? ReturnValue; // The current return value used for re-entry.
    //    public BinaryClassInfo BinaryClassInfo;
    //    public StackFrameObjectState ObjectState; // State tracking the current object.

    //    // Validate EndObject token on array with preserve semantics.
    //    public bool ValidateEndTokenOnArray;

    //    // For performance, we order the properties by the first deserialize and PropertyIndex helps find the right slot quicker.
    //    public int PropertyIndex;
    //    public List<PropertyRef>? PropertyRefCache;

    //    // Holds relevant state when deserializing objects with parameterized constructors.
    //    public int CtorArgumentStateIndex;
    //    public ArgumentState? CtorArgumentState;

  
    //    public void EndConstructorParameter()
    //    {
    //        CtorArgumentState!.JsonParameterInfo = null;
    //        JsonPropertyName = null;
    //        PropertyState = StackFramePropertyState.None;
    //    }

    //    public void EndProperty()
    //    {
    //        JsonPropertyInfo = null!;
    //        JsonPropertyName = null;
    //        JsonPropertyNameAsString = null;
    //        PropertyState = StackFramePropertyState.None;
    //        ValidateEndTokenOnArray = false;

    //        // No need to clear these since they are overwritten each time:
    //        //  NumberHandling
    //        //  UseExtensionProperty
    //    }

    //    public void EndElement()
    //    {
    //        JsonPropertyNameAsString = null;
    //        PropertyState = StackFramePropertyState.None;
    //    }

    //    /// <summary>
    //    /// Is the current object a Dictionary.
    //    /// </summary>
    //    public bool IsProcessingDictionary()
    //    {
    //        return (BinaryClassInfo.ClassType & ClassType.Dictionary) != 0;
    //    }

    //    /// <summary>
    //    /// Is the current object an Enumerable.
    //    /// </summary>
    //    public bool IsProcessingEnumerable()
    //    {
    //        return (BinaryClassInfo.ClassType & ClassType.Enumerable) != 0;
    //    }

    //    public void Reset()
    //    {
    //        CtorArgumentStateIndex = 0;
    //        CtorArgumentState = null;
    //        BinaryClassInfo = null!;
    //        ObjectState = StackFrameObjectState.None;
    //        OriginalDepth = 0;
    //        OriginalTokenType = JsonTokenType.None;
    //        PropertyIndex = 0;
    //        PropertyRefCache = null;
    //        ReturnValue = null;

    //        EndProperty();
    //    }
    //}
}
