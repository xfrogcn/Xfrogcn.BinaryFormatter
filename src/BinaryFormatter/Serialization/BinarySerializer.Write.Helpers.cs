using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xfrogcn.BinaryFormatter.Serialization;

namespace Xfrogcn.BinaryFormatter
{
   public static partial class BinarySerializer
    {
         private static bool WriteCore<TValue>(
            BinaryConverter jsonConverter,
            BinaryWriter writer,
            in TValue value,
            BinarySerializerOptions options,
            ref WriteStack state)
        {
            Debug.Assert(writer != null);

            bool success;

            if (jsonConverter is BinaryConverter<TValue> converter)
            {
                // Call the strongly-typed WriteCore that will not box structs.
                success = converter.WriteCore(writer, value, options, ref state);
            }
            else
            {
                // The non-generic API was called or we have a polymorphic case where TValue is not equal to the T in BinaryConverter<T>.
                success = jsonConverter.WriteCoreAsObject(writer, value, options, ref state);
            }

            writer.Flush();
            return success;
        }
    }
}
