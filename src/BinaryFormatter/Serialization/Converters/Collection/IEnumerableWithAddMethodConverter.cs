using System;
using System.Collections;
using System.Diagnostics;

namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{
    internal sealed class IEnumerableWithAddMethodConverter<TCollection>
        : IEnumerableDefaultConverter<TCollection, object>
        where TCollection : IEnumerable
    {
        protected override void Add(in object value, ref ReadStack state)
        {
            var addMethodDelegate = ((Action<TCollection, object>)state.Current.BinaryClassInfo.AddMethodDelegate);
            Debug.Assert(addMethodDelegate != null);
            addMethodDelegate((TCollection)state.Current.ReturnValue!, value);
        }

        protected override void CreateCollection(ref BinaryReader reader, ref ReadStack state, BinarySerializerOptions options, ulong len)
        {
            BinaryClassInfo classInfo = state.Current.BinaryClassInfo;
            BinaryClassInfo.ConstructorDelegate constructorDelegate = classInfo.CreateObject;

            if (constructorDelegate == null)
            {
                ThrowHelper.ThrowNotSupportedException_CannotPopulateCollection(TypeToConvert, ref reader, ref state);
            }

            if (classInfo.AddMethodDelegate == null)
            {
                Action<TCollection, object> addDelegate = options.MemberAccessorStrategy.CreateIEnumerableAddMethod<TCollection, object>();
                if (addDelegate == null)
                {
                    ThrowHelper.ThrowNotSupportedException_CannotPopulateCollection(TypeToConvert, ref reader, ref state);
                }

                classInfo.AddMethodDelegate = addDelegate;
            }

            state.Current.ReturnValue = constructorDelegate();
        }

        protected override long GetLength(TCollection value, BinarySerializerOptions options, ref WriteStack state)
        {
            
            BinaryClassInfo classInfo = state.Current.BinaryClassInfo;
            if (classInfo.CountMethodDelegate == null)
            {
                classInfo.CountMethodDelegate = options.MemberAccessorStrategy.CreateIEnumerableCountMethod<TCollection>();
                if (classInfo.CountMethodDelegate == null)
                {
                    Func<TCollection, int> linqCountDelegate = (collection) =>
                    {
                        var enumerator = value.GetEnumerator();
                        if (!enumerator.MoveNext())
                        {
                            return 0;
                        }
                        int index = 0;
                        do
                        {
                            index++;
                        } while (enumerator.MoveNext());

                        return index;
                    };
                    classInfo.CountMethodDelegate = linqCountDelegate;
                }
            }
            Func<TCollection, int> countDelegate = state.Current.BinaryClassInfo.CountMethodDelegate as Func<TCollection, int>;
            return countDelegate(value);
        }

        protected override bool OnWriteResume(BinaryWriter writer, TCollection value, BinarySerializerOptions options, ref WriteStack state)
        {
            IEnumerator enumerator;
            if (state.Current.CollectionEnumerator == null)
            {
                enumerator = value.GetEnumerator();
                if (!enumerator.MoveNext())
                {
                    return true;
                }
            }
            else
            {
                enumerator = (IEnumerator)state.Current.CollectionEnumerator;
            }

            BinaryConverter<object> converter = GetElementConverter(ref state);
            int index = state.Current.EnumeratorIndex;
            if (!state.SupportContinuation)
            {
                do
                {
                    state.Current.WriteEnumerableIndex(index, writer);
                    object element = enumerator.Current;
                    converter.TryWrite(writer, element, options, ref state);
                    // 列表状态下，每个写每个元素时独立的，故需清理多态属性
                    state.Current.PolymorphicBinaryPropertyInfo = null;
                    index++;
                } while (enumerator.MoveNext());

            }
            else
            {
                do
                {
                    if (ShouldFlush(writer, ref state))
                    {
                        state.Current.CollectionEnumerator = enumerator;
                        return false;
                    }

                    if (!state.Current.ProcessedEnumerableIndex)
                    {
                        state.Current.WriteEnumerableIndex(index, writer);
                        state.Current.ProcessedEnumerableIndex = true;
                    }

                    object element = enumerator.Current;
                    if (!converter.TryWrite(writer, element, options, ref state))
                    {
                        state.Current.CollectionEnumerator = enumerator;
                        state.Current.EnumeratorIndex = index;
                        return false;
                    }

                    // 列表状态下，每个写每个元素时独立的，故需清理多态属性
                    state.Current.PolymorphicBinaryPropertyInfo = null;
                    state.Current.ProcessedEnumerableIndex = false;
                    index++;

                } while (enumerator.MoveNext());

            }

            return true;
        }
    }
}
