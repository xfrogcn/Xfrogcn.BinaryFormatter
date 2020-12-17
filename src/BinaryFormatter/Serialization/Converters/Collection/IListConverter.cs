using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class IListConverter<TCollection>
        : IEnumerableDefaultConverter<TCollection, object>
        where TCollection : IList
    {
        protected override void Add(in object value, ref ReadStack state)
        {
            ((IList)state.Current.ReturnValue!).Add(value);
        }

        protected override void CreateCollection(ref BinaryReader reader, ref ReadStack state, BinarySerializerOptions options)
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
            IList list = value;

            // Using an index is 2x faster than using an enumerator.
            int index = state.Current.EnumeratorIndex;
            BinaryConverter<object> elementConverter = GetElementConverter(ref state);

            for (; index < list.Count; index++)
            {
                if (!state.Current.ProcessedEnumerableIndex)
                {
                    state.Current.WriteEnumerableIndex(index, writer);
                    state.Current.ProcessedEnumerableIndex = true;
                }

                object element = list[index];
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

            return true;
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            typeInfo.SerializeType = ClassType.Enumerable;
            typeInfo.Type = TypeEnum.Class;
            typeInfo.FullName = options.GetTypeFullName(typeof(TCollection));
        }
    }
}
