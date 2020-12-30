using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    /// <summary>
    /// 处理List<>或从List<>继承的类型，注意，如果是从List<>继承的自定义类型，则不支持有参数的构造函数
    /// </summary>
    /// <typeparam name="TCollection"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    internal sealed class IListOfTConverter<TCollection, TElement>
        : IEnumerableDefaultConverter<TCollection, TElement>
        where TCollection : IList<TElement>
    {
        protected override void Add(in TElement value, ref ReadStack state)
        {
            ((TCollection)state.Current.ReturnValue!).Add(value);
        }

        protected override void CreateCollection(ref BinaryReader reader, ref ReadStack state, BinarySerializerOptions options, ulong len)
        {
            if (state.Current.BinaryClassInfo.CreateObject == null)
            {
                ThrowHelper.ThrowNotSupportedException_SerializationNotSupported(state.Current.BinaryClassInfo.Type);
            }

            state.Current.ReturnValue = state.Current.BinaryClassInfo.CreateObject();
        }

        protected override long GetLength(TCollection value, BinarySerializerOptions options, ref WriteStack state)
        {
            return value.Count;
        }

        protected override bool OnWriteResume(BinaryWriter writer, TCollection value, BinarySerializerOptions options, ref WriteStack state)
        {
            IList<TElement> list = value;

            // Using an index is 2x faster than using an enumerator.
            int index = state.Current.EnumeratorIndex;
            BinaryConverter<TElement> elementConverter = GetElementConverter(ref state);

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

                    TElement element = list[index];
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

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            base.SetTypeMetadata(typeInfo, typeMap, options);
            Type listType = typeof(TCollection);
            if (listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(List<>))
            {
                typeInfo.Type = TypeEnum.List;
                typeInfo.FullName = null;
            }
            
            
        }
    }
}
