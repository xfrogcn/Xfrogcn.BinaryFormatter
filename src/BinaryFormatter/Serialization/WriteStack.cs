﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xfrogcn.BinaryFormatter.Serialization;

namespace Xfrogcn.BinaryFormatter
{
    [DebuggerDisplay("Path:{PropertyPath()} Current: ClassType.{Current.BinaryClassInfo.ClassType}, {Current.BinaryClassInfo.Type.Name}")]
    internal struct WriteStack
    {
        /// <summary>
        /// The number of stack frames when the continuation started.
        /// </summary>
        private int _continuationCount;

        /// <summary>
        /// The number of stack frames including Current. _previous will contain _count-1 higher frames.
        /// </summary>
        private int _count;

        private List<WriteStackFrame> _previous;

        // A field is used instead of a property to avoid value semantics.
        public WriteStackFrame Current;

        /// <summary>
        /// The amount of bytes to write before the underlying Stream should be flushed and the
        /// current buffer adjusted to remove the processed bytes.
        /// </summary>
        public int FlushThreshold;

        public bool IsContinuation => _continuationCount != 0;

        public TypeMap TypeMap { get; set; }

        public ushort PrimaryTypeSeq;


        private List<ushort> _typeSeqList;
        public IReadOnlyList<ushort> TypeSeqList => _typeSeqList;
        

        // The bag of preservable references.
        public ReferenceResolver ReferenceResolver;

        /// <summary>
        /// Internal flag to let us know that we need to read ahead in the inner read loop.
        /// </summary>
        public bool SupportContinuation;

        private void AddCurrent()
        {
            if (_previous == null)
            {
                _previous = new List<WriteStackFrame>();
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

        /// <summary>
        /// Initialize the state without delayed initialization of the BinaryClassInfo.
        /// </summary>
        public BinaryConverter Initialize(Type type, BinarySerializerOptions options, bool supportContinuation)
        {
            _typeSeqList = new List<ushort>();

            BinaryClassInfo binaryClassInfo = options.GetOrAddClass(type);

            TypeMap = options.TypeMap;
            AddTypeSeq(binaryClassInfo.TypeSeq);
            PrimaryTypeSeq = binaryClassInfo.TypeSeq;

            Current.BinaryClassInfo = binaryClassInfo;
            Current.BinaryTypeInfo = TypeMap.GetTypeInfo(binaryClassInfo.TypeSeq);
            Current.DeclaredBinaryPropertyInfo = binaryClassInfo.PropertyInfoForClassInfo;

            if (options.ReferenceHandler != null)
            {
                ReferenceResolver = options.ReferenceHandler!.CreateResolver(writing: true);
            }
            else
            {
                ReferenceResolver = new ObjectReferenceResolver();
            }

            SupportContinuation = supportContinuation;

            return binaryClassInfo.PropertyInfoForClassInfo.ConverterBase;
        }


        public void AddTypeSeq(ushort typeSeq)
        {
            if (!_typeSeqList.Contains(typeSeq))
            {
                _typeSeqList.Add(typeSeq);
            }
        }

        public ushort PushType(Type type)
        {
            ushort typeSeq = TypeMap.GetTypeSeq(type);
            AddTypeSeq(typeSeq);

            return typeSeq;
        }

        public List<BinaryTypeInfo> GetTypeList()
        {
            TypeMap tm = TypeMap;
            return _typeSeqList.Select(s => tm.GetTypeInfo(s)).ToList();
        }

        public void Push()
        {
            if (_continuationCount == 0)
            {
                if (_count == 0)
                {
                    // The first stack frame is held in Current.
                    _count = 1;
                }
                else
                {
                    BinaryClassInfo binaryClassInfo = Current.GetPolymorphicBinaryPropertyInfo().RuntimeClassInfo;

                    AddCurrent();
                    Current.Reset();

                    Current.BinaryClassInfo = binaryClassInfo;
                    Current.BinaryTypeInfo = TypeMap.GetTypeInfo(binaryClassInfo.TypeSeq);
                    Current.DeclaredBinaryPropertyInfo = binaryClassInfo.PropertyInfoForClassInfo;
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
                // A continuation, adjust the index.
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
                        _count = 1;
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
        }

        // Return a property path
        // $.x.y.z
        // $['PropertyName.With.Special.Chars']
        public string PropertyPath()
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

            void AppendStackFrame(StringBuilder sb, in WriteStackFrame frame)
            {
                // Append the property name.
                string? propertyName = frame.DeclaredBinaryPropertyInfo?.MemberInfo?.Name;
                if (propertyName == null)
                {
                    propertyName = frame.BinaryPropertyNameAsString;
                }

                AppendPropertyName(sb, propertyName);
            }

            void AppendPropertyName(StringBuilder sb, string? propertyName)
            {
                if (propertyName != null)
                {
                    if (propertyName.IndexOfAny(ReadStack.SpecialCharacters) != -1)
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
        }
    }
}
