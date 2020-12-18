using System;
using System.Collections.Generic;
using System.Linq;


namespace Xfrogcn.BinaryFormatter.Serialization.Converters
{

    internal sealed class ImmutableEnumerableOfTConverter<TCollection, TElement>
        : IEnumerableDefaultConverter<TCollection, TElement>
        where TCollection : IEnumerable<TElement>
    {
        

        private delegate void AddItemProc(in TElement value, ref ReadStack state);

        private AddItemProc _addProc;

        public ImmutableEnumerableOfTConverter()
        {
            if (typeof(TCollection).IsImmutableStackType())
            {
                _addProc = AddStackItem;
            }
            else
            {
                _addProc = AddItem;
            }
        }
        private void AddItem(in TElement value, ref ReadStack state)
        {
            ((List<TElement>)state.Current.ReturnValue!).Add(value);
        }

        private void AddStackItem(in TElement value, ref ReadStack state)
        {
            ((List<TElement>)state.Current.ReturnValue!).Insert(0,value);
        }

        protected override void Add(in TElement value, ref ReadStack state)
        {
            _addProc(value, ref state);
        }

        protected override void CreateCollection(ref BinaryReader reader, ref ReadStack state, BinarySerializerOptions options)
        {
            state.Current.ReturnValue = new List<TElement>();
        }

        protected override long GetLength(TCollection value, BinarySerializerOptions options, ref WriteStack state)
        {
            
            if (value is ICollection<TElement> collection)
            {
                //ICollection  ImmutableArray<T>  ImmutableList<T>  ImmutableHashSet<T> ImmutableSortedSet<T>
                return collection.Count;
            }
            else if (value is IEnumerable<TElement> enumerable)
            {
                // IEnumerable<T> ImmutableQueue<T> ImmutableStack<T>
                return enumerable.Count();
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException_SerializationNotSupported(typeof(TCollection));
            }
            
            return 0;
        }

        protected override void ConvertCollection(ref ReadStack state, BinarySerializerOptions options)
        {
            // 目前仅支持内置类型，不支持直接从接口实现的自定义类
            // IImmutableList<T>
            //      System.Collections.Immutable.ImmutableArray<T>
            //      System.Collections.Immutable.ImmutableList<T>
            // IImmutableQueue<T>
            //      System.Collections.Immutable.ImmutableQueue<T>
            // IImmutableSet<T>
            //      System.Collections.Immutable.ImmutableHashSet<T>
            //      System.Collections.Immutable.ImmutableSortedSet<T>
            // IImmutableStack<T>
            //      System.Collections.Immutable.ImmutableStack<T>
            BinaryClassInfo classInfo = state.Current.BinaryClassInfo;

            Func<IEnumerable<TElement>, TCollection> creator = (Func<IEnumerable<TElement>, TCollection>)classInfo.CreateObjectWithArgs;
            if (creator == null)
            {
                creator = options.MemberAccessorStrategy.CreateImmutableEnumerableCreateRangeDelegate<TElement, TCollection>();
                classInfo.CreateObjectWithArgs = creator;
            }

            state.Current.ReturnValue = creator((List<TElement>)state.Current.ReturnValue!);
        }

        protected override bool OnWriteResume(BinaryWriter writer, TCollection value, BinarySerializerOptions options, ref WriteStack state)
        {
            IEnumerator<TElement> enumerator;
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
                enumerator = (IEnumerator<TElement>)state.Current.CollectionEnumerator;
            }

            BinaryConverter<TElement> converter = GetElementConverter(ref state);
            int index = state.Current.EnumeratorIndex;
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


                TElement element = enumerator.Current;
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

            return true;
        }

        public override void SetTypeMetadata(BinaryTypeInfo typeInfo, TypeMap typeMap, BinarySerializerOptions options)
        {
            typeInfo.Type = TypeEnum.Class;
            typeInfo.SerializeType = ClassType.Enumerable;
            typeInfo.FullName = options.GetTypeFullName(typeof(TCollection));
        }
    }
}
