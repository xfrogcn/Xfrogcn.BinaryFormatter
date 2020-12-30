using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class ArrayConverter<TCollection, TElement>
        : IEnumerableDefaultConverter<TCollection, TElement>
        where TCollection : IEnumerable
    {
        protected override void Add(in TElement value, ref ReadStack state)
        {
            ((List<TElement>)state.Current.ReturnValue!).Add(value);
        }

        protected override void CreateCollection(ref BinaryReader reader, ref ReadStack state, BinarySerializerOptions options, ulong len)
        {
            state.Current.ReturnValue = new List<TElement>((int)len);
        }

        protected override bool OnWriteResume(BinaryWriter writer, TCollection value, BinarySerializerOptions options, ref WriteStack state)
        {
            TElement[] array = (TElement[])(IEnumerable)value;

            int index = state.Current.EnumeratorIndex;

            BinaryConverter<TElement> elementConverter = GetElementConverter(ref state);

            if (!state.SupportContinuation)
            {
                for (; index < array.Length; index++)
                {
                    state.Current.WriteEnumerableIndex(index, writer);
                    elementConverter.TryWrite(writer, array[index], options, ref state);

                    // 列表状态下，每个写每个元素时独立的，故需清理多态属性
                    state.Current.PolymorphicBinaryPropertyInfo = null;
                }
            }
            else
            {
                for (; index < array.Length; index++)
                {
                    if (!state.Current.ProcessedEnumerableIndex)
                    {
                        state.Current.WriteEnumerableIndex(index, writer);
                        state.Current.ProcessedEnumerableIndex = true;
                    }

                    TElement element = array[index];
                    if (!elementConverter.TryWrite(writer, element, options, ref state))
                    {
                        state.Current.EnumeratorIndex = index;
                        return false;
                    }

                    // 列表状态下，每个写每个元素时独立的，故需清理多态属性
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

        protected override void ConvertCollection(ref ReadStack state, BinarySerializerOptions options)
        {
            List<TElement> list = (List<TElement>)state.Current.ReturnValue!;
            state.Current.ReturnValue = list.ToArray();
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            base.SetTypeMetadata(typeInfo, typeMap, options);
            typeInfo.Type = TypeEnum.Array;
            typeInfo.FullName = null;
            typeInfo.GenericArgumentCount = 1;
            typeInfo.IsGeneric = true;
            var converter = options.GetConverter(typeof(TCollection).GetElementType());
            Debug.Assert(converter != null);
            typeInfo.GenericArguments = new ushort[] { converter.GetTypeSeq(typeMap, options) } ;
        }

        protected override long GetLength(TCollection value, BinarySerializerOptions options, ref WriteStack state)
        {
            TElement[] array = (TElement[])(IEnumerable)value;
            return array.LongLength;
        }
    }
}
