using System.Collections;
using System.Collections.Generic;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class BitArrayConverter : IEnumerableDefaultConverter<BitArray, bool>
    {
        protected override void Add(in bool value, ref ReadStack state)
        {
            ((List<bool>)state.Current.ReturnValue).Add(value);
        }

        protected override void CreateCollection(ref BinaryReader reader, ref ReadStack state, BinarySerializerOptions options, ulong len)
        {
            state.Current.ReturnValue = new List<bool>((int)len);
        }

        protected override long GetLength(BitArray value, BinarySerializerOptions options, ref WriteStack state)
        {
            return value.Length;
        }

        protected override void ConvertCollection(ref ReadStack state, BinarySerializerOptions options)
        {
            List<bool> list = (List<bool>)state.Current.ReturnValue;
            state.Current.ReturnValue = new BitArray(list.ToArray());
        }

        protected override bool OnWriteResume(BinaryWriter writer, BitArray value, BinarySerializerOptions options, ref WriteStack state)
        {
            BitArray list = value;


            int index = state.Current.EnumeratorIndex;
            BinaryConverter<bool> elementConverter = GetElementConverter(ref state);

            if (!state.SupportContinuation)
            {
                for (; index < list.Count; index++)
                {
                    state.Current.WriteEnumerableIndex(index, writer);
                    elementConverter.TryWrite(writer, list[index], options, ref state);
                    state.Current.PolymorphicBinaryPropertyInfo = null;
                }
            }
            else
            {
                for (; index < list.Count; index++)
                {
                    if (!state.Current.ProcessedEnumerableIndex)
                    {
                        state.Current.WriteEnumerableIndex(index, writer);
                        state.Current.ProcessedEnumerableIndex = true;
                    }

                    bool element = list[index];
                    if (!elementConverter.TryWrite(writer, element, options, ref state))
                    {
                        state.Current.EnumeratorIndex = index;
                        return false;
                    }

                    state.Current.PolymorphicBinaryPropertyInfo = null;
                    state.Current.ProcessedEnumerableIndex = false;

                    if (ShouldFlush(writer, ref state))
                    {
                        state.Current.EnumeratorIndex = ++index;
                        return false;
                    }
                }

            }



            return true;
        }
    }
}
