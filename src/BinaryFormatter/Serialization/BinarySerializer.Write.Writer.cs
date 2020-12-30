using System;

namespace Xfrogcn.BinaryFormatter
{
    public static partial class BinarySerializer
    {
        public static void Serialize<TValue>(
            BinaryWriter writer,
            TValue value,
            BinarySerializerOptions options = null)
        {
            Serialize<TValue>(writer, value, typeof(TValue), options);
        }

        private static void Serialize<TValue>(BinaryWriter writer, in TValue value, Type type, BinarySerializerOptions options)
        {
            if (options == null)
            {
                options = BinarySerializerOptions.s_defaultOptions;
            }

            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            WriteCore<TValue>(writer, value, type, options);
        }
    }
}
