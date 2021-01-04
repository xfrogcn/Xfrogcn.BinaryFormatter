using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Xfrogcn.BinaryFormatter.Serialization;

namespace Xfrogcn.BinaryFormatter
{
    [DebuggerDisplay("Path:{BinaryPath()} Current: ClassType.{Current.BinaryClassInfo.ClassType}, {Current.BinaryClassInfo.Type.Name}")]
    internal struct ReadStack
    {
        internal static readonly char[] SpecialCharacters = { '.', ' ', '\'', '/', '"', '[', ']', '(', ')', '\t', '\n', '\r', '\f', '\b', '\\', '\u0085', '\u2028', '\u2029' };

        private int _continuationCount;

        internal byte Version { get; set; }

        internal TypeMap TypeMap { get; set; }

        internal Dictionary<uint, ulong> RefMap { get; set; }

        internal ushort PrimaryTypeSeq { get; set; }

        internal Type PrimaryType { get; set; }

        internal TypeResolver TypeResolver { get; set; }

        internal BinarySerializerOptions Options { get; set; }

        

        public ReadStackFrame Current;

        public bool IsContinuation => _continuationCount != 0;
        public bool IsLastContinuation => _continuationCount == _count;

        private int _count;



        private List<ReadStackFrame> _previous;

        private List<ArgumentState> _ctorArgStateCache;

        /// <summary>
        /// Whether we need to read ahead in the inner read loop.
        /// </summary>
        public bool SupportContinuation;

        /// <summary>
        /// Whether we can read without the need of saving state for stream and preserve references cases.
        /// </summary>
        public bool UseFastPath;

        public long BytesConsumed;

        public ReferenceResolver ReferenceResolver;

        internal void ResolveTypes(BinarySerializerOptions options, Type returnType)
        {
            Options = options;
            if( options.TypeHandler!=null)
            {
                TypeResolver = options.TypeHandler.CreateResolver();
            }
            else
            {
                TypeResolver = TypeHandler.DefaultTypeResolver;
            }

            TypeMap.ResolveTypes(TypeResolver);

            PrimaryType = TypeMap.GetType(PrimaryTypeSeq);


            if (PrimaryType != null)
            {
                if (PrimaryType.IsClass && !returnType.IsAssignableFrom(PrimaryType) )
                {
                    PrimaryType = returnType;
                }
            }
            else
            {
                PrimaryType = returnType;
            }

            if (PrimaryType.IsInterface || PrimaryType.IsAbstract)
            {
                ThrowHelper.ThrowNotSupportedException_SerializationNotSupported(PrimaryType);
            }
        }

        public void Initialize(Type type, BinarySerializerOptions options, bool supportContinuation)
        {
            BinaryClassInfo binaryClassInfo = options.GetOrAddClass(type);
            Current.BinaryClassInfo = binaryClassInfo;

            // The initial BinaryPropertyInfo will be used to obtain the converter.
            Current.BinaryPropertyInfo = binaryClassInfo.PropertyInfoForClassInfo;

            Current.BinaryTypeInfo = TypeMap.GetTypeInfo(type);
            Current.TypeMap = TypeMap;

            if (options.ReferenceHandler != null)
            {
                ReferenceResolver = options.ReferenceHandler!.CreateResolver(writing: false);
            }
            else
            {
                ReferenceResolver = new ObjectReferenceResolver();
            }

            SupportContinuation = supportContinuation;
            UseFastPath = !supportContinuation;
        }


        internal BinaryPropertyInfo LookupProperty(string propertyName)
        {
            var binaryClassInfo = Current.BinaryClassInfo;
            if (binaryClassInfo.PropertyCache.ContainsKey(propertyName))
            {
                return binaryClassInfo.PropertyCache[propertyName];
            }
            return null;
        }

        internal BinaryMemberInfo GetMemberInfo(ushort seq)
        {
            if(Current.BinaryTypeInfo != null && Current.BinaryTypeInfo.MemberInfos!=null &&
                Current.BinaryTypeInfo.MemberInfos.ContainsKey(seq))
            {
                return Current.BinaryTypeInfo.MemberInfos[seq];
            }
            return null;
        }

        private void AddCurrent()
        {
            if (_previous == null)
            {
                _previous = new List<ReadStackFrame>();
            }

            if (_count > _previous.Count)
            {
                // Need to allocate a new array element.
                _previous.Add(Current);
            }
            else
            {
                // Use a previously allocated slot.
                _previous[_count - 1] = Current;
            }

            _count++;
        }

        public void Push(BinaryTypeInfo typeInfo)
        {
            if (_continuationCount == 0)
            {
                if (_count == 0)
                {
                    // The first stack frame is held in Current.
                    Current.BinaryTypeInfo = typeInfo;
                    Current.TypeMap = TypeMap;
                    _count = 1;
                }
                else
                {
                    BinaryClassInfo binaryClassInfo;
                    if (Current.BinaryClassInfo.ClassType == ClassType.Object)
                    {
                        if (Current.BinaryPropertyInfo != null)
                        {
                            binaryClassInfo = Current.PolymorphicBinaryClassInfo?? Current.BinaryPropertyInfo.RuntimeClassInfo;
                        }
                        else
                        {
                            binaryClassInfo = Current.PolymorphicBinaryClassInfo ?? Current.CtorArgumentState!.BinaryParameterInfo!.RuntimeClassInfo;
                        }
                    }
                    else if (((ClassType.Value | ClassType.NewValue) & Current.BinaryClassInfo.ClassType) != 0)
                    {
                        // Although ClassType.Value doesn't push, a custom custom converter may re-enter serialization.
                        binaryClassInfo = Current.BinaryPropertyInfo!.RuntimeClassInfo;
                    }
                    else
                    {
                        Debug.Assert(((ClassType.Enumerable | ClassType.Dictionary) & Current.BinaryClassInfo.ClassType) != 0);
                        binaryClassInfo = Current.PolymorphicBinaryClassInfo ?? Current.BinaryClassInfo.ElementClassInfo!;
                    }

                    AddCurrent();
                    Current.Reset();

                    Current.BinaryClassInfo = binaryClassInfo;
                    Current.BinaryPropertyInfo = binaryClassInfo.PropertyInfoForClassInfo;
                    Current.BinaryTypeInfo = typeInfo;
                    
                    Current.TypeMap = TypeMap;
                }
            }
            else if (_continuationCount == 1)
            {
                // No need for a push since there is only one stack frame.
                Debug.Assert(_count == 1);
                _continuationCount = 0;

            }
            else
            {
                // A continuation; adjust the index.
                Current = _previous[_count - 1];

                // Check if we are done.
                if (_count == _continuationCount)
                {
                    _continuationCount = 0;
                }
                else
                {
                    _count++;
                }
            }

            SetConstructorArgumentState();
        }

        public void Pop(bool success)
        {
            Debug.Assert(_count > 0);

            if (!success)
            {
                // Check if we need to initialize the continuation.
                if (_continuationCount == 0)
                {
                    if (_count == 1)
                    {
                        // No need for a continuation since there is only one stack frame.
                        _continuationCount = 1;
                    }
                    else
                    {
                        AddCurrent();
                        _count--;
                        _continuationCount = _count;
                        _count--;
                        Current = _previous[_count - 1];
                    }

                    return;
                }

                if (_continuationCount == 1)
                {
                    // No need for a pop since there is only one stack frame.
                    Debug.Assert(_count == 1);
                    return;
                }

                // Update the list entry to the current value.
                _previous[_count - 1] = Current;

                Debug.Assert(_count > 0);
            }
            else
            {
                Debug.Assert(_continuationCount == 0);
            }

            if (_count > 1)
            {
                Current = _previous[--_count - 1];
            }

            SetConstructorArgumentState();
        }

        // Return a BinaryPath using simple dot-notation when possible. When special characters are present, bracket-notation is used:
        // $.x.y[0].z
        // $['PropertyName.With.Special.Chars']
        public string BinaryPath()
        {
            StringBuilder sb = new StringBuilder("$");

            // If a continuation, always report back full stack.
            int count = Math.Max(_count, _continuationCount);

            for (int i = 0; i < count - 1; i++)
            {
                AppendStackFrame(sb, _previous[i]);
            }

            if (_continuationCount == 0)
            {
                AppendStackFrame(sb, Current);
            }

            return sb.ToString();

            static void AppendStackFrame(StringBuilder sb, in ReadStackFrame frame)
            {
                // Append the property name.
                string propertyName = GetPropertyName(frame);
                AppendPropertyName(sb, propertyName);

                if (frame.BinaryClassInfo != null && frame.IsProcessingEnumerable())
                {
                    IEnumerable enumerable = (IEnumerable)frame.ReturnValue;
                    if (enumerable == null)
                    {
                        return;
                    }

                    // For continuation scenarios only, before or after all elements are read, the exception is not within the array.
                    //if (frame.ObjectState == StackFrameObjectState.None ||
                    //    frame.ObjectState == StackFrameObjectState.CreatedObject ||
                    //    frame.ObjectState == StackFrameObjectState.ReadElements)
                    //{
                    //    sb.Append('[');
                    //    sb.Append(GetCount(enumerable));
                    //    sb.Append(']');
                    //}
                }
            }

            //static int GetCount(IEnumerable enumerable)
            //{
            //    if (enumerable is ICollection collection)
            //    {
            //        return collection.Count;
            //    }

            //    int count = 0;
            //    IEnumerator enumerator = enumerable.GetEnumerator();
            //    while (enumerator.MoveNext())
            //    {
            //        count++;
            //    }

            //    return count;
            //}

            static void AppendPropertyName(StringBuilder sb, string propertyName)
            {
                if (propertyName != null)
                {
                    if (propertyName.IndexOfAny(SpecialCharacters) != -1)
                    {
                        sb.Append(@"['");
                        sb.Append(propertyName);
                        sb.Append(@"']");
                    }
                    else
                    {
                        sb.Append('.');
                        sb.Append(propertyName);
                    }
                }
            }

            static string GetPropertyName(in ReadStackFrame frame)
            {
                string propertyName = null;

                // Attempt to get the Binary property name from the frame.
                byte[] utf8PropertyName = frame.BinaryPropertyName;
                if (utf8PropertyName == null)
                {
                    if (frame.BinaryPropertyNameAsString != null)
                    {
                        // Attempt to get the Binary property name set manually for dictionary
                        // keys and KeyValuePair property names.
                        propertyName = frame.BinaryPropertyNameAsString;
                    }
                    else
                    {
                        utf8PropertyName = frame.BinaryPropertyInfo.NameAsUtf8Bytes ??
                            frame.CtorArgumentState?.BinaryParameterInfo?.NameAsUtf8Bytes;
                    }
                }

                if (utf8PropertyName != null)
                {
                    propertyName = BinaryReaderHelper.TranscodeHelper(utf8PropertyName);
                }

                return propertyName;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetConstructorArgumentState()
        {
            if (Current.BinaryClassInfo.ParameterCount > 0)
            {
                // A zero index indicates a new stack frame.
                if (Current.CtorArgumentStateIndex == 0)
                {
                    if (_ctorArgStateCache == null)
                    {
                        _ctorArgStateCache = new List<ArgumentState>();
                    }

                    var newState = new ArgumentState();
                    _ctorArgStateCache.Add(newState);

                    (Current.CtorArgumentStateIndex, Current.CtorArgumentState) = (_ctorArgStateCache.Count, newState);
                }
                else
                {
                    Current.CtorArgumentState = _ctorArgStateCache![Current.CtorArgumentStateIndex - 1];
                }
            }
        }
    }
}
