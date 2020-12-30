using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xfrogcn.BinaryFormatter.Serialization;

namespace Xfrogcn.BinaryFormatter
{
   public static partial class BinarySerializer
    {
        private static void WriteCore<TValue>(
            BinaryWriter writer,
            in TValue value,
            Type inputType,
            ref WriteStack state,
            BinarySerializerOptions options)
        {
            Debug.Assert(writer != null);

            if (value != null )
            {
                inputType = value!.GetType();
            }

            BinaryConverter binaryConverter = state.Initialize(inputType, options, supportContinuation: false);

            bool success = WriteCore(binaryConverter, writer, value, options, ref state);
            Debug.Assert(success);
        }


        private static bool WriteCore<TValue>(
            BinaryConverter binaryConverter,
            BinaryWriter writer,
            in TValue value,
            BinarySerializerOptions options,
            ref WriteStack state)
        {
            Debug.Assert(writer != null);

            bool success;

            if (binaryConverter is BinaryConverter<TValue> converter)
            {
                // Call the strongly-typed WriteCore that will not box structs.
                success = converter.WriteCore(writer, value, options, ref state);
            }
            else
            {
                // The non-generic API was called or we have a polymorphic case where TValue is not equal to the T in BinaryConverter<T>.
                success = binaryConverter.WriteCoreAsObject(writer, value, options, ref state);
            }

            writer.Flush();
            return success;
        }
    }
}
