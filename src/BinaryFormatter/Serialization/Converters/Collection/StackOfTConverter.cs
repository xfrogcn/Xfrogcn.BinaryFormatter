using System;
using System.Collections.Generic;
using static Xfrogcn.BinaryFormatter.BinaryClassInfo;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class StackOfTConverter<TCollection, TElement>
        : IEnumeratorOfTConverter<TCollection, TElement>
        where TCollection : Stack<TElement>
    {
        public StackOfTConverter()
        {
            ConstructorInfo = typeof(TCollection).GetConstructor(new Type[] { typeof(IEnumerable<TElement>) });
        }

        protected override void Add(in TElement value, ref ReadStack state)
        {
            ((Stack<TElement>)state.Current.ReturnValue!).Push(value);
        }

        protected override void CreateCollection(ref BinaryReader reader, ref ReadStack state, BinarySerializerOptions options, ulong len)
        {
            state.Current.ReturnValue = new Stack<TElement>((int)len);
        }

        protected override void ConvertCollection(ref ReadStack state, BinarySerializerOptions options)
        {

            ParameterizedConstructorDelegate<TCollection> creator = null;
            if (state.Current.BinaryClassInfo.CreateObjectWithArgs == null && ConstructorInfo != null)
            {
                creator = options.MemberAccessorStrategy.CreateParameterizedConstructor<TCollection>(ConstructorInfo);
                state.Current.BinaryClassInfo.CreateObjectWithArgs = creator;
            }
            else if (state.Current.BinaryClassInfo.CreateObjectWithArgs != null)
            {
                creator = (ParameterizedConstructorDelegate<TCollection>)state.Current.BinaryClassInfo.CreateObjectWithArgs;
            }
            else if (state.Current.BinaryClassInfo.CreateObject == null)
            {
                ThrowHelper.ThrowNotSupportedException_SerializationNotSupported(state.Current.BinaryClassInfo.Type);
            }


            Stack<TElement> list = (Stack<TElement>)state.Current.ReturnValue;
            if(creator != null)
            {
                state.Current.ReturnValue = creator(new object[] { list });
            }
            else
            {
                state.Current.ReturnValue = state.Current.BinaryClassInfo.CreateObject();
                
                while (list.Count > 0)
                {
                    ((Stack<TElement>)state.Current.ReturnValue).Push(list.Pop());
                }
            }
        }

        protected override long GetLength(TCollection value, BinarySerializerOptions options, ref WriteStack state)
        {
            return value.Count;
        }

       
    }
}
