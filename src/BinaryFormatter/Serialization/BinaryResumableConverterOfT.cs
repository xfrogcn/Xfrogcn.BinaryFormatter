using System;

namespace Xfrogcn.BinaryFormatter.Serialization
{
    internal abstract class BinaryResumableConverter<T> : BinaryConverter<T>
    {
        public sealed override T Read(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options)
        {
            // Bridge from resumable to value converters.
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            ReadStack state = default;
            state.Initialize(typeToConvert, options, supportContinuation: false);
            TryRead(ref reader, typeToConvert, options, ref state, out ReferenceID refId, out T value);
            return value;
        }

        public sealed override void Write(BinaryWriter writer, T value, BinarySerializerOptions options)
        {
            // Bridge from resumable to value converters.
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            WriteStack state = default;
            Type inputType = value == null ? typeof(T) : value.GetType();
            state.Initialize(inputType, options, supportContinuation: false);
            TryWrite(writer, value, options, ref state);
        }

        public sealed override bool HandleNull => false;
    }
}
