using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Xfrogcn.BinaryFormatter.Serialization
{
    public abstract partial class BinaryConverter<T> : BinaryConverter
    {
        /// <summary>
        /// When overidden, constructs a new <see cref="BinaryConverter{T}"/> instance.
        /// </summary>
        protected internal BinaryConverter()
        {
            // Today only typeof(object) can have polymorphic writes.
            // In the future, this will be check for !IsSealed (and excluding value types).
            CanBePolymorphic = TypeToConvert == BinaryClassInfo.ObjectType;
            IsValueType = TypeToConvert.IsValueType;
            CanBeNull = !IsValueType || TypeToConvert.IsNullableOfT();
            IsInternalConverter = GetType().Assembly == typeof(BinaryConverter).Assembly;

            if (HandleNull)
            {
                HandleNullOnRead = true;
                HandleNullOnWrite = true;
            }

            // For the HandleNull == false case, either:
            // 1) The default values are assigned in this type's virtual HandleNull property
            // or
            // 2) A converter overroad HandleNull and returned false so HandleNullOnRead and HandleNullOnWrite
            // will be their default values of false.

            CanUseDirectReadOrWrite = !CanBePolymorphic && IsInternalConverter && ClassType == ClassType.Value;
        }

        /// <summary>
        /// Determines whether the type can be converted.
        /// </summary>
        /// <remarks>
        /// The default implementation is to return True when <paramref name="typeToConvert"/> equals typeof(T).
        /// </remarks>
        /// <param name="typeToConvert"></param>
        /// <returns>True if the type can be converted, False otherwise.</returns>
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(T);
        }

        internal override ClassType ClassType => ClassType.Value;

        internal sealed override BinaryPropertyInfo CreateBinaryPropertyInfo()
        {
            return new BinaryPropertyInfo<T>();
        }

        internal override sealed BinaryParameterInfo CreateBinaryParameterInfo()
        {
            return new BinaryParameterInfo<T>();
        }

        internal override Type ElementType => null;

        /// <summary>
        /// Indicates whether <see langword="null"/> should be passed to the converter on serialization,
        /// and whether <see cref="BinaryTokenType.Null"/> should be passed on deserialization.
        /// </summary>
        /// <remarks>
        /// The default value is <see langword="true"/> for converters for value types, and <see langword="false"/> for converters for reference types.
        /// </remarks>
        public virtual bool HandleNull
        {
            get
            {
                HandleNullOnRead = !CanBeNull;

                HandleNullOnWrite = false;

                return false;
            }
        }

        /// <summary>
        /// Does the converter want to be called when reading null tokens.
        /// </summary>
        internal bool HandleNullOnRead { get; private set; }

        /// <summary>
        /// Does the converter want to be called for null values.
        /// </summary>
        internal bool HandleNullOnWrite { get; private set; }

        /// <summary>
        /// Can <see langword="null"/> be assigned to <see cref="TypeToConvert"/>?
        /// </summary>
        internal bool CanBeNull { get; }

        // This non-generic API is sealed as it just forwards to the generic version.
        internal sealed override bool TryWriteAsObject(BinaryWriter writer, object value, BinarySerializerOptions options, ref WriteStack state)
        {
            T valueOfT = (T)value!;
            return TryWrite(writer, valueOfT, options, ref state);
        }

        // Provide a default implementation for value converters.
        internal virtual bool OnTryWrite(BinaryWriter writer, T value, BinarySerializerOptions options, ref WriteStack state)
        {
            Write(writer, value, options);
            return true;
        }

        // Provide a default implementation for value converters.
        //internal virtual bool OnTryRead(ref Utf8BinaryReader reader, Type typeToConvert, BinarySerializerOptions options, ref ReadStack state, out T? value)
        //{
        //    value = Read(ref reader, typeToConvert, options);
        //    return true;
        //}

        /// <summary>
        /// Read and convert the Binary to T.
        /// </summary>
        /// <remarks>
        /// A converter may throw any Exception, but should throw <cref>BinaryException</cref> when the Binary is invalid.
        /// </remarks>
        /// <param name="reader">The <see cref="Utf8BinaryReader"/> to read from.</param>
        /// <param name="typeToConvert">The <see cref="Type"/> being converted.</param>
        /// <param name="options">The <see cref="BinarySerializerOptions"/> being used.</param>
        /// <returns>The value that was converted.</returns>
        public abstract T Read(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options);

        internal bool TryRead(ref BinaryReader reader, Type typeToConvert, BinarySerializerOptions options, ref ReadStack state, out T value)
        {
            value = default;
            return true;
//            if (ClassType == ClassType.Value)
//            {
//                // A value converter should never be within a continuation.
//                Debug.Assert(!state.IsContinuation);

//                // For perf and converter simplicity, handle null here instead of forwarding to the converter.
//                if (reader.TokenType == BinaryTokenType.Null && !HandleNullOnRead)
//                {
//                    if (!CanBeNull)
//                    {
//                        ThrowHelper.ThrowBinaryException_DeserializeUnableToConvertValue(TypeToConvert);
//                    }

//                    value = default;
//                    return true;
//                }

//#if !DEBUG
//                        // For performance, only perform validation on internal converters on debug builds.
//                        if (IsInternalConverter)
//                        {
//                            if (state.Current.NumberHandling != null)
//                            {
//                                value = ReadNumberWithCustomHandling(ref reader, state.Current.NumberHandling.Value);
//                            }
//                            else
//                            {
//                                value = Read(ref reader, typeToConvert, options);
//                            }
//                        }
//                        else
//#endif
//                {
//                    BinaryTokenType originalPropertyTokenType = reader.TokenType;
//                    int originalPropertyDepth = reader.CurrentDepth;
//                    long originalPropertyBytesConsumed = reader.BytesConsumed;

//                    if (state.Current.NumberHandling != null)
//                    {
//                        value = ReadNumberWithCustomHandling(ref reader, state.Current.NumberHandling.Value);
//                    }
//                    else
//                    {
//                        value = Read(ref reader, typeToConvert, options);
//                    }

//                    VerifyRead(
//                        originalPropertyTokenType,
//                        originalPropertyDepth,
//                        originalPropertyBytesConsumed,
//                        isValueConverter: true,
//                        ref reader);
//                }

//                if (CanBePolymorphic && options.ReferenceHandler != null && value is BinaryElement element)
//                {
//                    // Edge case where we want to lookup for a reference when parsing into typeof(object)
//                    // instead of return `value` as a BinaryElement.
//                    Debug.Assert(TypeToConvert == typeof(object));

//                    if (BinarySerializer.TryGetReferenceFromBinaryElement(ref state, element, out object? referenceValue))
//                    {
//                        value = (T?)referenceValue;
//                    }
//                }

//                return true;
//            }

//            bool success;

//            // Remember if we were a continuation here since Push() may affect IsContinuation.
//            bool wasContinuation = state.IsContinuation;

//            state.Push();

//#if !DEBUG
//                    // For performance, only perform validation on internal converters on debug builds.
//                    if (IsInternalConverter)
//                    {
//                        if (reader.TokenType == BinaryTokenType.Null && !HandleNullOnRead && !wasContinuation)
//                        {
//                            if (!CanBeNull)
//                            {
//                                ThrowHelper.ThrowBinaryException_DeserializeUnableToConvertValue(TypeToConvert);
//                            }

//                            // For perf and converter simplicity, handle null here instead of forwarding to the converter.
//                            value = default;
//                            success = true;
//                        }
//                        else
//                        {
//                            success = OnTryRead(ref reader, typeToConvert, options, ref state, out value);
//                        }
//                    }
//                    else
//#endif
//            {
//                if (!wasContinuation)
//                {
//                    // For perf and converter simplicity, handle null here instead of forwarding to the converter.
//                    if (reader.TokenType == BinaryTokenType.Null && !HandleNullOnRead)
//                    {
//                        if (!CanBeNull)
//                        {
//                            ThrowHelper.ThrowBinaryException_DeserializeUnableToConvertValue(TypeToConvert);
//                        }

//                        value = default;
//                        state.Pop(true);
//                        return true;
//                    }

//                    Debug.Assert(state.Current.OriginalTokenType == BinaryTokenType.None);
//                    state.Current.OriginalTokenType = reader.TokenType;

//                    Debug.Assert(state.Current.OriginalDepth == 0);
//                    state.Current.OriginalDepth = reader.CurrentDepth;
//                }

//                success = OnTryRead(ref reader, typeToConvert, options, ref state, out value);
//                if (success)
//                {
//                    if (state.IsContinuation)
//                    {
//                        // The resumable converter did not forward to the next converter that previously returned false.
//                        ThrowHelper.ThrowBinaryException_SerializationConverterRead(this);
//                    }

//                    VerifyRead(
//                        state.Current.OriginalTokenType,
//                        state.Current.OriginalDepth,
//                        bytesConsumed: 0,
//                        isValueConverter: false,
//                        ref reader);

//                    // No need to clear state.Current.* since a stack pop will occur.
//                }
//            }

//            state.Pop(success);
//            return success;
        }

        internal override sealed bool TryReadAsObject(ref BinaryReader reader, BinarySerializerOptions options, ref ReadStack state, out object value)
        {
            bool success = TryRead(ref reader, TypeToConvert, options, ref state, out T typedValue);
            value = typedValue;
            return success;
        }

        internal bool TryWrite(BinaryWriter writer, in T value, BinarySerializerOptions options, ref WriteStack state)
        {
            if (writer.CurrentDepth >= options.EffectiveMaxDepth)
            {
                ThrowHelper.ThrowBinaryException_SerializerCycleDetected(options.EffectiveMaxDepth);
            }

            
            ushort typeSeq = state.PushType(value==null? typeof(T) : value.GetType());
            writer.WriteTypeSeq(typeSeq);

            if (CanBePolymorphic)
            {
                if (value == null)
                {
                    if (!HandleNullOnWrite)
                    {
                        writer.WriteNullValue();
                    }
                    else
                    {
                        Debug.Assert(ClassType == ClassType.Value);
                        Debug.Assert(!state.IsContinuation);

                        int originalPropertyDepth = writer.CurrentDepth;
                        Write(writer, value, options);
                        VerifyWrite(originalPropertyDepth, writer);
                    }

                    return true;
                }

                Type type = value.GetType();
                if (type == BinaryClassInfo.ObjectType)
                {
                    //writer.WriteStartObject();
                    //writer.WriteEndObject();
                    return true;
                }

                if (type != TypeToConvert && IsInternalConverter)
                {
                    // For internal converter only: Handle polymorphic case and get the new converter.
                    // Custom converter, even though polymorphic converter, get called for reading AND writing.
                    BinaryConverter BinaryConverter = state.Current.InitializeReEntry(type, options);
                    if (BinaryConverter != this)
                    {
                        // We found a different converter; forward to that.
                        return BinaryConverter.TryWriteAsObject(writer, value, options, ref state);
                    }
                }
            }
            else if (value == null && !HandleNullOnWrite)
            {
                // We do not pass null values to converters unless HandleNullOnWrite is true. Null values for properties were
                // already handled in GetMemberAndWriteBinary() so we don't need to check for IgnoreNullValues here.
                writer.WriteNullValue();
                return true;
            }

            if (ClassType == ClassType.Value)
            {
                Debug.Assert(!state.IsContinuation);

                int originalPropertyDepth = writer.CurrentDepth;
                Write(writer, value, options);
                VerifyWrite(originalPropertyDepth, writer);
                return true;
            }

            bool isContinuation = state.IsContinuation;

            state.Push();

            if (!isContinuation)
            {
                Debug.Assert(state.Current.OriginalDepth == 0);
                state.Current.OriginalDepth = writer.CurrentDepth;
            }

            bool success = OnTryWrite(writer, value, options, ref state);
            if (success)
            {
                VerifyWrite(state.Current.OriginalDepth, writer);
                // No need to clear state.Current.OriginalDepth since a stack pop will occur.
            }

            state.Pop(success);

            return success;
        }

        internal bool TryWriteDataExtensionProperty(BinaryWriter writer, T value, BinarySerializerOptions options, ref WriteStack state)
        {
            Debug.Assert(value != null);

            if (!IsInternalConverter)
            {
                return TryWrite(writer, value, options, ref state);
            }

            //Debug.Assert(this is BinaryDictionaryConverter<T>);

            //if (writer.CurrentDepth >= options.EffectiveMaxDepth)
            //{
            //    ThrowHelper.ThrowBinaryException_SerializerCycleDetected(options.EffectiveMaxDepth);
            //}

            //BinaryDictionaryConverter<T> dictionaryConverter = (BinaryDictionaryConverter<T>)this;

            //bool isContinuation = state.IsContinuation;
            //bool success;

            //state.Push();

            //if (!isContinuation)
            //{
            //    Debug.Assert(state.Current.OriginalDepth == 0);
            //    state.Current.OriginalDepth = writer.CurrentDepth;
            //}

            //// Ignore the naming policy for extension data.
            //state.Current.IgnoreDictionaryKeyPolicy = true;

            //success = dictionaryConverter.OnWriteResume(writer, value, options, ref state);
            //if (success)
            //{
            //    VerifyWrite(state.Current.OriginalDepth, writer);
            //}

            //state.Pop(success);

            //return success;
            return true;
        }

        internal sealed override Type TypeToConvert => typeof(T);

        //internal void VerifyRead(BinaryTokenType tokenType, int depth, long bytesConsumed, bool isValueConverter, ref Utf8BinaryReader reader)
        //{
        //    switch (tokenType)
        //    {
        //        case BinaryTokenType.StartArray:
        //            if (reader.TokenType != BinaryTokenType.EndArray)
        //            {
        //                ThrowHelper.ThrowBinaryException_SerializationConverterRead(this);
        //            }
        //            else if (depth != reader.CurrentDepth)
        //            {
        //                ThrowHelper.ThrowBinaryException_SerializationConverterRead(this);
        //            }

        //            break;

        //        case BinaryTokenType.StartObject:
        //            if (reader.TokenType != BinaryTokenType.EndObject)
        //            {
        //                ThrowHelper.ThrowBinaryException_SerializationConverterRead(this);
        //            }
        //            else if (depth != reader.CurrentDepth)
        //            {
        //                ThrowHelper.ThrowBinaryException_SerializationConverterRead(this);
        //            }

        //            break;

        //        default:
        //            // A non-value converter (object or collection) should always have Start and End tokens.
        //            // A value converter should not make any reads.
        //            if (!isValueConverter || reader.BytesConsumed != bytesConsumed)
        //            {
        //                ThrowHelper.ThrowBinaryException_SerializationConverterRead(this);
        //            }

        //            // Should not be possible to change token type.
        //            Debug.Assert(reader.TokenType == tokenType);

        //            break;
        //    }
        //}

        internal void VerifyWrite(int originalDepth, BinaryWriter writer)
        {
            //if (originalDepth != writer.CurrentDepth)
            //{
            //    ThrowHelper.ThrowBinaryException_SerializationConverterWrite(this);
            //}
        }

        /// <summary>
        /// Write the value as Binary.
        /// </summary>
        /// <remarks>
        /// A converter may throw any Exception, but should throw <cref>BinaryException</cref> when the Binary
        /// cannot be created.
        /// </remarks>
        /// <param name="writer">The <see cref="Utf8BinaryWriter"/> to write to.</param>
        /// <param name="value">The value to convert.</param>
        /// <param name="options">The <see cref="BinarySerializerOptions"/> being used.</param>
        public abstract void Write(BinaryWriter writer, T value, BinarySerializerOptions options);

        internal virtual T ReadWithQuotes(ref BinaryReader reader)
            => throw new InvalidOperationException();

        internal virtual void WriteWithQuotes(BinaryWriter writer, [DisallowNull] T value, BinarySerializerOptions options, ref WriteStack state)
            => throw new InvalidOperationException();

        internal sealed override void WriteWithQuotesAsObject(BinaryWriter writer, object value, BinarySerializerOptions options, ref WriteStack state)
            => WriteWithQuotes(writer, (T)value, options, ref state);

        //internal virtual T ReadNumberWithCustomHandling(ref Utf8BinaryReader reader, BinaryNumberHandling handling)
        //    => throw new InvalidOperationException();

        //internal virtual void WriteNumberWithCustomHandling(Utf8BinaryWriter writer, T value, BinaryNumberHandling handling)
        //    => throw new InvalidOperationException();
    }
}
