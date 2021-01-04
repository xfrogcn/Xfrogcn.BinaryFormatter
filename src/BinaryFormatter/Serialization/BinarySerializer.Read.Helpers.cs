using System;
using System.Diagnostics;
using Xfrogcn.BinaryFormatter.Serialization;

namespace Xfrogcn.BinaryFormatter
{
    public static partial class BinarySerializer
    {
       
        private static TValue ReadCore<TValue>(ref BinaryReader reader, Type returnType, ref ReadStack state, BinarySerializerOptions options)
        {
            BinaryConverter binaryConverter = state.Current.BinaryPropertyInfo!.ConverterBase;
            return ReadCore<TValue>(binaryConverter, ref reader, options, ref state);
        }

        private static TValue ReadCore<TValue>(BinaryConverter binaryConverter, ref BinaryReader reader, BinarySerializerOptions options, ref ReadStack state)
        {
            if (binaryConverter is BinaryConverter<TValue> converter)
            {
                // Call the strongly-typed ReadCore that will not box structs.
                return converter.ReadCore(ref reader, options, ref state);
            }

            // The non-generic API was called or we have a polymorphic case where TValue is not equal to the T in BinaryConverter<T>.
            object value = binaryConverter.ReadCoreAsObject(ref reader, options, ref state);
            return (TValue)value!;
        }
    }
}
