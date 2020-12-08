using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Xfrogcn.BinaryFormatter
{
    internal struct ReadStack
    {
        internal static readonly char[] SpecialCharacters = { '.', ' ', '\'', '/', '"', '[', ']', '(', ')', '\t', '\n', '\r', '\f', '\b', '\\', '\u0085', '\u2028', '\u2029' };

        private int _continuationCount;

        internal byte Version { get; set; }

        internal TypeMap TypeMap { get; set; }

        internal ushort PrimaryTypeSeq { get; set; }

        internal Type PrimaryType { get; set; }

        internal TypeResolver TypeResolver { get; set; }

        internal BinarySerializerOptions Options { get; set; }

        public ReadStackFrame Current;

        public bool IsContinuation => _continuationCount != 0;
        public bool IsLastContinuation => _continuationCount == _count;

        private int _count;

        private List<ReadStackFrame> _previous;

        /// <summary>
        /// Whether we need to read ahead in the inner read loop.
        /// </summary>
        public bool SupportContinuation;

        /// <summary>
        /// Whether we can read without the need of saving state for stream and preserve references cases.
        /// </summary>
        public bool UseFastPath;

        public long BytesConsumed;

        internal void ResolveTypes(BinarySerializerOptions options)
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
            Debug.Assert(PrimaryType != null);
        }

        public void Initialize(Type type, BinarySerializerOptions options, bool supportContinuation)
        {
            BinaryClassInfo binaryClassInfo = options.GetOrAddClass(type);
            Current.BinaryClassInfo = binaryClassInfo;

            // The initial JsonPropertyInfo will be used to obtain the converter.
            Current.BinaryPropertyInfo = binaryClassInfo.PropertyInfoForClassInfo;

           

            //bool preserveReferences = options.ReferenceHandler != null;
            //if (preserveReferences)
            //{
            //    ReferenceResolver = options.ReferenceHandler!.CreateResolver(writing: false);
            //}

            SupportContinuation = supportContinuation;
           //UseFastPath = !supportContinuation && !preserveReferences;
        }

    }
}
