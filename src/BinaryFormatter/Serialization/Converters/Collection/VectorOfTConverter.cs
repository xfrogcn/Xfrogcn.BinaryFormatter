﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class VectorOfTConverter<TElement> : IEnumerableDefaultConverter<Vector<TElement>, TElement>
       where TElement: struct
    {
        protected override void Add(in TElement value, ref ReadStack state)
        {
            ((List<TElement>)state.Current.ReturnValue!).Add(value);
        }

        protected override void CreateCollection(ref BinaryReader reader, ref ReadStack state, BinarySerializerOptions options)
        {
            state.Current.ReturnValue = new List<TElement>();
        }

        protected override void ConvertCollection(ref ReadStack state, BinarySerializerOptions options)
        {
            List<TElement> list = (List<TElement>)state.Current.ReturnValue!;
            state.Current.ReturnValue = new Vector<TElement>(list.ToArray());
        }

        protected override long GetLength(Vector<TElement> value, BinarySerializerOptions options, ref WriteStack state)
        {
            return Vector<TElement>.Count;
        }

        protected override bool OnWriteResume(BinaryWriter writer, Vector<TElement> value, BinarySerializerOptions options, ref WriteStack state)
        {
            int index = state.Current.EnumeratorIndex;

            BinaryConverter<TElement> elementConverter = options.GetConverter(typeof(TElement)) as BinaryConverter<TElement>;
            

            for (; index < Vector<TElement>.Count; index++)
            {
                if (!state.Current.ProcessedEnumerableIndex)
                {
                    state.Current.WriteEnumerableIndex(index, writer);
                    state.Current.ProcessedEnumerableIndex = true;
                }

                TElement element = value[index];
                if (!elementConverter.TryWrite(writer, element, options, ref state))
                {
                    state.Current.EnumeratorIndex = index;
                    return false;
                }

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
            typeInfo.Type = TypeEnum.VectorT;
        }
    }
}