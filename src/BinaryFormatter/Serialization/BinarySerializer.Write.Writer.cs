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

        private static void Serialize<TValue>(BinaryWriter writer, in TValue value, Type inputType, BinarySerializerOptions options)
        {
            if (options == null)
            {
                options = BinarySerializerOptions.s_defaultOptions;
            }

            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            // 写入头
            writer.WriteHeader();
            if (value == null)
            {
                writer.Flush();
                return;
            }
            if (value != null)
            {
                inputType = value.GetType();
            }

            WriteStack state = default;
            WriteCore<TValue>(writer, value, inputType, ref state, options);
            writer.WriteMetadata(ref state, inputType);
            writer.Flush();
        }
    }
}
