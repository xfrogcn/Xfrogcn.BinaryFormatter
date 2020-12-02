using System;

namespace Xfrogcn.BinaryFormatter.Serialization
{
    public partial class BinaryConverter<T>
    {
        internal sealed override bool WriteCoreAsObject(
           BinaryWriter writer,
           object value,
           BinarySerializerOptions options,
           ref WriteStack state)
        {
            // Value types can never have a null except for Nullable<T>.
            if (value == null && IsValueType && Nullable.GetUnderlyingType(TypeToConvert) == null)
            {
                ThrowHelper.ThrowBinaryException_DeserializeUnableToConvertValue(TypeToConvert);
            }

            T actualValue = (T)value!;
            return WriteCore(writer, actualValue, options, ref state);
        }

        internal bool WriteCore(
            BinaryWriter writer,
            in T value,
            BinarySerializerOptions options,
            ref WriteStack state)
        {
            try
            {
                return TryWrite(writer, value, options, ref state);
            }
            catch (InvalidOperationException ex) when (ex.Source == ThrowHelper.ExceptionSourceValueToRethrowAsBinaryException)
            {
                ThrowHelper.ReThrowWithPath(state, ex);
                throw;
            }
            catch (BinaryException ex)
            {
                ThrowHelper.AddBinaryExceptionInformation(state, ex);
                throw;
            }
            catch (NotSupportedException ex)
            {
                // If the message already contains Path, just re-throw. This could occur in serializer re-entry cases.
                // To get proper Path semantics in re-entry cases, APIs that take 'state' need to be used.
                if (ex.Message.Contains(" Path: "))
                {
                    throw;
                }

                ThrowHelper.ThrowNotSupportedException(state, ex);
                return default;
            }
        }
    }
}
